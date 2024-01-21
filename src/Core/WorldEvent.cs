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
    private System.Timers.Timer timer = null;
    
    public static WorldEvent Get() {
        if (_instance == null) {
            _instance = new WorldEvent();
        }
        return _instance;
    }
    
    private WorldEvent() {
        Reset(3.3f);
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
            
            ResetTimer((endTime - DateTime.UtcNow).TotalSeconds + 30);
        }
    }
    
    private void ResetTimer(double timeout) {
        if (timer != null) {
            timer.Stop();
            timer.Close();
        }
        
        timer = new System.Timers.Timer(timeout * 1000);
        timer.Elapsed += PostEndEvent;
        timer.AutoReset = false;
        timer.Enabled = true;
        
        Console.WriteLine($"Event {uid} reset timer set to {timeout} s");
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
                
                operatorAI.Send(Utils.VlNetworkPacket("WE__AI", operatorAI.PlayerData.Uid, room.Id));
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
                $"{uid};{results};{scores};{targets}",
                room.Id
            );
            foreach (var roomClient in room.Clients) {
                roomClient.Send(packet);
            }
            
            Console.WriteLine($"Event {uid} end: {results} {targets}");
            
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
            packet = Utils.VlNetworkPacket(vl, room.Id);
            foreach (var roomClient in room.Clients) {
                roomClient.Send(packet);
            }
            
            // (re)schedule event reset and announcement of next event
            ResetTimer(10);

            return true;
        }
        return false;
    }
    
    private void PostEndEvent(Object source, ElapsedEventArgs e) {
        Reset(7);
        
        Console.WriteLine($"Event {uid} send event notification (WE_ + WEN_) to all clients");
        NetworkPacket packet = Utils.VlNetworkPacket(EventInfoArray(), room.Id);
        foreach (var r in Room.AllRooms()) {
            foreach (var roomClient in r.Clients) {
                roomClient.Send(packet);
            }
        }
    }

    
    public string EventInfo() {
        return startTimeString + "," + uid + ", false, HubTrainingDO";
    }
    
    public NetworkArray EventInfoArray(bool x = false) {
        NetworkArray vl = new();
        NetworkArray vl1 = new();
        vl1.Add("WE_ScoutAttack");
        vl1.Add((Byte)4);
        vl1.Add(EventInfo());
        vl1.Add(false);
        vl1.Add(x);
        vl.Add(vl1);
        NetworkArray vl2 = new();
        vl2.Add("WEN_ScoutAttack");
        vl2.Add((Byte)4);
        vl2.Add(startTimeString);
        vl2.Add(false);
        vl2.Add(x);
        vl.Add(vl2);
        
        return vl;
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
