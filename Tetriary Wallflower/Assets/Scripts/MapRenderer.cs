using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Guildleader;

public class MapPlane
{
    public Mesh mapSliceMesh = new Mesh();
    public Vector3Int pos; //upper left of the map
    public int xSize, ySize;

    public GameObject thisObject;
    public MeshRenderer mr;
    public MeshFilter mf;

    public bool requiresRefreshDueToMissingBlock;
    public DateTime lastRefreshRequest;

    const float sca = ImageLibrary.tileScale;
    //  List<Vector2> storedVerts;
    public List<List<Vector2>> storedVerts2D = new List<List<Vector2>>();

    public MapPlane(Vector3Int upperLeftPos, int tilesToDrawx, int tilesToDrawy)
    {
        pos = upperLeftPos;
        xSize = tilesToDrawx; ySize = tilesToDrawy;
        List<Vector3> vertsStorage = new List<Vector3>();
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                vertsStorage.Add(new Vector3(x, y, 0) * sca);
                vertsStorage.Add(new Vector3(x + 1, y, 0) * sca);
                vertsStorage.Add(new Vector3(x, y + 1, 0) * sca);
                vertsStorage.Add(new Vector3(x + 1, y + 1, 0) * sca);
            }
        }
        List<int> triangleStorage = new List<int> { };
        for (int i = 0; i < xSize * ySize; i++)
        {
            int ti = 4 * i;
            triangleStorage.AddRange(new int[]
            { ti+2, ti+1, ti+0,
            ti + 1, ti+2, ti+3});
        }
        mapSliceMesh.vertices = vertsStorage.ToArray();
        mapSliceMesh.triangles = triangleStorage.ToArray();

        thisObject = new GameObject();
        mr = thisObject.AddComponent<MeshRenderer>();
        mf = thisObject.AddComponent<MeshFilter>();
        mf.mesh = mapSliceMesh;

        refreshMeshColors();

        mr.material = ImageLibrary.tilemapMaterial;
        mr.material.SetFloat("PixelSnap", 1);
    }

    int tileSize { get { return ImageLibrary.tileSizeInPixels; } }

    public Vector3Int getSubPositionOnSlice(int x, int y)
    {
        return new Vector3Int(pos.x + x, pos.y + y, pos.z);
    }

    public void refreshMeshColors()
    {
        storedVerts2D = new List<List<Vector2>>(xSize);
        for (int x = 0; x < xSize; x++)
        {
            storedVerts2D.Add(new List<Vector2>(ySize));
            for (int y = 0; y < ySize; y++)
            {
                Int3 tilePos = new Int3(pos.x + x, pos.y + y, pos.z);
                storedVerts2D[x].AddRange(GetUVsOfTileAtLocation(tilePos));
            }
        }

        pushUVListUpdate();
    }
    public void shiftSlice(Vector2Int xy)
    {
        pos.x += xy.x;
        //start with x shift, since it's just a line, then go to y shift

        if (xy.x > 0)
        {
            for (int x = 0; x < xy.x; x++)
            {
                storedVerts2D.RemoveAt(0);
                List<Vector2> newRow = new List<Vector2>(ySize * 4);
                for (int y = 0; y < ySize; y++)
                {
                    newRow.AddRange(GetUVsOfTileAtLocation(new Int3(pos.x + x - xy.x + xSize, pos.y + y, pos.z)));
                }
                storedVerts2D.Add(newRow);
            }
        }
        else if (xy.x < 0)
        {
            for (int x = 0; x < -xy.x; x++)
            {
                List<Vector2> newRow = new List<Vector2>(ySize * 4);
                for (int y = 0; y < ySize; y++)
                {
                    newRow.AddRange(GetUVsOfTileAtLocation(new Int3(pos.x - x - xy.x - 1, pos.y + y, pos.z)));
                }
                storedVerts2D.Insert(0, newRow);

                storedVerts2D.RemoveAt(storedVerts2D.Count - 1);
            }
        }

        pos.y += xy.y;

        if (xy.y > 0)
        {
            for (int x = 0; x < xSize; x++)
            {
                List<Vector2> list = storedVerts2D[x];
                for (int y = 0; y < xy.y; y++)
                {
                    list.RemoveRange(0, 4);

                    list.AddRange(GetUVsOfTileAtLocation(new Int3(pos.x + x, pos.y + ySize + -xy.y + y, pos.z)));
                }
            }
        }
        else if (xy.y < 0)
        {
            for (int x = 0; x < xSize; x++)
            {
                List<Vector2> list = storedVerts2D[x];
                for (int y = 0; y < -xy.y; y++)
                {
                    list.RemoveRange(list.Count - 4, 4);

                    list.InsertRange(0, GetUVsOfTileAtLocation(new Int3(pos.x + x, pos.y + xy.y - y + 1, pos.z)));
                }
            }
        }

        pushUVListUpdate();
    }

    public Vector2[] GetUVsOfTileAtLocation(Int3 location)
    {
        SingleWorldTile swt = SessionManager.world.GetTileAtLocation(location);
        short numb = swt.tileID;
        if (numb == 5)
        {
            requiresRefreshDueToMissingBlock = true;
            lastRefreshRequest = DateTime.Now;
        }
        string tileImageName = TileLibrary.tileLib[numb].variantsAndWeights[(int)swt.variant].Item1;
        return ImageLibrary.translateTileNameToPositionOnTilemap[tileImageName];
    }
    public Vector2[] formUVList()
    {
        List<Vector2> wip = new List<Vector2>(xSize * ySize * 4);

        foreach (List<Vector2> list in storedVerts2D)
        {
            //foreach (Vector2 v2 in list)
            //{
            //    wip[inc] = v2;
            //    inc++;
            //}
            wip.AddRange(list);

        }
        return wip.ToArray();
    }

    public void pushUVListUpdate()
    {
        mf.mesh.uv = formUVList();
    }

    public void cleanup()
    {
        GameObject.Destroy(thisObject);
    }
}
