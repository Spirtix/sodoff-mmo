using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;

namespace sodoffmmo.CommandHandlers;

[ExtensionCommandHandler("PNG")]
class PingHandler : ICommandHandler {
    public void Handle(Client client, NetworkObject receivedObject) {
        NetworkObject cmd = new();
        NetworkObject obj = new();
        obj.Add("arr", new string[] { "PNG", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() });
        cmd.Add("c", "PNG");
        cmd.Add("p", obj);
        client.Send(NetworkObject.WrapObject(1, 13, cmd).Serialize());
    }
}
