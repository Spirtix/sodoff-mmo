using sodoffmmo.Data;

namespace sodoffmmo.Core;
public interface ICommandHandler {
    public void Handle(Client client, NetworkObject receivedObject);
}
