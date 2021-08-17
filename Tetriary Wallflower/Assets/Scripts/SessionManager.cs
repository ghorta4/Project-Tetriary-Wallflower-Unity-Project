using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Guildleader;

public static class SessionManager
{
    static WirelessClient client;

    static ClientWorld world;

    public static void Initialize()
    {
        client = new WirelessClient();
        client.serverEndpoint = new IPEndPoint(IPAddress.Loopback, 44500);
        client.Initialize();
        client.StartListeningThread();

        world = new ClientWorld();
        world.Initialize();

        ErrorHandler.PrintErrorLog();
    }

    public static void Update()
    {
        client.Update();
    }
}
