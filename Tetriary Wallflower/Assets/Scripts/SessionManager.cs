using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Guildleader;

public static class SessionManager
{
    static WirelessClient client;

    public static ClientWorld world;

    public const float defaultTileScale = 0.1f;

    public static void Initialize()
    {
        client = new WirelessClient();
        client.serverEndpoint = new IPEndPoint(IPAddress.Loopback, 44500);
        client.Initialize();
        client.StartListeningThread();

        world = new ClientWorld();
        world.Initialize();

        FileAccess.SetDefaultDirectory(FileAccess.LocalFilesLocation);

        ImageLibrary.LoadImageLibraries();
        ErrorHandler.PrintErrorLog();
    }

    public static void Update()
    {
        client.Update();
    }
}
