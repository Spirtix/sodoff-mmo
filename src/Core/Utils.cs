using sodoffmmo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
