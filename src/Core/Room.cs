using System;

namespace sodoffmmo.Core;
public class Room {
    static int playerId = 0;
    static int roomId = 0;
    static Dictionary<string, Room> rooms = new();
    public object roomLock = new object();

    List<Client> clients = new();

    public string Name { get; private set; }
    public int Id { get; private set; }

    public Room(int id, string name) {
        Name = name;
        Id = id;
    }

    public IEnumerable<Client> Clients {
        get {
                return new List<Client>(clients);
        }
    }

    public void AddClient(Client client) {
        lock (roomLock) {
            clients.Add(client);
        }
    }

    public void RemoveClient(Client client) {
        lock (roomLock) {
            clients.Remove(client);
        }
    }

    public int NextPlayerId() => ++playerId;

    public static Room Get(string name) => rooms[name];

    public static bool Exists(string name) => rooms.ContainsKey(name);

    public static void Add(string name) {
        rooms[name] = new Room(rooms.Count + 1, name);
    }
}
