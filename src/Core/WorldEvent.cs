using System.Globalization;
using sodoffmmo.Data;
using System.Timers;

namespace sodoffmmo.Core;
class WorldEvent {
    enum State {
        Active,
        End,
        NotActive
    }
    private static WorldEvent _instance = null;
    private object EventLock = new object();
    private Random random = new Random();
    private System.Timers.Timer timer;
    
    public static WorldEvent Get() {
        if (_instance == null) {
            _instance = new WorldEvent();
        }
        return _instance;
    }
    
    private WorldEvent() {
        Reset(1.3f);
    }
    
    private Room room;
    private string uid;
    private Client operatorAI;
    private State state;
    
    private DateTime startTime;
    private DateTime endTime;
    private string startTimeString;
    private Dictionary<string, float> health;
    private Dictionary<Client, string> players;
    private DateTime AITime;
    private bool endTimeIsSet;
    
    private void Reset(float time) {
        lock (EventLock) {
            room = Room.GetOrAdd("HubTrainingDO");
            uid = Path.GetRandomFileName().Substring(0, 8); // this is used as RandomSeed for random select ship variant
            operatorAI = null;
            state = State.NotActive;
            
            startTime = DateTime.UtcNow.AddMinutes(time);
            startTimeString = startTime.ToString("MM/dd/yyyy HH:mm:ss");
            AITime = startTime.AddMinutes(-1);
            endTime = startTime.AddMinutes(10);
            endTimeIsSet = false;
            
            Console.WriteLine($"Event {uid} start time: {startTimeString}");
        }
    }
    
    private void InitEvent() {
        lock (EventLock) {
            if (AITime < DateTime.UtcNow) {
                var clients = room.Clients.ToList();
                operatorAI = clients[random.Next(0, clients.Count)];
                AITime = DateTime.UtcNow.AddSeconds(3.5);
                
                if (state == State.NotActive) {
                    // clear here because after Reset() we can get late packages about previous events
                    health = new();
                    players = new();
                    state = State.Active;
                }
                
                operatorAI.Send(Utils.VlNetworkPacket("WE__AI", operatorAI.PlayerData.Uid));
                Console.WriteLine($"Event {uid} AI operator: {operatorAI.PlayerData.Uid}");
            }
        }
    }
    
    private bool EndEvent(bool force = false) {
        bool results = true;
        string targets = "";
        foreach (var x in health) {
            results = results && (x.Value == 0.0f);
            targets += x.Key + ":" + x.Value.ToString("0.0#####", CultureInfo.GetCultureInfo("en-US")) + ",";
        }
        if (results || force) {
            lock (EventLock) {
                if (state != State.Active)
                    return true;
                state = State.End;
            }
            
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
            
            Console.WriteLine($"Event {uid} end: {results} {targets}");
            
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
            
            foreach (var roomClient in room.Clients) {
                roomClient.Send(packet);
            }
            
            Reset(4);
            
            timer = new System.Timers.Timer(10000);
            timer.Elapsed += PostEndEvent;
            timer.AutoReset = false;
            timer.Enabled = true;

            return true;
        }
        return false;
    }
    
    private void PostEndEvent(Object source, ElapsedEventArgs e) {
        NetworkPacket packet = Utils.ArrNetworkPacket( new string[] {
            "WESR",
            "WE_ScoutAttack|" + EventInfo()
        });
        foreach (var roomClient in room.Clients) {
            roomClient.Send(packet);
        }
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
        
        lock (EventLock) {
            if (state != State.Active) {
                Console.WriteLine($"Event {uid} reject UpdateHealth for {targetUid} with event state {state}");
                return -1.0f; // do not send WEH_ when event is not active
            }
        }
        
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
            Console.WriteLine($"Event {uid} set TimeSpan: {seconds} from operator: {client == operatorAI}");
            endTime = startTime.AddSeconds(seconds);
            endTimeIsSet = true;
        }
    }
    
    public float GetHealth(string targetUid) => health[targetUid];
}
