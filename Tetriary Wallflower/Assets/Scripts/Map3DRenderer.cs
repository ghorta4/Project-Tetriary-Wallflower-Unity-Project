using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;

public class VoxelRenderer3D
{
    public Mesh ownMesh;

    public GameObject thisObject;
    public MeshRenderer mr;
    MeshFilter mf;

    public const float sca = SessionManager.unitScaling;
    const float zSca = SessionManager.zScaleStretch;

    public bool needsMeshPushed, beingUpdated, isBeingMeshPushed;

    const int expectedBlocksToStore = Chunk.defaultx * Chunk.defaulty * Chunk.defaultz;

    int lastCreatedFaceNumber;

    public List<Vector3> vertices; public List<int> triangles; public List<Vector2> UVs; public List<Color> vertColors;

    static Dictionary<int, Vector2[]> QuickUVAccess = new Dictionary<int, Vector2[]>(); //used in an attempt to speed up the bottleneck that is UV lookup

    public void Initialize(int chunkMultiplier)
    {
        ownMesh = new Mesh();
        vertices = new List<Vector3>(expectedBlocksToStore * 8 * chunkMultiplier);
        triangles = new List<int>(expectedBlocksToStore * 8 * chunkMultiplier);
        UVs = new List<Vector2>(expectedBlocksToStore * 4 * chunkMultiplier);
        vertColors = new List<Color>(expectedBlocksToStore * 4 * chunkMultiplier);
        thisObject = new GameObject("VoxelRenderer");
        thisObject.transform.localScale = Vector3.one * sca;
        mr = thisObject.AddComponent<MeshRenderer>();
        mf = thisObject.AddComponent<MeshFilter>();

        mr.material = ImageLibrary.tilemapMaterial;
        mr.material.SetFloat("PixelSnap", 1);

        mf.mesh = ownMesh;
    }

    public void CreateUpperFace(Vector3 position, short id, short variant)
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
    public void CreateLowerFace(Vector3 position, short id, short variant)
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
    public void CreateLeftFace(Vector3 position, short id, short variant)
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
    public void CreateRightFace(Vector3 position, short id, short variant)
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
    public void CreateTopFace(Vector3 position, short id, short variant)
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
    public void CreateBottomFace(Vector3 position, short id, short variant)
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

}

public class MapChunkRenderer : VoxelRenderer3D
{
    public Int3 targetChunk;

    bool[,,] opacityMap; //filled at the start of each refresh to reduce the 'gettile' calls. includes 1 tile on each side as a 'buffer'.

    MapChunkRenderer() //just to stop other functions from calling this when grabbing from recycling is a better idea
    {

    }

    public static MapChunkRenderer GrabMapRenderer(Int3 position)
    {
        MapChunkRenderer targ = null;
        if (recycling.Count > 0)
        {
            targ = recycling.Dequeue();
            targ.thisObject.SetActive(true);
            targ.vertices.Clear();
            targ.triangles.Clear();
            targ.UVs.Clear();
            targ.vertColors.Clear();
        }
        else
        {
            targ = new MapChunkRenderer();
            targ.Initialize(1);
        }
        targ.targetChunk = position;
        targ.thisObject.transform.position = new Vector3(position.x * Chunk.defaultx, position.y * Chunk.defaulty, position.z * Chunk.defaultz) * sca;
        return targ;
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
    }

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

    void AddColorsAtPosition(Int3 blockPos, Int3 faceDirection)
    {
        Color32 targetCol = SessionManager.world.GetLightAtPosition(blockPos, faceDirection);
        for (int i = 0; i < 4; i++)
        {
            vertColors.Add(targetCol);
        }
    }

    public void Cleanup()
    {
        thisObject.SetActive(false);
        recycling.Enqueue(this);
    }

    static Queue<MapChunkRenderer> recycling = new Queue<MapChunkRenderer>();
}

public class FFXRenderer : VoxelRenderer3D
{
    public Int3 cornerPosition;

    Color32[,,] colorMap; //controlls color and opacity (a channel). a of 0 is transparent and not given a cube shape.

    public FFXRenderer()
    {
        Initialize(3);
    }

    public void TestColorMap()
    {
        colorMap = new Color32[Chunk.defaultx * 2, Chunk.defaulty * 2, Chunk.defaultz * 3];
        colorMap[2, 2, 2] = new Color32(255, 0, 0, 100);
        colorMap[2, 2, 1] = new Color32(255, 0, 0, 100);
        colorMap[2, 2, 3] = new Color32(255, 0, 0, 200);
        colorMap[2, 3, 2] = new Color32(255, 255, 255, 200);
        colorMap[2, 1, 2] = new Color32(255, 255, 255, 255);
        colorMap[3, 2, 2] = new Color32(0, 0, 255, 255);
        colorMap[1, 1, 1] = new Color32(0, 0, 0, 230);
    }

    public void RefreshColorMap()
    {
        for (int x = 0; x < colorMap.GetLength(0)-2; x++)
        {
            for (int y = 0; y < colorMap.GetLength(1)-2; y++)
            {
                for (int z = 0; z < colorMap.GetLength(2) - 2; z++)
                {
                    if (colorMap[x, y, z].a <= 0)
                    {
                        continue;
                    }

                    CreateCubeMesh(new Int3(x, y, z), new Vector3(x, y, z), 6, 0);

                }
            }
        }
    }

    void CreateCubeMesh(Int3 positionWithinArray, Vector3 drawPosition, short blockID, short blockVariant)
    {
        List<Color32> neighbors = new List<Color32> {
            colorMap[positionWithinArray.x+1, positionWithinArray.y+1, positionWithinArray.z+2],
            colorMap[positionWithinArray.x+1, positionWithinArray.y+1, positionWithinArray.z],
            colorMap[positionWithinArray.x, positionWithinArray.y+1, positionWithinArray.z+1],
            colorMap[positionWithinArray.x+2, positionWithinArray.y+1, positionWithinArray.z+1],
            colorMap[positionWithinArray.x+1, positionWithinArray.y+2, positionWithinArray.z+1],
            colorMap[positionWithinArray.x+1, positionWithinArray.y, positionWithinArray.z+1],
        };

        Color32 col = colorMap[positionWithinArray.x, positionWithinArray.y, positionWithinArray.z];
        if (true || neighbors[0].a < 127)
        {
            CreateUpperFace(drawPosition, blockID, blockVariant);
            AddColor(col);
        }
        if (true || neighbors[1].a < 127)
        {
            CreateLowerFace(drawPosition, blockID, blockVariant);
            AddColor(col);
        }
        if (true || neighbors[2].a < 127)
        {
            CreateLeftFace(drawPosition, blockID, blockVariant);
            AddColor(col);
        }
        if (true || neighbors[3].a < 127)
        {
            CreateRightFace(drawPosition, blockID, blockVariant);
            AddColor(col);
        }
        if (true || neighbors[4].a < 127)
        {
            CreateTopFace(drawPosition, blockID, blockVariant);
            AddColor(col);
        }
        if (true || neighbors[5].a < 127)
        {
            CreateBottomFace(drawPosition, blockID, blockVariant);
            AddColor(col);
        }
    }

    void AddColor(Color32 blockColor)
    {
        for (int i = 0; i < 4; i++)
        {
            vertColors.Add(blockColor);
        }
    }
}