﻿using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;
using System.Globalization;

namespace sodoffmmo.CommandHandlers;

[ExtensionCommandHandler("SUV")]
class SetUserVariablesHandler : ICommandHandler {
    NetworkObject suvData;
    Client client;
    string? uid;
    public void Handle(Client client, NetworkObject receivedObject) {
        this.client = client;
        suvData = receivedObject.Get<NetworkObject>("p");
        uid = suvData.Get<string>("UID");

        // TODO
        if (uid != null && client.PlayerData.Uid != uid)
            ProcessPlayerData();
        else
            UpdateVars();
        
    }

    private void ProcessPlayerData() {
        client.PlayerData.Uid = uid;
        client.PlayerData.A = suvData.Get<string>("A");
        client.PlayerData.Cu = suvData.Get<string>("CU");
        client.PlayerData.M = suvData.Get<string>("M");
        client.PlayerData.L = suvData.Get<string>("L");
        client.PlayerData.Ra = suvData.Get<string>("RA");
        string udt = suvData.Get<string>("UDT");
        if (udt != null)
            client.PlayerData.Udt = udt;
        client.PlayerData.P1 = double.Parse(suvData.Get<string>("P1"), CultureInfo.InvariantCulture);
        client.PlayerData.P2 = double.Parse(suvData.Get<string>("P2"), CultureInfo.InvariantCulture);
        client.PlayerData.P3 = double.Parse(suvData.Get<string>("P3"), CultureInfo.InvariantCulture);
        client.PlayerData.R1 = double.Parse(suvData.Get<string>("R1"), CultureInfo.InvariantCulture);
        client.PlayerData.R2 = double.Parse(suvData.Get<string>("R2"), CultureInfo.InvariantCulture);
        client.PlayerData.R3 = double.Parse(suvData.Get<string>("R3"), CultureInfo.InvariantCulture);
        client.PlayerData.R = double.Parse(suvData.Get<string>("R"), CultureInfo.InvariantCulture);
        client.PlayerData.Mbf = int.Parse(suvData.Get<string>("MBF"));
        client.PlayerData.F = int.Parse(suvData.Get<string>("F"));
        client.PlayerData.J = suvData.Get<string>("J");
        client.PlayerData.Bu = suvData.Get<string>("BU");
        client.PlayerData.Fp = suvData.Get<string>("FP");

        Console.WriteLine($"SUV {client.Room.Name} IID: {client.ClientID}");
        client.Room.AddClient(client);
        UpdatePlayersInRoom();
        SendSUVToPlayerInRoom();
    }

    private void UpdateVars() {
        string? FP = suvData.Get<string>("FP");
        string? PU = suvData.Get<string>("PU");
        string? L = suvData.Get<string>("L");
        if (FP is null && PU is null && L is null) {
            return; // TODO
        }
        NetworkObject data = new();
        NetworkObject data2 = new();
        data.Add("u", client.ClientID);

        NetworkArray vl = new();
        if (FP != null) {
            client.PlayerData.Fp = FP;
            data2.Add("FP", client.PlayerData.Fp);
            vl.Add(NetworkArray.StringParam("FP", client.PlayerData.Fp));
        }
        if (PU != null) {
            client.PlayerData.Pu = PU;
            data2.Add("PU", client.PlayerData.Pu);
            vl.Add(NetworkArray.StringParam("PU", client.PlayerData.Pu));
        }
        if (L != null) {
            client.PlayerData.L = L;
            data2.Add("L", client.PlayerData.L);
            vl.Add(NetworkArray.StringParam("L", client.PlayerData.L));
        }

        data.Add("vl", vl);

        NetworkPacket packet = NetworkObject.WrapObject(0, 12, data).Serialize();
        foreach (var roomClient in client.Room.Clients)
            roomClient.Send(packet);

        NetworkObject cmd = new();
        cmd.Add("c", "SUV");

        NetworkObject container = new();

        NetworkArray arr = new();
        data2.Add("RID", "1");
        data2.Add("MID", client.ClientID);
        arr.Add(data2);
        container.Add("arr", arr);
        cmd.Add("p", container);
        packet = NetworkObject.WrapObject(1, 13, cmd).Serialize();
        foreach (var roomClient in client.Room.Clients) {
            if (roomClient != client)
                roomClient.Send(packet);
        }
    }

    private void UpdatePlayersInRoom() {
        NetworkObject data = new();
        NetworkObject acknowledgement = new();
        data.Add("r", client.Room.Id);

        NetworkArray user = new();
        user.Add(client.ClientID);
        user.Add(client.PlayerData.Uid);
        user.Add((short)1);
        user.Add((short)client.ClientID);

        NetworkArray playerData = new();
        playerData.Add(NetworkArray.DoubleParam("R1", client.PlayerData.R1));
        playerData.Add(NetworkArray.StringParam("FP", client.PlayerData.Fp));
        playerData.Add(NetworkArray.DoubleParam("MX", client.PlayerData.Mx));
        playerData.Add(NetworkArray.StringParam("UDT", client.PlayerData.Udt));
        playerData.Add(NetworkArray.DoubleParam("P2", client.PlayerData.P2));
        playerData.Add(NetworkArray.DoubleParam("NT", Runtime.CurrentRuntime));
        playerData.Add(NetworkArray.IntParam("t", (int)(Runtime.CurrentRuntime / 1000)));
        playerData.Add(NetworkArray.StringParam("J", client.PlayerData.J));
        playerData.Add(NetworkArray.IntParam("F", client.PlayerData.F));
        playerData.Add(NetworkArray.IntParam("MBF", client.PlayerData.Mbf));
        playerData.Add(NetworkArray.DoubleParam("R2", client.PlayerData.R2));
        playerData.Add(NetworkArray.DoubleParam("R", client.PlayerData.R));
        playerData.Add(NetworkArray.StringParam("BU", client.PlayerData.Bu));
        playerData.Add(NetworkArray.DoubleParam("P1", client.PlayerData.P1));
        playerData.Add(NetworkArray.StringParam("UID", client.PlayerData.Uid));
        playerData.Add(NetworkArray.DoubleParam("R3", client.PlayerData.R3));
        playerData.Add(NetworkArray.StringParam("PU", client.PlayerData.Pu));
        playerData.Add(NetworkArray.StringParam("A", client.PlayerData.A));
        playerData.Add(NetworkArray.StringParam("RA", client.PlayerData.Ra));
        playerData.Add(NetworkArray.DoubleParam("P3", client.PlayerData.P3));
        playerData.Add(NetworkArray.StringParam("CU", client.PlayerData.Cu));
        playerData.Add(NetworkArray.StringParam("M", client.PlayerData.M));
        playerData.Add(NetworkArray.StringParam("L", client.PlayerData.L));

        user.Add(playerData);
        data.Add("u", user);

        acknowledgement.Add("u", client.ClientID);
        acknowledgement.Add("vl", playerData);
        NetworkPacket ackPacket = NetworkObject.WrapObject(0, 12, acknowledgement).Serialize();
        NetworkObject obj = ackPacket.GetObject();
        ackPacket.Compress();
        client.Send(ackPacket);

        NetworkPacket packet = NetworkObject.WrapObject(0, 1000, data).Serialize();
        packet.Compress();

        foreach (var roomClient in client.Room.Clients) {
            if (roomClient != client)
                roomClient.Send(packet);
        }
    }

    private void SendSUVToPlayerInRoom() {
        NetworkObject cmd = new();
        NetworkObject obj = new();

        cmd.Add("c", "SUV");
        obj.Add("MID", client.ClientID);
        cmd.Add("p", obj);

        NetworkPacket packet = NetworkObject.WrapObject(1, 13, cmd).Serialize();
        foreach (var roomClient in client.Room.Clients) {
            if (roomClient != client.Room.Clients)
                roomClient.Send(packet);
        }
    }
}
