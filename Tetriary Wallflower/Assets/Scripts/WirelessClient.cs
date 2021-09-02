using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using System;
using System.Net;
using System.Linq;

public class WirelessClient : WirelessCommunicator
{
    LargeObjectByteHandler lobh = new LargeObjectByteHandler();

    public IPEndPoint serverEndpoint;

    //keep track of what kind of data type is read/the associated ID and don't use out of order packets
    public Dictionary<PacketType, int> sentDataRecords = new Dictionary<PacketType, int> { };

    //this value stores what the latest processed request ID was
    public Dictionary<PacketType, int> dataSequencingDictionary = new Dictionary<PacketType, int> { };

    public override void Initialize()
    {
        FindVariablePort();
        StartListeningThread();
    }

    DateTime lastHeartbeatSent= DateTime.Now;

    public void UpdateMainThread()
    {
        SendHeartbeat();
        LargeObjectByteHandlerUpdate();
        CheckForServerConnection();
    }
    public void UpdateSideThread()
    {

        while (packets.Count > 0)
        {
            ProcessLatestPacket();
        }

    }

    public override void RecievePacket(IPAddress address, int port, byte[] data)
    {
        DataPacket dp = DataPacket.GetDataPacket(address, port, data, dataSequencingDictionary);
        if (dp != null)
        {
            packets.Enqueue(dp);
        }
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
            case PacketType.heartbeatPing:
                break;
            case PacketType.largeObjectPacket:
                lobh.RecieveSegments("SLP", dp.contents); //SLP Stands for Server Large Packet
                break;
            case PacketType.chunkInfo:
                SessionManager.world?.ProcessChunkBytes(dp.contents);
                break;
            case PacketType.nearbyEntityInfo:
                SessionManager.world?.ProcessEntityBytes(dp.contents);
                break;
            case PacketType.entityIDToTrack:
                SessionManager.world?.ProcessToTrackBytes(dp.contents);
                break;
            default:
                Debug.Log(new Exception("Unhandled packet type: " + dp.stowedPacketType));
                break;
        }
    }

    public void SendMessageToServer(byte[] message, PacketType type, int repeats)
    {
        byte[] assembledPacket = GenerateProperDataPacket(message, type, sentDataRecords);
        SendPacketToGivenEndpoint(serverEndpoint, assembledPacket);
    }

    void LargeObjectByteHandlerUpdate()
    {
        byte[][] completedPackets = lobh.GrabAllCompletedPackets();
        foreach(byte[] barray in completedPackets)
        {
            DataPacket dp = DataPacket.GetDataPacket(serverEndpoint.Address, serverEndpoint.Port, barray, dataSequencingDictionary);
            packets.Enqueue(dp);
        }

        lobh.RunCleanup();

        //check for the oldest large packet missing segments and re-request them, with the knowledge that the above function already grabbed the completed ones and marked them as such
        PacketAssembler oldestUnfinishedPacket = null;
        DateTime now = DateTime.Now;
        foreach (var kvp in lobh.recievedSegments)
        {
            if (kvp.Value.fullPacketAlreadyAcknowledged)
            {
                continue;
            }
            if (((now - kvp.Value.dateOfLastRecievedPart).TotalMilliseconds > 300) && (oldestUnfinishedPacket == null || kvp.Value.dateOfLastRecievedPart < oldestUnfinishedPacket.dateOfLastRecievedPart))
            {
                oldestUnfinishedPacket = kvp.Value;
            }
        }

        if (oldestUnfinishedPacket != null)
        {
            List<int> missingSegments = oldestUnfinishedPacket.GetMissingParts();
            List<byte> converted = new List<byte>();
            converted.AddRange(Guildleader.Convert.ToByte(oldestUnfinishedPacket.packetID));
            foreach (int i in missingSegments)
            {
                converted.AddRange(Guildleader.Convert.ToByte(i));
            }
            byte[] requestArray = converted.ToArray();
            SendMessageToServer(requestArray, PacketType.largePacketRepairRequest, 3);
        }
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