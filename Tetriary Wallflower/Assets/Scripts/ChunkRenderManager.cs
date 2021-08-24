using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;

public static class ChunkRenderManager
{
    public static bool RendererRequiresUpdates;
    static List<VoxelRenderer3d> RenderChunks = new List<VoxelRenderer3d>();

    const float sca = SessionManager.unitScaling;

    public static void Update()
    {
        if (!RendererRequiresUpdates)
        {
            return;
        }
        AddChunksToList();
        UpdateAllChunks();
        RendererRequiresUpdates = false;
    }

    public static void AddChunksToList()
    {
        List<Chunk> chunks = SessionManager.world.GetAllChunksLoaded();
        foreach (Chunk c in chunks)
        {
            if (RenderChunks.Find(x => x.targetChunk == c.chunkPos) == null)
            {
                VoxelRenderer3d newChunk = VoxelRenderer3d.GrabVRenderer();
                Int3 pos = newChunk.targetChunk = c.chunkPos;
                newChunk.thisObject.transform.position = new Vector3(pos.x * sca * Chunk.defaultx, pos.y * sca * Chunk.defaulty, pos.z * sca * Chunk.defaultz * SessionManager.zScaleStretch);
                RenderChunks.Add(newChunk);
            }
        }
    }
    public static void UpdateAllChunks()
    {
        foreach (VoxelRenderer3d vr3d in RenderChunks)
        {
            vr3d.RefreshEntireMap();
        }
    }
    public static void RemoveIrrelevantChunks()
    {
        WorldManager.currentWorld.UnloadDistantChunkData(SessionManager.world.locallyLoadedEntities[SessionManager.world.TrackedEntityID].currentChunk, 1);
    }
}
