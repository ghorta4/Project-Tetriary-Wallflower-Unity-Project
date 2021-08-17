using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;

public class ClientWorld : WorldDataStorageModuleGeneric
{
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
}
