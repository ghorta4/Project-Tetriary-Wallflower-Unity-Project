using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;

public class TestMono : MonoBehaviour
{
    public VoxelRenderer3d targetChunk;

    public Vector3 position;
    public Vector3 tilePosition;
    public int upperLeftBlock;
    public int atPosition;

    public void Update()
    {
        Int3 pos = targetChunk.targetChunk;
        position = new Vector3(pos.x, pos.y, pos.z);
        upperLeftBlock = SessionManager.world.GetChunk(pos).GetAllTiles()[0,0,0].tileID;
        Int3 tilePos = new Int3(Mathf.FloorToInt(pos.x * Chunk.defaultx), Mathf.FloorToInt(pos.y * Chunk.defaulty), Mathf.FloorToInt(pos.z * Chunk.defaultz));
        atPosition = SessionManager.world.GetTileAtLocation(tilePos).tileID;
        tilePosition = new Vector3(tilePos.x, tilePos.y, tilePos.z);
    }
}
