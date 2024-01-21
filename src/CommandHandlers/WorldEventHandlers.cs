﻿using System.Globalization;
using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;

namespace sodoffmmo.CommandHandlers;

[ExtensionCommandHandler("wex.WES")] // event status request
class WorldEventStatusHandler : ICommandHandler {
    public void Handle(Client client, NetworkObject receivedObject) {
        client.Send(Utils.ArrNetworkPacket( new string[] {
            "WESR",
            "WE_ScoutAttack|" + WorldEvent.Get().EventInfo()
        }));
    }
}

[ExtensionCommandHandler("wex.OV")] 
class WorldEventHealthHandler : ICommandHandler {
    // rec: {"a":13,"c":1,"p":{"c":"wex.OV","p":{"en":"","event":"ScoutAttack","eventUID":"ZydLUmCC","oh":"0.003444444","uid":"ZydLUmCC1"},"r":-1}}
    public void Handle(Client client, NetworkObject receivedObject) {
        NetworkObject p = receivedObject.Get<NetworkObject>("p");
        float healthUpdateVal = float.Parse(
            p.Get<string>("oh"),
            System.Globalization.CultureInfo.InvariantCulture
        );
        string targetUid = p.Get<string>("uid");
        
        float health = WorldEvent.Get().UpdateHealth(targetUid, healthUpdateVal);
        
        if (health >= 0.0f) {
            // send: {"a":11,"c":0,"p":{"r":367256,"vl":[["WEH_ZydLUmCC1",4,"0.33133352,Thu Jun 22 02:02:43 UTC 2023",false,false]]}}
            NetworkPacket packet = Utils.VlNetworkPacket(
                "WEH_" + targetUid,
                health.ToString("0.0#####", CultureInfo.GetCultureInfo("en-US")) + "," + DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss UTC yyyy", CultureInfo.GetCultureInfo("en-US")),
                WorldEvent.Get().GetRoom().Id
            );
            foreach (var roomClient in WorldEvent.Get().GetRoom().Clients) {
                roomClient.Send(packet);
            }
        }
    }
}

[ExtensionCommandHandler("wex.OVF")] // flare info from ship AI -> resend as WEF_
class WorldEventFlareHandler : ICommandHandler {
    // rec: {"a":13,"c":1,"p":{"c":"wex.OVF","p":{"en":"","fuid":"WpnpDyJ51,14,0","oh":"0","ts":"6/29/2023 3:03:18 AM"},"r":-1}}
    public void Handle(Client client, NetworkObject receivedObject) {
        NetworkObject p = receivedObject.Get<NetworkObject>("p");
        
        // send: {"a":11,"c":0,"p":{"r":403777,"vl":[["WEF_WpnpDyJ51,14,0",4,"0,6/29/2023 3:03:18 AM",false,false]]}}
        NetworkPacket packet = Utils.VlNetworkPacket(
            "WEF_" + p.Get<string>("fuid"),
            p.Get<string>("oh") + "," + p.Get<string>("ts"),
            WorldEvent.Get().GetRoom().Id
        );
        foreach (var roomClient in WorldEvent.Get().GetRoom().Clients) {
            roomClient.Send(packet);
        }
    }
}

[ExtensionCommandHandler("wex.ST")] // missile info from ship AI -> resend as WA
class WorldEventMissileHandler : ICommandHandler {
    // rec: {"a":13,"c":1,"p":{"c":"wex.ST","p":{"en":"","objID":"-4X_gWAo1","tID":"f5b6254a-df78-4e24-aa9d-7e14539fb858","uID":"1f8eeb6b-753f-4e7f-af13-42cdd69d14e7","wID":"5"},"r":-1}}
    public void Handle(Client client, NetworkObject receivedObject) {
        NetworkObject p = receivedObject.Get<NetworkObject>("p");
        
        // send: {"a":13,"c":1,"p":{"c":"","p":{"arr":["WA","1f8eeb6b-753f-4e7f-af13-42cdd69d14e7","5","f5b6254a-df78-4e24-aa9d-7e14539fb858","-4X_gWAo1"]}}}
        NetworkPacket packet = Utils.ArrNetworkPacket(new string[] {
            "WA",
            p.Get<string>("uID"),
            p.Get<string>("wID"),
            p.Get<string>("tID"),
            p.Get<string>("objID")
        });
        foreach (var roomClient in WorldEvent.Get().GetRoom().Clients) {
            roomClient.Send(packet);
        }
    }
}

[ExtensionCommandHandler("wex.PS")]
class WorldEventScoreHandler : ICommandHandler {
    // rec: {"a":13,"c":1,"p":{"c":"wex.PS","p":{"ScoreData":"Datashyo/10","en":"","id":"ScoutAttack"},"r":-1}}
    public void Handle(Client client, NetworkObject receivedObject) {
        string scoreData = receivedObject.Get<NetworkObject>("p").Get<string>("ScoreData");
        WorldEvent.Get().UpdateScore(client, scoreData);
    }
}

[ExtensionCommandHandler("wex.AIACK")] // AI ack
class WorldEventAIACKHandler : ICommandHandler {
    // rec: {"a":13,"c":1,"p":{"c":"wex.AIACK","p":{"en":"","id":"f322dd98-e9fb-4b2d-a5e0-1c98680517b5","uid":"SoDOff1"},"r":-1}}
    public void Handle(Client client, NetworkObject receivedObject) {
        WorldEvent.Get().UpdateAI(client);
    }
}
[ExtensionCommandHandler("wex.AIP")] // AI ping
class WorldEventAIPingHandler : ICommandHandler {
    // rec: {"a":13,"c":1,"p":{"c":"wex.AIP","p":{"en":""},"r":-1}}
    public void Handle(Client client, NetworkObject receivedObject) {
        WorldEvent.Get().UpdateAI(client);
    }
}

[ExtensionCommandHandler("wex.ETS")] // time span
class WorldEventTimeSpanHandler : ICommandHandler {
    // rec: {"a":13,"c":1,"p":{"c":"wex.ETS","p":{"en":"","timeSpan":"300"},"r":-1}}
    public void Handle(Client client, NetworkObject receivedObject) {
        float timeSpan = float.Parse(
            receivedObject.Get<NetworkObject>("p").Get<string>("timeSpan"),
            System.Globalization.CultureInfo.InvariantCulture
        );
        WorldEvent.Get().SetTimeSpan(client, timeSpan);
    }
}
