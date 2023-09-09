using sodoffmmo.Core;
using sodoffmmo.Data;
using System;
using System.Net;
using System.Net.Sockets;

namespace sodoffmmo;
public class Server {

    readonly int port;
    readonly IPAddress ipAddress;
    ModuleManager moduleManager = new();

    public Server(IPAddress ipAdress, int port) {
        this.ipAddress = ipAdress;
        this.port = port;
    }

    public async Task Run() {
        moduleManager.RegisterModules();
        using Socket listener = new(ipAddress.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);

        listener.Bind(new IPEndPoint(ipAddress, port));
        await Listen(listener);
    }

    private async Task Listen(Socket listener) {
        Console.WriteLine($"MMO Server listening on port {port}");
        listener.Listen(100);
        while (true) {
            Socket handler = await listener.AcceptAsync();
            handler.SendTimeout = 200;
            Console.WriteLine($"New connection from {((IPEndPoint)handler.RemoteEndPoint!).Address}");
            _ = Task.Run(() => HandleClient(handler));
        }
    }

    private async Task HandleClient(Socket handler) {
        Client client = new(handler);
        try {
            while (client.Connected) {
                await client.Receive();
                List<NetworkObject> networkObjects = new();
                while (client.TryGetNextPacket(out NetworkPacket packet))
                    networkObjects.Add(packet.GetObject());

                HandleObjects(networkObjects, client);
            }
        } finally {
            try {
                client.LeaveRoom();
            } catch (Exception) { }
            client.Disconnect();
            Console.WriteLine("Socket disconnected IID: " + client.ClientID);
        }
    }

    private void HandleObjects(List<NetworkObject> networkObjects, Client client) {
        foreach (var obj in networkObjects) {
            try {
                short commandId = obj.Get<short>("a");
                ICommandHandler handler;
                if (commandId != 13) {
                    if (commandId == 0 || commandId == 1)
                        Console.WriteLine($"System command: {commandId} IID: {client.ClientID}");
                    handler = moduleManager.GetCommandHandler(commandId);
                } else
                    handler = moduleManager.GetCommandHandler(obj.Get<NetworkObject>("p").Get<string>("c"));
                handler.Handle(client, obj.Get<NetworkObject>("p"));
            } catch (Exception ex) {
                Console.WriteLine($"Exception IID: {client.ClientID} - {ex}");
            }
        }
    }
}
