using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;

namespace sodoffmmo.CommandHandlers;

[ExtensionCommandHandler("JA")]
class JoinRoomHandler : ICommandHandler
{
    Room room;
    Client client;
    public void Handle(Client client, NetworkObject receivedObject)
    {
        string roomName = receivedObject.Get<NetworkObject>("p").Get<string>("rn");
        client.LeaveRoom();
        client.InvalidatePlayerData();
        room = Room.GetOrAdd(roomName);
        Console.WriteLine($"Join Room: {roomName} RoomID: {room.Id} IID: {client.ClientID}");
        this.client = client;

        RespondJoinRoom();
        SubscribeRoom();
        UpdatePlayerUserVariables();
        client.Room = room;
    }

    private void RespondJoinRoom() {
        NetworkObject obj = new();
        NetworkArray roomInfo = new();
        roomInfo.Add(room.Id);
        roomInfo.Add(room.Name); // Room Name
        roomInfo.Add(room.Name); // Group Name
        roomInfo.Add(true);
        roomInfo.Add(false);
        roomInfo.Add(false);
        roomInfo.Add((short)24);
        roomInfo.Add((short)27);
        roomInfo.Add(new NetworkArray());
        roomInfo.Add((short)0);
        roomInfo.Add((short)0);

        NetworkArray userList = new();
        foreach (Client player in room.Clients) {
            userList.Add(player.PlayerData.GetNetworkData(player.ClientID));
        }

        obj.Add("r", roomInfo);
        obj.Add("ul", userList);

        NetworkPacket packet = NetworkObject.WrapObject(0, 4, obj).Serialize();
        packet.Compress();
        client.Send(packet);
    }

    private void SubscribeRoom() {
        NetworkObject obj = new();
        NetworkArray list = new();

        NetworkArray r1 = new();
        r1.Add(room.Id);
        r1.Add(room.Name); // Room Name
        r1.Add(room.Name); // Group Name
        r1.Add(true);
        r1.Add(false);
        r1.Add(false);
        r1.Add((short)24);
        r1.Add((short)27);
        r1.Add(new NetworkArray());
        r1.Add((short)0);
        r1.Add((short)0);

        NetworkArray r2 = new();
        r2.Add(room.Id);
        r2.Add(room.Name); // Room Name
        r2.Add(room.Name); // Group Name
        r2.Add(true);
        r2.Add(false);
        r2.Add(false);
        r2.Add((short)7);
        r2.Add((short)27);
        r2.Add(new NetworkArray());
        r2.Add((short)0);
        r2.Add((short)0);

        list.Add(r1);
        list.Add(r2);

        obj.Add("rl", list);
        obj.Add("g", room.Name);
        client.Send(NetworkObject.WrapObject(0, 15, obj).Serialize());
    }

    private void UpdatePlayerUserVariables() {
        foreach (Client c in room.Clients) {
            NetworkObject cmd = new();
            NetworkObject obj = new();
            cmd.Add("c", "SUV");
            obj.Add("MID", c.ClientID);
            cmd.Add("p", obj);
            client.Send(NetworkObject.WrapObject(1, 13, cmd).Serialize());
        }

    }
}
