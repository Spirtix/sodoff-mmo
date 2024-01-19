using System.Globalization;
using sodoffmmo.Data;

namespace sodoffmmo.Core;
class WorldEvent {
    private static WorldEvent _instance = null;
    private object EventLock = new object();
    private Random random = new Random();
    
    public static WorldEvent Get() {
        if (_instance == null) {
            _instance = new WorldEvent();
        }
        return _instance;
    }
    
    private WorldEvent() {
        Reset(0.5f);
    }
    
    private string uid;
    private Room room;
    private DateTime startTime;
    private DateTime endTime;
    private string startTimeString;
    private Dictionary<string, float> health;
    private Dictionary<Client, string> players;
    private Client operatorAI;
    private DateTime AITime;
    private bool endTimeIsSet;
    
    private void Reset(float time) {
        startTime = DateTime.UtcNow.AddMinutes(time);
        startTimeString = startTime.ToString("MM/dd/yyyy HH:mm:ss");
        uid = Path.GetRandomFileName().Substring(0, 8); // this is used as RandomSeed for random select ship variant
        room = Room.GetOrAdd("HubTrainingDO");
        endTime = startTime.AddMinutes(10);
        endTimeIsSet = false;
        operatorAI = null;
        health = new();
        players = new();
    }
    
    private void InitEvent() {
        lock (EventLock) {
            if (operatorAI is null || AITime < DateTime.UtcNow) {
                var clients = room.Clients.ToList();
                operatorAI = clients[random.Next(0, clients.Count)];
                AITime = DateTime.UtcNow.AddSeconds(3.5);
            } else {
                return;
            }
        }
        operatorAI.Send(Utils.VlNetworkPacket("WE__AI", operatorAI.PlayerData.Uid));
        Console.WriteLine($"Event AI operator: {operatorAI.PlayerData.Uid}");
    }
    
    private bool EndEvent(bool force = false) {
        bool results = true;
        string targets = "";
        foreach (var x in health) {
            results = results && (x.Value == 0.0f);
            targets += x.Key + ":" + x.Value.ToString("0.0#####", CultureInfo.GetCultureInfo("en-US")) + ",";
        }
        if (results || force) {
            string scores = "";
            foreach (var x in players) {
                scores += x.Value + ",";
            }
            
            NetworkPacket packet = Utils.VlNetworkPacket(
                "WE_ScoutAttack_End",
                $"{uid};{results};{scores};{targets}"
            );
            foreach (var roomClient in room.Clients) {
                roomClient.Send(packet);
            }
            
            NetworkObject wedata = new();
            NetworkArray vl = new();
            NetworkArray vl1 = new();
            vl1.Add("WE__AI");
            vl1.Add((Byte)0);
            vl1.Add("");
            vl1.Add(false);
            vl1.Add(false);
            vl.Add(vl1);
            foreach (var t in health) {
                NetworkArray vl2 = new();
                vl2.Add("WEH_" + t.Key);
                vl2.Add((Byte)0);
                vl2.Add("");
                vl2.Add(false);
                vl2.Add(false);
                vl.Add(vl2);
                NetworkArray vl3 = new();
                vl3.Add("WEF_" + t.Key);
                vl3.Add((Byte)0);
                vl3.Add("");
                vl3.Add(false);
                vl3.Add(false);
                vl.Add(vl3);
            }
            wedata.Add("r", room.Id);
            wedata.Add("vl", vl);
            packet = NetworkObject.WrapObject(0, 11, wedata).Serialize();
            
            Reset(5);
            return true;
        }
        return false;
    }
    
    public string EventInfo() {
        return startTimeString + "," + uid + ", false, HubTrainingDO";
    }
    
    public string EventInfoNext() {
        return startTimeString; // TODO on og this was different time (real next event?)
    }
    
    public string GetUid() => uid;
    
    public Room GetRoom() => room;
    
    public float UpdateHealth(string targetUid, float updateVal) {
        InitEvent(); // TODO better place for this
        
        if (!health.ContainsKey(targetUid))
            health.Add(targetUid, 1.0f);
        health[targetUid] -= updateVal;
        
        if (health[targetUid] < 0) {
            health[targetUid] = 0.0f;
            if (EndEvent())
                return -1.0f;
        }
        
        if (endTime < DateTime.UtcNow) {
            EndEvent(true);
            return -1.0f;
        }
        
        return health[targetUid];
    }
    
    public void UpdateScore(Client client, string value) {
        if (!players.ContainsKey(client)) {
            players.Add(client, value);
        } else {
            players[client] = value;
        }
    }
    
    public void UpdateAI(Client client) {
        if (client == operatorAI)
            AITime = DateTime.UtcNow.AddSeconds(7);
    }

    public void SetTimeSpan(Client client, float seconds) {
        if (client == operatorAI || !endTimeIsSet) {
            endTime = startTime.AddSeconds(seconds);
            endTimeIsSet = true;
        }
    }
    
    public float GetHealth(string targetUid) => health[targetUid];
}
