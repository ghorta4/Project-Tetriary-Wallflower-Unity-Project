using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using Guildleader;

public static class SessionManager
{
    public const float unitScaling = 1;
    public const float zScaleStretch = 1;

    static WirelessClient client;

    public static ClientWorld world { get { return WorldManager.currentWorld as ClientWorld; } }

    public static void Initialize()
    {
        client = new WirelessClient();
        client.serverEndpoint = new IPEndPoint(IPAddress.Loopback, 44500);
        client.Initialize();

        WorldManager.currentWorld = new ClientWorld();
        (WorldManager.currentWorld as ClientWorld).Initialize();

        FileAccess.SetDefaultDirectory(FileAccess.AssetsFileLocation);

        ImageLibrary.LoadImageLibraries();
        TileLibrary.LoadTileLibrary();

        ErrorHandler.PrintErrorLog();
    }

    public static void Update()
    {
        while (ErrorHandler.messageLog.Count > 0)
        {
            Debug.Log(ErrorHandler.messageLog[0]);
            ErrorHandler.messageLog.RemoveAt(0);
        }

        client.Update();

        ChunkRenderManager.Update();

        EntitySpriteManager.Update();
    }
}
