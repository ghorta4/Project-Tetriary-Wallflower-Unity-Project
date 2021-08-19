using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
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
        client.StartListeningThread();

        WorldManager.currentWorld = new ClientWorld();
        (WorldManager.currentWorld as ClientWorld).Initialize();

        FileAccess.SetDefaultDirectory(FileAccess.AssetsFileLocation);

        ImageLibrary.LoadImageLibraries();
        TileLibrary.LoadTileLibrary();

        m3r = VoxelRenderer3d.grabVRenderer();
        m3r.RefreshEntireMap();

        world.LoadNearbyChunkData(Int3.Zero, 2);

        ErrorHandler.PrintErrorLog();
    }

    public static void Update()
    {
        client.Update();
        bool buttonPressed = Input.GetKey("h");
        if (buttonPressed)
        {
            world.UnloadDistantChunkData(new Int3(10,10,10), 0);
            world.LoadNearbyChunkData(Int3.Zero, 1);
            Debug.Log("pressed");
        }
        m3r.RefreshEntireMap();
    }
}
