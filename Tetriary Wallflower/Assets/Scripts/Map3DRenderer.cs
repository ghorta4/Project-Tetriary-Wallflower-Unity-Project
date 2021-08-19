using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;


public class VoxelRenderer3d
{
    public Mesh ownMesh;
    //public Vector3Int pos; //upper left of the map
    // public Vector3Int size;
    public Vector3Int targetChunk;

    public GameObject thisObject;
    public MeshRenderer mr;
    MeshFilter mf;

    List<Vector3> vertices; List<int> triangles; List<Vector2> UVs; List<Color> vertColors;

    const float sca = 1;//ImageLibrary.tileScale;

    const int expectedBlocksToStore = Chunk.defaultx * Chunk.defaulty * Chunk.defaultz;

    VoxelRenderer3d() //just to stop other functions from calling this when grabbing from recycling is a better idea
    {

    }

    public static VoxelRenderer3d grabVRenderer()
    {
        if (recycling.Count > 0)
        {
            VoxelRenderer3d target = recycling.Dequeue();
            target.mf.mesh.Clear();
            target.thisObject.SetActive(true);
            return target;
        }
        VoxelRenderer3d newGuy = new VoxelRenderer3d();
        newGuy.Initialize();
        return newGuy;
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

        //refreshEntireMap();
    }

    public void RefreshEntireMap()
    {
        for (int x = 0; x < Chunk.defaultx; x++)
        {
            for (int y = 0; y < Chunk.defaulty; y++)
            {
                for (int z = 0; z < Chunk.defaultz; z++)
                {
                    Int3 targetPos = new Int3(Chunk.defaultx * targetChunk.x + x, Chunk.defaulty * targetChunk.y + y, Chunk.defaultz * targetChunk.z + z);
                    SingleWorldTile target = SessionManager.world.GetTileAtLocation(targetPos);
                    if (!target.properties.tags.Contains("transparent"))
                    {
                        CreateCubeMesh(targetPos, new Vector3(x, y, z), target.tileID, target.variant);
                    }
                }
            }
        }
        thisObject.transform.position = new Vector3(targetChunk.x * Chunk.defaultx, targetChunk.y * Chunk.defaulty, targetChunk.z * Chunk.defaultz) * sca;
        pushMesh();
    }

    int lastCreatedFaceNumber;
    void CreateCubeMesh(Int3 blockPosition, Vector3 drawPosition, short blockID, short blockVariant)
    {
        List<SingleWorldTile> neighbors = new List<SingleWorldTile> {
            SessionManager.world.GetTileAtLocation(blockPosition + new Int3(0,0,1)),
            SessionManager.world.GetTileAtLocation(blockPosition + new Int3(0,0,-1)),
            SessionManager.world.GetTileAtLocation(blockPosition + new Int3(-1,0,0)),
            SessionManager.world.GetTileAtLocation(blockPosition + new Int3(1,0,0)),
            SessionManager.world.GetTileAtLocation(blockPosition + new Int3(0,1,0)),
            SessionManager.world.GetTileAtLocation(blockPosition + new Int3(0,-1,0))
        };

        if (neighbors[0].properties.tags.Contains("transparent"))
        {
            CreateUpperFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(0, 0, 1);
            addColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[1].properties.tags.Contains("transparent"))
        {
            CreateLowerFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(0, 0, -1);
            addColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[2].properties.tags.Contains("transparent"))
        {
            CreateLeftFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(-1, 0, 0);
            addColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[3].properties.tags.Contains("transparent"))
        {
            CreateRightFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(1, 0, 0);
            addColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[4].properties.tags.Contains("transparent"))
        {
            CreateTopFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(0, 1, 0);
            addColorsAtPosition(blockPosition + faceDir, faceDir);
        }
        if (neighbors[5].properties.tags.Contains("transparent"))
        {
            CreateBottomFace(drawPosition, blockID, blockVariant);
            Int3 faceDir = new Int3(0, -1, 0);
            addColorsAtPosition(blockPosition + faceDir, faceDir);
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
        addTrianglesToLastFace();
        addUVsOfID(id, variant);
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
        addTrianglesToLastFace();
        addUVsOfID(id, variant);
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
        addTrianglesToLastFace();
        addUVsOfID(id, variant);
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
        addTrianglesToLastFace();
        addUVsOfID(id, variant);
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
        addTrianglesToLastFace();
        addUVsOfID(id, variant);
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
        addTrianglesToLastFace();
        addUVsOfID(id, variant);
    }

    void addTrianglesToLastFace()
    {
        triangles.AddRange(new int[]
        { lastCreatedFaceNumber * 4+2, lastCreatedFaceNumber * 4+1, lastCreatedFaceNumber * 4+0,
        lastCreatedFaceNumber * 4+1, lastCreatedFaceNumber * 4+2, lastCreatedFaceNumber * 4+3});
        lastCreatedFaceNumber++;
    }
    void addUVsOfID(short id, short variant)
    {
        string tileImageName = TileLibrary.tileLib[id].variantsAndWeights[variant].Item1;
        UVs.AddRange(ImageLibrary.translateTileNameToPositionOnTilemap[tileImageName]);
    }
    void addColorsAtPosition(Int3 blockPos, Int3 faceDirection)
    {
        Int3 targetCol = SessionManager.world.GetLightAtPosition(blockPos, faceDirection);
        Color32 convertedColor = new Color32((byte)targetCol.x, (byte)targetCol.y, (byte)targetCol.z, 255);
        for (int i = 0; i < 4; i++)
        {
            vertColors.Add(convertedColor);
        }
    }

    public void pushMesh()
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

    public void cleanup()
    {
        thisObject.SetActive(false);
        recycling.Enqueue(this);
    }

    static Queue<VoxelRenderer3d> recycling = new Queue<VoxelRenderer3d>();
}

