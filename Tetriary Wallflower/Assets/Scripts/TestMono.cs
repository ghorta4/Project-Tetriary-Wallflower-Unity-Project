using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;

public class TestMono : MonoBehaviour
{

    FFXRenderer ffx;

    private void Start()
    {
        FileAccess.SetDefaultDirectory(FileAccess.AssetsFileLocation);

        ImageLibrary.LoadImageLibraries();
        TileLibrary.LoadTileLibrary();

        ffx = new FFXRenderer();
        ffx.TestColorMap();

    }

    private void Update()
    {
        ffx.RefreshColorMap();
        ffx.PushMesh();
    }
}
