using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using Guildleader.Entities;
using Guildleader.Entities.BasicEntities;

public class ClientWorld : WorldDataStorageModuleGeneric
{
    public Dictionary<int, PhysicalObject> locallyLoadedEntities = new Dictionary<int, PhysicalObject>();//stored as a dictionary to improve speed when handling data from the server

    public int TrackedEntityID = int.MaxValue;

    public void Initialize()
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
        allChunks[pos.x][pos.y][pos.z] = c;
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
}
