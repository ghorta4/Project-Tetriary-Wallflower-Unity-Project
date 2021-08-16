using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using System;
using System.Net;

public class WirelessClient : WirelessCommunicator
{
    largeObjectByteHandler lobh = new largeObjectByteHandler();

    public IPEndPoint serverEndpoint;

    //keep track of what kind of data type is read/the associated ID and don't use out of order packets
    public Dictionary<PacketType, int> sentDataRecords = new Dictionary<PacketType, int> { };

    //this value stores what the latest processed request ID was
    public Dictionary<PacketType, int> dataSequencingDictionary = new Dictionary<PacketType, int> { };

    public override void Initialize()
    {
        FindVariablePort();
    }

    public void Update()
    {
        SendHeartbeat();

        lobh.runCleanup();
        while (packets.Count > 0)
        {
            ProcessLatestPacket();
        }

        CheckForServerConnection();

    }

    public override void RecievePacket(IPAddress address, int port, byte[] data)
    {
        packets.Enqueue(DataPacket.GetDataPacket(address, port, data, dataSequencingDictionary));
    }

    public void ProcessLatestPacket()
    {
        DataPacket dp = packets.Dequeue();
        if (dp == null)
        {
            return;
        }
        switch (dp.stowedPacketType)
        {
            default:
                ErrorHandler.AddErrorToLog(new Exception("Unhandled packet type: " + dp.stowedPacketType));
                break;
        }
    }

    public void SendMessageToServer(byte[] message, PacketType type, int repeats)
    {
        byte[] assembledPacket = GenerateProperDataPacket(message, type, sentDataRecords);
        SendPacketToGivenEndpoint(serverEndpoint, assembledPacket);
    }

    DateTime lastHeartbeat;
    public void SendHeartbeat()
    {
        if ((DateTime.Now -lastHeartbeat).TotalMilliseconds > 500)
        {
            lastHeartbeat = DateTime.Now;
            SendMessageToServer(new byte[0], PacketType.heartbeatPing, 1);
        }
    }

    DateTime lastServerCheck, lastServerPingRecieved;
    public void CheckForServerConnection()
    {
        if ((DateTime.Now - lastServerCheck).TotalMilliseconds > 300)
        {
            lastServerCheck = DateTime.Now;
            SendMessageToServer(new byte[0], PacketType.requestPingback, 1);
        }
    }
}