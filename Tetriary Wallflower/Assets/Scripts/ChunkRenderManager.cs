using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using Guildleader.Entities;
using Guildleader.Entities.BasicEntities;
using System.Threading;
using System;

public static class ChunkRenderManager
{
    public static bool RendererRequiresUpdates;
    static List<VoxelRenderer3d> RenderChunks = new List<VoxelRenderer3d>();

    const float sca = SessionManager.unitScaling;

    public static Int3 playerPos;

    static Thread meshGeneratorThread;

    public static void Initialize()
    {
        meshGeneratorThread = new Thread(BuildMeshThread);
        meshGeneratorThread.Start();
    }

    public static void Update()
    {
        PhysicalObject tracked = SessionManager.world.TrackedEntity; //we need to check for this and make sure it exists, since block transparency requires the player z position to function
        if (tracked == null)
        {
            return;
        }
        playerPos = tracked.worldPositon;
        AddChunksToList();
        PushFinishedChunks();
        RemoveIrrelevantChunks();
    }

    public static void AddChunksToList()
    {
        List<Chunk> chunks = SessionManager.world.GetAllChunksLoaded();
        foreach (Chunk c in chunks)
        {
            if (RenderChunks.Find(x => x.targetChunk == c.chunkPos) == null)
            {
                VoxelRenderer3d newChunk = VoxelRenderer3d.GrabVRenderer(c.chunkPos);
                Int3 pos = c.chunkPos;
                newChunk.thisObject.transform.position = new Vector3(pos.x * sca * Chunk.defaultx, pos.y * sca * Chunk.defaulty, pos.z * sca * Chunk.defaultz * SessionManager.zScaleStretch);
                RenderChunks.Add(newChunk);

                RendererRequiresUpdates = true;
                return;
            }
        }
    }
    public static void UpdateAllChunks()
    {
        List<VoxelRenderer3d> clonedProcessingChunks = new List<VoxelRenderer3d>(RenderChunks);
        foreach (VoxelRenderer3d vr3d in clonedProcessingChunks)
        {
            try
            {
                if (vr3d.isBeingMeshPushed)
                {
                    continue;
                }
                vr3d.beingUpdated = true;
                vr3d.needsMeshPushed = false;
                vr3d.RefreshEntireMap();
                vr3d.needsMeshPushed = true;
                vr3d.beingUpdated = false;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                RendererRequiresUpdates = true;
            }

        }
    }
    public static void RemoveIrrelevantChunks()
    {
        SessionManager.world.UnloadDistantChunkData(WorldDataStorageModuleGeneric.GetChunkPositionBasedOnTilePosition(playerPos.x, playerPos.y, playerPos.z), 2);
        List<Chunk> allChunksLoaded = SessionManager.world.GetAllChunksLoaded();
        List<VoxelRenderer3d> toRemove = new List<VoxelRenderer3d>();
        foreach (VoxelRenderer3d vox in RenderChunks)
        {
            if (vox.beingUpdated || vox.needsMeshPushed)
            {
                continue;
            }
            Chunk c = allChunksLoaded.Find(x => x.chunkPos == vox.targetChunk);
            if (c == null)
            {
                vox.Cleanup();
                toRemove.Add(vox);
            }
        }
        foreach (VoxelRenderer3d vox in toRemove)
        {
            RenderChunks.Remove(vox);
        }
    }
    static void PushFinishedChunks()
    {
        foreach (VoxelRenderer3d Renderer in RenderChunks)
        {
            Renderer.isBeingMeshPushed = true;
            if (Renderer.needsMeshPushed)
            {
                Renderer.PushMesh();
                Renderer.needsMeshPushed = false;
            }
            Renderer.isBeingMeshPushed = false;
        }
    }

    static void BuildMeshThread()
    {
        while (!SessionManager.QuitApplication)
        {
            if (!RendererRequiresUpdates)
            {
                Thread.Sleep(250);
            //    continue;
            }
            RendererRequiresUpdates = false;
            UpdateAllChunks();
        }
    }
}
