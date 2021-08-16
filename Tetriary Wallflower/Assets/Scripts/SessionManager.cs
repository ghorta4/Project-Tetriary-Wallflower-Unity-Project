using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public static class SessionManager
{
    static WirelessClient client;

    public static void Initialize()
    {
        client = new WirelessClient();
        client.serverEndpoint = new IPEndPoint(IPAddress.Loopback, 44500);
        client.Initialize();
        client.StartListeningThread();
    }

    public static void Update()
    {
        client.Update();
        client.SendPacketToGivenEndpoint(new IPEndPoint(IPAddress.Loopback, 44500), new byte[24]);
    }
}
