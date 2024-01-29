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
        startTime = DateTime.UtcNow.AddMinutes(-60);
        startTimeString = startTime.ToString("MM/dd/yyyy HH:mm:ss");
        uid = "sodoff";
        state = State.End;
        ScheduleEvent(10); // WE_ != WEN_
    }
    
    // controlled (init/reset) by Reset()
    private Room room;
    private string uid;
    private Client operatorAI;
    private State state;
    
    private DateTime startTime;
    private DateTime endTime;
    private DateTime nextStartTime;
    private DateTime AITime;
    private bool endTimeIsSet;
    private string startTimeString;
    private string nextStartTimeString;
    
    // controlled (init/reset) by InitEvent()
    private Dictionary<string, float> health = new();
    private Dictionary<string, string> players = new();
    private string lastResults = "";
    
    // reset event - set new id, start time, end time, etc
    private void Reset(float time = 2) {
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
            
            nextStartTime = startTime;
            nextStartTimeString = startTimeString;
            
            Console.WriteLine($"Event {uid} start time: {startTimeString}");
        }
    }
    
    // set / update event end time in results of Reset() or SetTimeSpan()
    private void UpdateEndTime(double timeout) {
        endTime = startTime.AddSeconds(timeout);
        Console.WriteLine($"Event {uid} end time: {endTime}");
        SetTimer((endTime - DateTime.UtcNow).TotalSeconds, PreEndEvent);
    }
    
    // schedule next event and set timer to call PreInit
    private void ScheduleEvent(float minutes) {
        nextStartTime = DateTime.UtcNow.AddMinutes(minutes);
        nextStartTimeString = nextStartTime.ToString("MM/dd/yyyy HH:mm:ss");;
        SetTimer(minutes*60 - 120, PreInit);
    }
    
    // reset event and set timer to call PreEndEvent, send new WE_ info
    private void PreInit(Object source, ElapsedEventArgs e) {
        Reset(); // WE_ == WEN_
        AnnounceEvent();
    }
    
    // check init state and init event (set AI, reset health, score) if need in response to client (shot, etc) request
    private void InitEvent() {
        lock (EventLock) {
            if (AITime < DateTime.UtcNow && state != State.End) {
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
            
            Console.WriteLine($"Event {uid} end: {results} {targets} {scores}");
            
            SetTimer(2, PostEndEvent1); // looks like client don't like get _End before WEH_ with 0.0 ... so wait to send _End
            
            return true;
        }
        return false;
    }
    
    // send reward info
    private void PostEndEvent1(Object source, ElapsedEventArgs e) {
            NetworkPacket packet = Utils.VlNetworkPacket(
                "WE_ScoutAttack_End",
                lastResults,
                room.Id
            );
            foreach (var roomClient in room.Clients) {
                roomClient.Send(packet);
            }
            
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
            
            Console.WriteLine($"Event {uid} sent _End");
            
            SetTimer(60, PostEndEvent2);
    }
    
    // schedule next event, set timer to call PreInit() and send new WEN_ info
    private void PostEndEvent2(Object source, ElapsedEventArgs e) {
        ScheduleEvent(30); // WE_ != WEN_
        AnnounceEvent(false, true); // send only WEN_ (WE_ should stay unchanged ... as WE_..._End)
    }
    
    // set server side timer for word event state changes
    private void SetTimer(double timeout, System.Timers.ElapsedEventHandler callback) {
        if (timer != null) {
            timer.Stop();
            timer.Close();
        }
        
        timer = new System.Timers.Timer(timeout * 1000);
        timer.AutoReset = false;
        timer.Enabled = true;
        timer.Elapsed += callback;
        
        Console.WriteLine($"Event timer {callback.Method.Name} set to {timeout} s");
    }
    
    // send event info
    private void AnnounceEvent(bool WE = true, bool WEN = true) {
        Console.WriteLine($"Event {uid} send event notification (WE_ = {(WE ? startTimeString : WE)}  WEN_ = {(WEN ? nextStartTimeString : WEN)}, room = {room.Id}) to all clients");
        NetworkPacket packet = Utils.VlNetworkPacket(EventInfoArray(WE, WEN), room.Id);
        foreach (var r in Room.AllRooms()) {
            foreach (var roomClient in r.Clients) {
                roomClient.Send(packet);
            }
        }
    }
    
    public string EventInfo() {
        return startTimeString + "," + uid + ", false, HubTrainingDO";
    }
    
    public NetworkArray EventInfoArray(bool WE = true, bool WEN = true) {
        NetworkArray vl = new();
        if (WE) {
            NetworkArray vl1 = new();
            vl1.Add("WE_ScoutAttack");
            vl1.Add((Byte)4);
            vl1.Add(EventInfo());
            vl1.Add(false);
            vl1.Add(true);
            vl.Add(vl1);
        }
        if (WEN) {
            NetworkArray vl2 = new();
            vl2.Add("WEN_ScoutAttack");
            vl2.Add((Byte)4);
            vl2.Add(nextStartTimeString);
            vl2.Add(false);
            vl2.Add(true);
            vl.Add(vl2);
        }
        
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
        
        if (health[targetUid] < 0.0001f) {
            health[targetUid] = 0.0f;
            EndEvent();
        }
        
        if (endTime < DateTime.UtcNow) {
            Console.WriteLine($"Event {uid} force end from UpdateHealth");
            EndEvent(true);
        }
        
        return health[targetUid];
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
    
    public float GetHealth(string targetUid) => health[targetUid];
    
    public bool IsActive() => (state == State.Active);
    
    public string GetLastResults() => lastResults;
}
