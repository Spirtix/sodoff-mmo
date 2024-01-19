using sodoffmmo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sodoffmmo.Data;

namespace sodoffmmo.Core;
internal static class Utils {
    public static bool VariablesValid(Client client) {
        if (client.PlayerData.Fp != "" && (client.PlayerData.Mbf & 8) == 8
            && (client.PlayerData.GeometryType == PetGeometryType.Default && client.PlayerData.PetAge < PetAge.Teen
                || client.PlayerData.GeometryType == PetGeometryType.Terror && client.PlayerData.PetAge < PetAge.Titan)) {
                        NetworkObject obj = new NetworkObject();
                        obj.Add("dr", (byte)1);
                        client.Send(NetworkObject.WrapObject(0, 1005, obj).Serialize());
                        client.SheduleDisconnect();
                        return false;
        }
        return true;
    }
    
    public static NetworkPacket VlNetworkPacket(NetworkArray vl2, int roomID) {
        NetworkObject wedata = new();
        NetworkArray vl = new();
        vl.Add(vl2);
        wedata.Add("r", roomID);
        wedata.Add("vl", vl);
        return NetworkObject.WrapObject(0, 11, wedata).Serialize();
    }

    public static NetworkPacket VlNetworkPacket(string a, string b) {
        NetworkArray vl2 = new();
        vl2.Add(a);
        vl2.Add((Byte)4);
        vl2.Add(b);
        vl2.Add(false);
        vl2.Add(false);
        return VlNetworkPacket(vl2, WorldEvent.Get().GetRoom().Id);
    }

    public static NetworkPacket ArrNetworkPacket(string[] data) {
        NetworkObject cmd = new();
        NetworkObject obj = new();
        obj.Add("arr", data);
        cmd.Add("c", "");
        cmd.Add("p", obj);
        return NetworkObject.WrapObject(1, 13, cmd).Serialize();
    }
}
