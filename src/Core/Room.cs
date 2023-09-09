using System;

namespace sodoffmmo.Core;
public class Room {
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
            List<Client> list;
            lock (roomLock) {
                list = new List<Client>(clients);
            }
            return list;
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

    public static Room Get(string name) => rooms[name];

    public static bool Exists(string name) => rooms.ContainsKey(name);

    public static void Add(string name) {
        rooms[name] = new Room(rooms.Count + 3, name);
    }
}
