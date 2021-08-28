using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using Guildleader.Entities;
using Guildleader.Entities.BasicEntities;
using System.Linq;
using System;

public class ClientWorld : WorldDataStorageModuleGeneric
{
    public Dictionary<int, PhysicalObject> locallyLoadedEntities = new Dictionary<int, PhysicalObject>();//stored as a dictionary to improve speed when handling data from the server

    public int TrackedEntityID = int.MaxValue;

    public PhysicalObject TrackedEntity { get { locallyLoadedEntities.TryGetValue(TrackedEntityID, out PhysicalObject player); return player; } }

    public override void Initialize()
    {
        for (int i = -worldStartSizeX - 1; i <= worldStartSizeX + 1; i++)
        {
            if (!allChunks.ContainsKey(i))
            {
                allChunks[i] = new Dictionary<int, Dictionary<int, Chunk>>();
            }
            for (int j = -worldStartSizeY - 1; j <= worldStartSizeY + 1; j++)
            {
                if (!allChunks[i].ContainsKey(j))
                {
                    allChunks[i][j] = new Dictionary<int, Chunk>();
                }
            }
        }
        base.Initialize();
    }

    DateTime lastTrackedEntityRequest = DateTime.Now;
    public void Update()
    {
        if (TrackedEntityID == int.MaxValue && (DateTime.Now - lastTrackedEntityRequest).TotalMilliseconds > 100)
        {
            SessionManager.client.SendMessageToServer(new byte[0], WirelessCommunicator.PacketType.requestIDToTrack, 1);
            lastTrackedEntityRequest = DateTime.Now;
        }
    }

    public void ProcessChunkBytes(byte[] data)
    {
        Chunk c = Chunk.ConvertBytesWithPositionInFrontToChunkSimple(data);
        Int3 pos = c.chunkPos;
        if (!allChunks.ContainsKey(pos.x))
        {
            allChunks.Add(pos.x, new Dictionary<int, Dictionary<int, Chunk>>());
        }
        if (!allChunks[pos.x].ContainsKey(pos.y))
        {
            allChunks[pos.x].Add(pos.y, new Dictionary<int, Chunk>());
        }
        if (allChunks[pos.x][pos.y].ContainsKey(pos.z))
        {
            bool tilesAreTheSame = true;

            for (int i = 0; i < Chunk.defaultx; i++)
            {
                for (int j = 0; j < Chunk.defaulty; j++)
                {
                    for (int k = 0; k < Chunk.defaultz; k++)
                    {
                        if (allChunks[pos.x][pos.y][pos.z].GetTile(i,j,k) != c.GetTile(i,j,k))
                        {
                            tilesAreTheSame = false;
                            goto escapeLoop;
                        }
                    }
                }
            }
            escapeLoop:;
            if (tilesAreTheSame)
            {
                return;
            }
        }
        allChunks[pos.x][pos.y][pos.z] = c;
        ChunkRenderManager.RendererRequiresUpdates = true;
    }

    public void ProcessEntityBytes(byte[] data)
    {
        List<byte> bytes = new List<byte>(data);
        List<PhysicalObject> extractedEntities = new List<PhysicalObject>();
        while (bytes.Count > 0)
        {
            extractedEntities.Add(Entity.GenerateEntity(bytes, true) as PhysicalObject);
        }
        //alright! Now that we have our entities, we must now handle 3 cases: Updating entities we already know are there, adding new entities, and removing ones not present
        List<int> entityIDsNotIncludedInPacket = new List<int>(locallyLoadedEntities.Keys); //we store each ID in the iteration; then, in a final go, we compare it to the locally loaded entities to remove the ones that don't appear
        foreach (PhysicalObject e in extractedEntities)
        {
            entityIDsNotIncludedInPacket.Remove(e.EntityID);
            bool entityLoaded = locallyLoadedEntities.TryGetValue(e.EntityID, out PhysicalObject localEntity);
            if (entityLoaded)
            {
                localEntity.UpdateEntityUsingHarvestedData(e);
            }
            else
            {
                locallyLoadedEntities.Add(e.EntityID, e);
            }
        }
        foreach (int i in entityIDsNotIncludedInPacket)
        {
            locallyLoadedEntities.Remove(i);
        }
    }

    public void ProcessToTrackBytes(byte[] data)
    {
        TrackedEntityID = Guildleader.Convert.ToInt(data);
    }

    public Color32 GetLightAtPosition(Int3 position, Int3 directionFacing)
    {
        int zPosRelative = position.z - ChunkRenderManager.playerPos.z;

        switch (zPosRelative)
        {
            case -2:
                if (directionFacing.z > 0)
                {
                    return new Color32(180, 150, 150, 200);
                }
                return new Color32(130, 100, 100, 200);
            case -1:
                if (directionFacing.z > 0)
                {
                    return new Color32(220, 200, 200, 255);
                }
                return new Color32(200, 180, 180, 255);
            case 0:
                return new Color32(255, 255, 255, 255);
            case 1:
                if (directionFacing.z > 0)
                {
                    return new Color32(255, 255, 255, 230);
                }
                return new Color32(200, 200, 200, 230);
            case 2:
                if (directionFacing.z > 0)
                {
                    return new Color32(200, 200, 255, 150);
                }
                return new Color32(150, 150, 200, 150);
            case 3: //for top of top layer tiles maybe
                return new Color32(200, 200, 255, 150);
            default:
                return new Color32(0, 0, 0, 0);
                
        }

    }

    public void UnloadDistantChunkData(Int3 chunkPos, int cutoffRange)
    {
        List<Int3> chunksToRemove = new List<Int3>();
        foreach (var xdic in allChunks)
        {
            foreach (var ydic in xdic.Value)
            {
                foreach (var zdic in ydic.Value)
                {
                    int dist = Math.Max(Math.Max(Math.Abs(chunkPos.x - xdic.Key), Math.Abs(chunkPos.y - ydic.Key)), Math.Abs(chunkPos.z - zdic.Key));
                    if (dist > cutoffRange)
                    {
                        chunksToRemove.Add(new Int3(xdic.Key, ydic.Key, zdic.Key));
                    }
                }
            }
        }

        foreach (Int3 toRemove in chunksToRemove)
        {
            allChunks[toRemove.x][toRemove.y].Remove(toRemove.z);
        }

        List<int> subDictsToRemovea = new List<int>();
        List<int> subDictsToRemoveb = new List<int>();
        foreach (var xdic in allChunks)
        {
            foreach (var ydic in xdic.Value)
            {
                if (ydic.Value.Count == 0)
                {
                    subDictsToRemovea.Add(ydic.Key);
                }
            }
            foreach (int key in subDictsToRemovea)
            {
                xdic.Value.Remove(key);
            }
            subDictsToRemovea.Clear();
            if (xdic.Value.Count == 0)
            {
                subDictsToRemoveb.Add(xdic.Key);
            }
        }
        foreach (int key in subDictsToRemoveb)
        {
            allChunks.Remove(key);
        }
    }

    public override Chunk GetChunkNotYetLoaded(int x, int y, int z)
    {
        Chunk c = new Chunk(new Int3(x,y,z));
        c.InitializeNotLoaded();
        return c;
    }
}
