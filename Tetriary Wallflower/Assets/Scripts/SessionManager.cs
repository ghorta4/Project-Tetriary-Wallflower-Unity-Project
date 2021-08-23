using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using Guildleader;

public static class SessionManager
{
    static WirelessClient client;

    public const float defaultTileScale = 0.1f;

    public static ClientWorld world { get { return WorldManager.currentWorld as ClientWorld; } }

    static VoxelRenderer3d m3r;

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

        m3r = VoxelRenderer3d.grabVRenderer();
        m3r.RefreshEntireMap();

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

        m3r.RefreshEntireMap();
        EntitySpriteManager.Update();
    }
}
