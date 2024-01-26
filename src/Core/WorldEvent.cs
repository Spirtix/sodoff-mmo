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
        Reset(10);
    }
    
    // controlled (init/reset) by Reset()
    private Room room;
    private string uid;
    private Client operatorAI;
    private State state;
    
    private DateTime startTime;
    private DateTime endTime;
    private string startTimeString;
    private DateTime AITime;
    private bool endTimeIsSet;
    
    // controlled (init/reset) by InitEvent()
    private Dictionary<string, float> health = new();
    private Dictionary<string, string> players = new();
    private string lastResults = "";
    
    private void Reset(float time) {
        lock (EventLock) {
            room = Room.GetOrAdd("HubTrainingDO");
            uid = Path.GetRandomFileName().Substring(0, 8); // this is used as RandomSeed for random select ship variant
            operatorAI = null;
            state = State.NotActive;
            
            startTime = DateTime.UtcNow.AddMinutes(time);
            startTimeString = startTime.ToString("MM/dd/yyyy HH:mm:ss");
            AITime = startTime.AddMinutes(-1);
            UpdateEndTime(600 + 90);
            endTimeIsSet = false;
            
            Console.WriteLine($"Event {uid} start time: {startTimeString}");
        }
    }
    
    private void UpdateEndTime(double timeout) {
        endTime = startTime.AddSeconds(timeout);
        Console.WriteLine($"Event {uid} end time: {endTime}");
        ResetTimer((endTime - DateTime.UtcNow).TotalSeconds);
        timer.Elapsed += PreEndEvent;
    }
    
    private void ResetTimer(double timeout) {
        if (timer != null) {
            timer.Stop();
            timer.Close();
        }
        
        timer = new System.Timers.Timer(timeout * 1000);
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
                    lastResults = "";
                    state = State.Active;
                }
                
                operatorAI.Send(Utils.VlNetworkPacket("WE__AI", operatorAI.PlayerData.Uid, room.Id));
                Console.WriteLine($"Event {uid} AI operator: {operatorAI.PlayerData.Uid}");
            }
        }
    }
    
    private void PreEndEvent(Object source, ElapsedEventArgs e) {
        Console.WriteLine($"Event {uid} force end from timer");
        EndEvent(true);
    }
    
    private bool EndEvent(bool force = false) {
        bool results = false;
        string targets = "";
        if (health.Count > 0) {
            results = true;
            foreach (var x in health) {
                results = results && (x.Value == 0.0f);
                targets += x.Key + ":" + x.Value.ToString("0.0#####", CultureInfo.GetCultureInfo("en-US")) + ",";
            }
        }
        if (results || force) {
            lock (EventLock) {
                if (state == State.End || (state == State.NotActive && !force))
                    return true;
                state = State.End;
            }
            
            string scores = "";
            foreach (var x in players) {
                scores += x.Key + "/" + x.Value + ",";
            }
            lastResults = $"{uid};{results};{scores};{targets}";
            
            NetworkPacket packet = Utils.VlNetworkPacket(
                "WE_ScoutAttack_End",
                lastResults,
                room.Id
            );
            foreach (var roomClient in room.Clients) {
                roomClient.Send(packet);
            }
            
            Console.WriteLine($"Event {uid} end: {results} {targets} {scores}");
            
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
            ResetTimer(60);
            timer.Elapsed += PostEndEvent;

            return true;
        }
        return false;
    }
    
    private void PostEndEvent(Object source, ElapsedEventArgs e) {
        Reset(30);
        
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
        InitEvent();
        
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
            EndEvent();
        }
        
        if (endTime < DateTime.UtcNow) {
            Console.WriteLine($"Event {uid} force end from UpdateHealth");
            EndEvent(true);
        }
        
        return health[targetUid];
    }
    
    public void UpdateScore(string client, string value) {
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
        if (state != State.Active) {
            return;
        }
        if (client == operatorAI || !endTimeIsSet) {
            Console.WriteLine($"Event {uid} set TimeSpan: {seconds} from operator: {client == operatorAI}");
            UpdateEndTime(seconds);
            endTimeIsSet = true;
        }
    }
    
    public float GetHealth(string targetUid) => health[targetUid];
    
    public bool IsActive() {
        return state == State.Active;
    }
    
    public string GetLastResults() {
       return lastResults;
   }
}
