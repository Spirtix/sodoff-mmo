using sodoffmmo.Data;
using System;
using System.Net.Sockets;

namespace sodoffmmo.Core;
public class Client {
    static int id;
    static object lck = new();

    public int ClientID { get; private set; }
    public PlayerData PlayerData { get; set; } = new();
    public Room Room { get; set; }
    public object ClientLock = new();

    private readonly Socket socket;
    private NetworkData? lastData;
    private NetworkPacket? incompleteData;
    private bool hasIncompletePakcet = false;

    public Client(Socket clientSocket) {
        socket = clientSocket;
        lock (lck) {
            ClientID = ++id;
        }
    }

    public async Task Receive() {
        byte[] buffer = new byte[2048];
        int len = await socket.ReceiveAsync(buffer, SocketFlags.None);
        if (len == 0)
            throw new SocketException();
        lastData = new NetworkData(buffer);
    }

    public bool TryGetNextPacket(out NetworkPacket packet) {
        packet = new();
        if (hasIncompletePakcet) {
            // TODO
        }
        byte header = lastData!.ReadByte();
        if (header != 0x80 && header != 0xa0)
            return false;
        short length = lastData.ReadShort();
        byte[] data = lastData.ReadChunk(length);
        packet = new NetworkPacket(header, data);
        return true;
    }

    public void Send(NetworkPacket packet) {
        socket.Send(packet.SendData);
    }

    public void LeaveRoom() {
        if (Room != null) {
            Console.WriteLine($"Leave room {Room.Name} IID: {ClientID}");
            Room.RemoveClient(this);
            NetworkObject data = new();
            data.Add("r", Room.Id);
            data.Add("u", ClientID);

            NetworkPacket packet = NetworkObject.WrapObject(0, 1004, data).Serialize();
            foreach (var roomClient in Room.Clients) {
                roomClient.Send(packet);
            }
            Room = null;
        }
    }

    public void InvalidatePlayerData() {
        PlayerData = new();
    }

    public void Disconnect() {
        try {
            socket.Shutdown(SocketShutdown.Both);
        } finally {
            socket.Close();
        }
    }

    public bool Connected {
        get {
            return socket.Connected;
        }
    }
}
