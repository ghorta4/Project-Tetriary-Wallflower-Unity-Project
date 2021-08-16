using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using System;
using System.Net;

public class WirelessClient : WirelessCommunicator
{
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
        lobh.runCleanup();
        while (packets.Count > 0)
        {
            ProcessLatestPacket();
        }
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

    public void sendMessageToServer(byte[] message, PacketType type)
    {
        byte[] assembledPacket = GenerateProperDataPacket(message, type, sentDataRecords);
        SendPacketToGivenEndpoint(serverEndpoint, assembledPacket);
    }
}