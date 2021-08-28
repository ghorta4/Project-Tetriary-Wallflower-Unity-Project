using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;


public class VoxelRenderer3d
{
    public Mesh ownMesh;
    //public Vector3Int pos; //upper left of the map
    // public Vector3Int size;
    public Int3 targetChunk;

    public GameObject thisObject;
    public MeshRenderer mr;
    MeshFilter mf;

    List<Vector3> vertices; List<int> triangles; List<Vector2> UVs; List<Color> vertColors;

    const float sca = SessionManager.unitScaling;
    const float zSca = SessionManager.zScaleStretch;

    const int expectedBlocksToStore = Chunk.defaultx * Chunk.defaulty * Chunk.defaultz;

    public bool needsMeshPushed, beingUpdated, isBeingMeshPushed;

    bool[,,] opacityMap; //filled at the start of each refresh to reduce the 'gettile' calls. includes 1 tile on each side as a 'buffer'.

    VoxelRenderer3d() //just to stop other functions from calling this when grabbing from recycling is a better idea
    {

    }

    public static VoxelRenderer3d GrabVRenderer(Int3 position)
    {
        VoxelRenderer3d targ = null;
        if (recycling.Count > 0)
        {
            targ = recycling.Dequeue();
            targ.mf.mesh.Clear();
            targ.thisObject.SetActive(true);
            targ.vertices.Clear();
            targ.triangles.Clear();
            targ.UVs.Clear();
            targ.vertColors.Clear();
        }
        else
        {
            targ = new VoxelRenderer3d();
            targ.Initialize();
        }
        targ.targetChunk = position;
        targ.thisObject.transform.position = new Vector3(position.x * Chunk.defaultx, position.y * Chunk.defaulty, position.z * Chunk.defaultz) * sca;
        return targ;
    }

    void Initialize()
    {
        ownMesh = new Mesh();
        vertices = new List<Vector3>(expectedBlocksToStore * 8);
        triangles = new List<int>(expectedBlocksToStore * 8);
        UVs = new List<Vector2>(expectedBlocksToStore * 4);
        vertColors = new List<Color>(expectedBlocksToStore * 4);
        thisObject = new GameObject("VoxelRenderer");
        thisObject.transform.localScale = Vector3.one * sca;
        mr = thisObject.AddComponent<MeshRenderer>();
        mf = thisObject.AddComponent<MeshFilter>();

        mr.material = ImageLibrary.tilemapMaterial;
        mr.material.SetFloat("PixelSnap", 1);

        mf.mesh = ownMesh;
    }

    public void RefreshEntireMap()
    {
        opacityMap = new bool[Chunk.defaultx + 2, Chunk.defaulty + 2, Chunk.defaultz + 2];
        SingleWorldTile[,,] surroundingTiles = new SingleWorldTile[Chunk.defaultx + 2, Chunk.defaulty + 2, Chunk.defaultz + 2];

        //sets the center of 'surroundingTiles' to this chunk's tiles
        SingleWorldTile[,,] thisChunkTiles = SessionManager.world.GetChunk(targetChunk).GetAllTiles();
        for (int i = 1; i < thisChunkTiles.GetLength(0) + 1; i++)
        {
            for (int j = 1; j < thisChunkTiles.GetLength(1) + 1; j++)
            {
                for (int k = 1; k < thisChunkTiles.GetLength(2) + 1; k++)
                {
                    surroundingTiles[i, j, k] = thisChunkTiles[i - 1, j - 1, k - 1];
                  //  surroundingTiles[i, j, k] = SessionManager.world.GetTileAtLocation(targetChunk.x * Chunk.defaultx + i-1, targetChunk.y * Chunk.defaulty + j - 1, targetChunk.z * Chunk.defaultz + k - 1);
                }
            }
        }

        //sets the opacity map
        for (int i = 0; i < opacityMap.GetLength(0); i++)
        {
            for (int j = 0; j < opacityMap.GetLength(1); j++)
            {
                for (int k = 0; k < opacityMap.GetLength(2); k++)
                {
                    opacityMap[i, j, k] = surroundingTiles[i,j,k] == null || surroundingTiles[i, j, k].properties.tags.Contains("transparent");
                }
            }
        }

        for (int x = 0; x < Chunk.defaultx; x++)
        {
            for (int y = 0; y < Chunk.defaulty; y++)
            {
                for (int z = 0; z < Chunk.defaultz; z++)
                {
                    if (opacityMap[x+1, y+1, z+1]) //we've already determined tile transparency above, so don't bother reca;culating if we should draw stuff
                    {
                        continue;
                    }
                    Int3 targetPos = new Int3(Chunk.defaultx * targetChunk.x + x, Chunk.defaulty * targetChunk.y + y, Chunk.defaultz * targetChunk.z + z);
                    SingleWorldTile target = surroundingTiles[x + 1, y + 1, z + 1];
                    if (!target.properties.tags.Contains("transparent"))
                    {
                        CreateCubeMesh(targetPos, new Int3(x,y,z), new Vector3(x, y, z), target.tileID, target.variant);
                    }
                }
            }
        }
       // PushMesh();
    }

    int lastCreatedFaceNumber;
    void CreateCubeMesh(Int3 blockPosition, Int3 positionWithinArray, Vector3 drawPosition, short blockID, short blockVariant)
    {
        List<bool> neighbors = new List<bool> {
            opacityMap[positionWithinArray.x+1, positionWithinArray.y+1, positionWithinArray.z+2],
            opacityMap[positionWithinArray.x+1, positionWithinArray.y+1, positionWithinArray.z],
            opacityMap[positionWithinArray.x, positionWithinArray.y+1, positionWithinArray.z+1],
            opacityMap[positionWithinArray.x+2, positionWithinArray.y+1, positionWithinArray.z+1],
            opacityMap[positionWithinArray.x+1, positionWithinArray.y+2, positionWithinArray.z+1],
            opacityMap[positionWithinArray.x+1, positionWithinArray.y, positionWithinArray.z+1],
        };

        if (neighbors[0])
        {
            CreateUpperFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(0, 0, 1);
            AddColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[1])
        {
            CreateLowerFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(0, 0, -1);
            AddColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[2])
        {
            CreateLeftFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(-1, 0, 0);
            AddColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[3])
        {
            CreateRightFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(1, 0, 0);
            AddColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[4])
        {
            CreateTopFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(0, 1, 0);
            AddColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[5])
        {
            CreateBottomFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(0, -1, 0);
            AddColorsAtPosition(blockPosition + faceDir, faceDir);
        }
    }

    void CreateUpperFace(Vector3 position, short id, short variant)
    {
        vertices.AddRange(
        new Vector3[]{
                (position + new Vector3(0,0,1)) ,
                (position + new Vector3(0,1,1)) ,
                (position + new Vector3(1,0,1)) ,
                (position + new Vector3(1,1,1))
         });
        AddTrianglesToLastFace();
        AddUVsOfID(id, variant);
    }
    void CreateLowerFace(Vector3 position, short id, short variant)
    {
        vertices.AddRange(
        new Vector3[]{
                (position + new Vector3(1,1,0)) ,
                (position + new Vector3(0,1,0)) ,
                (position + new Vector3(1,0,0)) ,
                (position + new Vector3(0,0,0))
         });
        AddTrianglesToLastFace();
        AddUVsOfID(id, variant);
    }
    void CreateLeftFace(Vector3 position, short id, short variant)
    {
        vertices.AddRange(
        new Vector3[]{
                (position + new Vector3(0,0,0)) ,
                (position + new Vector3(0,1,0)) ,
                (position + new Vector3(0,0,1)) ,
                (position + new Vector3(0,1,1))
         });
        AddTrianglesToLastFace();
        AddUVsOfID(id, variant);
    }
    void CreateRightFace(Vector3 position, short id, short variant)
    {
        vertices.AddRange(
        new Vector3[]{
                (position + new Vector3(1,0,0)) ,
                (position + new Vector3(1,0,1)) ,
                (position + new Vector3(1,1,0)) ,
                (position + new Vector3(1,1,1))
         });
        AddTrianglesToLastFace();
        AddUVsOfID(id, variant);
    }
    void CreateTopFace(Vector3 position, short id, short variant)
    {
        vertices.AddRange(
        new Vector3[]{
                (position + new Vector3(0,1,0)) ,
                (position + new Vector3(1,1,0)) ,
                (position + new Vector3(0,1,1)) ,
                (position + new Vector3(1,1,1))
         });
        AddTrianglesToLastFace();
        AddUVsOfID(id, variant);
    }
    void CreateBottomFace(Vector3 position, short id, short variant)
    {
        vertices.AddRange(
        new Vector3[]{
                (position + new Vector3(0,0,0)) ,
                (position + new Vector3(0,0,1)) ,
                (position + new Vector3(1,0,0)) ,
                (position + new Vector3(1,0,1))
         });
        AddTrianglesToLastFace();
        AddUVsOfID(id, variant);
    }

    void AddTrianglesToLastFace()
    {
        triangles.AddRange(new int[]
        { lastCreatedFaceNumber * 4+2, lastCreatedFaceNumber * 4+1, lastCreatedFaceNumber * 4+0,
        lastCreatedFaceNumber * 4+1, lastCreatedFaceNumber * 4+2, lastCreatedFaceNumber * 4+3});
        lastCreatedFaceNumber++;
    }
    static Dictionary<int, Vector2[]> QuickUVAccess = new Dictionary<int, Vector2[]>(); //used in an attempt to speed up the bottleneck that is UV lookup
    void AddUVsOfID(short id, short variant)
    {
        int UVHash = (ushort)id | (variant << sizeof(short));
        QuickUVAccess.TryGetValue(UVHash, out Vector2[] tableresult);
        if (tableresult != null)
        {
            UVs.AddRange(tableresult);
        }
        else
        {
            string tileImageName = TileLibrary.tileLib[id].variantsAndWeights[variant].Item1;
            Vector2[] result = ImageLibrary.translateTileNameToPositionOnTilemap[tileImageName];
            UVs.AddRange(result);
            QuickUVAccess.Add(UVHash, result);
        }
    }
    void AddColorsAtPosition(Int3 blockPos, Int3 faceDirection)
    {
        Color32 targetCol = SessionManager.world.GetLightAtPosition(blockPos, faceDirection);
        for (int i = 0; i < 4; i++)
        {
            vertColors.Add(targetCol);
        }
    }

    public void PushMesh()
    {
        ownMesh.Clear();
        ownMesh.vertices = vertices.ToArray();
        ownMesh.uv = UVs.ToArray();
        ownMesh.triangles = triangles.ToArray();
        ownMesh.colors = vertColors.ToArray();
        ownMesh.Optimize();

        vertices.Clear();
        UVs.Clear();
        triangles.Clear();
        vertColors.Clear();
        lastCreatedFaceNumber = 0;
    }

    public void Cleanup()
    {
        thisObject.SetActive(false);
      //  recycling.Enqueue(this);
    }

    static Queue<VoxelRenderer3d> recycling = new Queue<VoxelRenderer3d>();
}

