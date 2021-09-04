using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using System;

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

        ClientFFXNodeManger.AllNodes.Add(new ExpandingShellNode() { timeAlive = 0, totalLifespan = 2f, maxSize = 6, shellThickness = 3f, r = 240, g = 130, b = 10, a = 230 });

        ClientFFXNodeManger.AllNodes.Add(new ExpandingShellNode() { timeAlive = 0, totalLifespan = 2f, maxSize = 12, shellThickness = 1.4f, r = 90, g = 90, b = 90, a = 120 });
    }

    private void Update()
    {
        ffx.colorMap = ClientFFXNodeManger.GenerateColorMap();

        ffx.RefreshColorMap();
        ffx.PushMesh();

        ClientFFXNodeManger.Update(Time.deltaTime);
        ClientFFXNodeManger.GenerateColorMap();
    }
}

public static class ClientFFXNodeManger
{
    public static Int3 upperLeftCorner = new Int3(-Chunk.defaultx, -Chunk.defaulty, -Chunk.defaultz * 2);
    public static List<FFXNode> AllNodes = new List<FFXNode>();

    static Color32[,,] reservedArray = new Color32[Chunk.defaultx * 2, Chunk.defaulty * 2, Chunk.defaultz * 4]; //to save on memory

    public static Color32[,,] GenerateColorMap()
    {
        for (int i = 0; i < reservedArray.GetLength(0); i++)
        {
            for (int j = 0; j < reservedArray.GetLength(1); j++)
            {
                for (int k = 0; k < reservedArray.GetLength(2); k++)
                {
                    reservedArray[i, j, k] = new Color32();
                }
            }
        }

        foreach (FFXNode node in AllNodes)
        {
            ApplyNodeToArray(node);
        }

        return reservedArray;
    }

    //functions for drawing different nodes
    public enum NodeKey
    {
        ExpandingShell,
    }

    static void ApplyNodeToArray(FFXNode node)
    {
        
        switch (node.NodeKey())
        {
            case NodeKey.ExpandingShell:
                DrawExpandingShell( node as ExpandingShellNode);
                break;
        }
    }

    static void SetColor (int x, int y, int z, Color32 color)
    {
        int l0 = reservedArray.GetLength(0);
        int l1 = reservedArray.GetLength(1);
        int l2 = reservedArray.GetLength(2);

        if (x < 0 || x >= l0 || y < 0 || z < 0 || y >= l1 || z >= l2)
        {
            return;
        }
        reservedArray[x,y,z] = color;
    }

    //to make this super fast, only calculate the tiles included in a 1/8th wedge and copy it over to each other side (reduce math calls)

    static void DrawExpandingShell(ExpandingShellNode esn)
    {
        float fractionComplete = Mathf.Clamp01(esn.timeAlive / esn.totalLifespan);
        float currentRadius = Mathf.Sin(Mathf.Sqrt(fractionComplete) * Mathf.PI /2f) * esn.maxSize;
        Int3 relativeCenter = esn.center - upperLeftCorner;

        float lifeTimeBasedOpacityMult = 1;
        if (esn.timeAlive > esn.totalLifespan * 0.8f)
        {
            lifeTimeBasedOpacityMult = 1 - (esn.timeAlive - esn.totalLifespan * 0.6f) / (esn.totalLifespan * 0.4f);
        }
        for (int x = Mathf.FloorToInt(relativeCenter.x - currentRadius)-1; x < relativeCenter.x + 1; x++)
        {
            for (int y = Mathf.FloorToInt(relativeCenter.y - currentRadius)-1; y < relativeCenter.y + 1; y++)
            {
                for (int z = Mathf.FloorToInt(relativeCenter.z - currentRadius)-1; z < relativeCenter.z + 1; z++)
                {
                    float distance = Mathf.Sqrt(Mathf.Pow(relativeCenter.x - x,2) + Mathf.Pow(relativeCenter.y - y, 2) + Mathf.Pow(relativeCenter.z - z, 2));
                    if (distance < currentRadius - esn.shellThickness  || distance > currentRadius + 1)
                    {
                        continue;
                    }
                    float intensityMultiplier = 1;
                    if (distance <= currentRadius)
                    {
                        if (currentRadius > esn.shellThickness)
                        {
                            intensityMultiplier = (distance - esn.shellThickness) / (currentRadius - esn.shellThickness);
                        }
                        else
                        {
                            intensityMultiplier = distance / currentRadius;
                        }
                    }
                    else
                    {
                        intensityMultiplier = 1 - (distance - currentRadius);
                    }

                    intensityMultiplier *= lifeTimeBasedOpacityMult;
                    
                    for (int xm = 0; xm <= 1; xm++)
                    {
                        for (int ym = 0; ym <= 1; ym++)
                        {
                            for (int zm = 0; zm <= 1; zm++)
                            {
                                SetColor(x + xm * (2 * relativeCenter.x - 2 * x), y + ym * (2 * relativeCenter.y - 2 * y), z + zm * (2 * relativeCenter.z - 2 * z), new Color32(esn.r, esn.g, esn.b, (byte)Mathf.Floor(esn.a * intensityMultiplier)));
                            }
                        }
                    }
                }
            }
        }
    }

    public static void Update(float Timestep)
    {
        foreach (FFXNode n in AllNodes)
        {
            n.timeAlive += Timestep;
            if (n.timeAlive >= n.totalLifespan)
            {
                n.timeAlive = 0;
            }
        }
    }
}

public abstract class FFXNode
{
    public float timeAlive, totalLifespan;
    public abstract byte[] ConvertToBytes();
    public abstract ClientFFXNodeManger.NodeKey NodeKey();
}

public class ExpandingShellNode : FFXNode
{
    public byte r = 127, g = 127, b = 127, a = 127;
    public Int3 center = Int3.Zero;
    public float maxSize, shellThickness;

    public override ClientFFXNodeManger.NodeKey NodeKey()
    {
        return ClientFFXNodeManger.NodeKey.ExpandingShell;
    }

    public override byte[] ConvertToBytes()
    {
        throw new System.NotImplementedException();
    }
}
