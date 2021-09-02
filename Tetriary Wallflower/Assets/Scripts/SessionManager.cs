using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using Guildleader;
using Guildleader.Entities;
using System.Threading;

public static class SessionManager
{
    public const float unitScaling = 1;
    public const float zScaleStretch = 1;

    public static WirelessClient client;

    public static ClientWorld world { get { return WorldManager.currentWorld as ClientWorld; } }

    public static bool QuitApplication;

    static Thread clientThread;

    public static void Initialize()
    {
        client = new WirelessClient();
        client.serverEndpoint = new IPEndPoint(IPAddress.Loopback, 44500);
        client.Initialize();

        clientThread = new Thread(ClientUpdateThread);
        clientThread.Start();

        WorldManager.currentWorld = new ClientWorld();
        (WorldManager.currentWorld as ClientWorld).Initialize();

        FileAccess.SetDefaultDirectory(FileAccess.AssetsFileLocation);

        ImageLibrary.LoadImageLibraries();
        TileLibrary.LoadTileLibrary();

        ErrorHandler.PrintErrorLog();

        ChunkRenderManager.Initialize();
    }

    public static void Update(float timestep)
    {

        ChunkRenderManager.Update();

        EntitySpriteManager.Update(timestep);

        world.Update();

        client.UpdateMainThread();

        int x = 0, y = 0, z = 0; //test values. delete later
        if (Input.GetKeyDown("d"))
        {
            x--;
        }
        if (Input.GetKeyDown("a"))
        {
            x++;
        }
        if (Input.GetKeyDown("w"))
        {
            y++;
        }
        if (Input.GetKeyDown("s"))
        {
            y--;
        }
        if (Input.GetKeyDown("q"))
        {
            z++;
        }
        if (Input.GetKeyDown("e"))
        {
            z--;
        }
        if (x != 0 || y != 0 || z != 0)
        {
            List<byte> packet = new List<byte>();
            packet.AddRange(Guildleader.Convert.ToByte(x));
            packet.AddRange(Guildleader.Convert.ToByte(y));
            packet.AddRange(Guildleader.Convert.ToByte(z));
            client.SendMessageToServer(packet.ToArray(),WirelessCommunicator.PacketType.debugCommands, 3);
        }
    }

    static void ClientUpdateThread() //to help with the bottleneck of client processing chunk data
    {
        while (!QuitApplication)
        {
            client.UpdateSideThread();
        }
    }
}
