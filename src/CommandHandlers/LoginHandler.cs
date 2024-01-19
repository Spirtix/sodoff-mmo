using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;

namespace sodoffmmo.CommandHandlers;

[CommandHandler(1)]
class LoginHandler : ICommandHandler
{
    public void Handle(Client client, NetworkObject receivedObject)
    {
        client.PlayerData.UNToken = receivedObject.Get<string>("un");
        NetworkArray rl = new();

        NetworkArray r1 = new();
        r1.Add(0);
        r1.Add("MP_SYS");
        r1.Add("default");
        r1.Add(true);
        r1.Add(false);
        r1.Add(false);
        r1.Add((short)0);
        r1.Add((short)10);
        r1.Add(new NetworkArray());
        r1.Add((short)0);
        r1.Add((short)0);
        rl.Add(r1);

        NetworkArray r2 = new();

        NetworkArray we = new();
        NetworkArray we1 = new();
        NetworkArray we2 = new();
        
        we1.Add("WE_ScoutAttack");
        we1.Add((Byte)4);
        we1.Add(WorldEvent.Get().EventInfo());
        we1.Add(false);
        we1.Add(true);
        
        we2.Add("WEN_ScoutAttack");
        we2.Add((Byte)4);
        we2.Add(WorldEvent.Get().EventInfoNext());
        we2.Add(false);
        we2.Add(true);
        
        we.Add(we1);
        we.Add(we2);
        
        r2.Add(1);
        r2.Add("ADMIN");
        r2.Add("default");
        r2.Add(false);
        r2.Add(false);
        r2.Add(true);
        r2.Add((short)0);
        r2.Add((short)1);
        r2.Add(we);
        rl.Add(r2);

        NetworkArray r3 = new();
        r3.Add(2);
        r3.Add("LIMBO");
        r3.Add("default");
        r3.Add(false);
        r3.Add(false);
        r3.Add(false);
        r3.Add((short)31);
        r3.Add((short)10000);
        r3.Add(new NetworkArray());
        rl.Add(r3);

        NetworkObject content = new();
        content.Add("rl", rl);
        content.Add("zn", "JumpStart");
        content.Add("rs", (short)5);
        content.Add("un", client.PlayerData.UNToken);
        content.Add("id", client.ClientID);
        content.Add("pi", (short)1);

        client.Send(NetworkObject.WrapObject(0, 1, content).Serialize());
    }
}
