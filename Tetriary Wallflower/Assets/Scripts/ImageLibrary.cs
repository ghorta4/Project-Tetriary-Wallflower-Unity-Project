using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using System.IO;

public static class ImageLibrary
{
    public const float tileScale = 0.1f;

    const int compiledTileMapsDesiredWidth = 50, compiledTileMapsDesiredHeight = 50; //in tiles
    public static Texture2D loadedTilemap;
    public static Material tilemapMaterial;
    public static Dictionary<string, Vector2[]> translateTileNameToPositionOnTilemap = new Dictionary<string, Vector2[]> { };
    public static Dictionary<string, Texture2D> pokemonSprites = new Dictionary<string, Texture2D> { };

    public static void LoadImageLibraries()
    {
        LoadTileLibrary();
        LoadSpriteLibrary();
    }

    public const int tileSizeInPixels = 20;
    static void LoadTileLibrary()
    {
        FileInfo[] allFiles = Guildleader.FileAccess.GetAllFilesInDirectory(Guildleader.FileAccess.TilesLocation);

        int lastWrittenTileLocation = 0;
        loadedTilemap = new Texture2D(compiledTileMapsDesiredWidth * tileSizeInPixels, compiledTileMapsDesiredHeight * tileSizeInPixels, TextureFormat.RGBA32, false);
        loadedTilemap.filterMode = FilterMode.Point;
        foreach (FileInfo fi in allFiles)
        {
            if (fi.Extension != ".png")
            {
                continue;
            }
            Texture2D loadedSprite = new Texture2D(1,1);
            byte[] loadedData = File.ReadAllBytes(fi.FullName);
            loadedSprite.LoadImage(loadedData);
            
            cutTileSpriteAndAddToLibrary(loadedSprite, fi.Name.Replace(".png", ""), ref lastWrittenTileLocation);
        }
        loadedTilemap.Apply();

        tilemapMaterial = new Material(Shader.Find("Terrain"));
        tilemapMaterial.SetFloat("PixelSnap", 1);
        tilemapMaterial.SetTexture("_MainTex", loadedTilemap);
    }

    static void cutTileSpriteAndAddToLibrary(Texture2D targetTex, string fileName, ref int lastWrittenTileLocation)
    {
        int numberOfTilesInXDirection = Mathf.FloorToInt(targetTex.width / tileSizeInPixels);
        int yMax = targetTex.height / tileSizeInPixels;
        for (int y = 0; (y+1) * tileSizeInPixels <= targetTex.height; y++)
        {
            for (int x = 0; (x+1) * tileSizeInPixels <= targetTex.width; x++)
            {
                int number = (yMax - (y+1)) * numberOfTilesInXDirection + x;
                Color[] col = targetTex.GetPixels(x * tileSizeInPixels, y * tileSizeInPixels, tileSizeInPixels, tileSizeInPixels, 0);
                int xPosToWrite = lastWrittenTileLocation % compiledTileMapsDesiredWidth;
                int yposToWrite = Mathf.FloorToInt((float)lastWrittenTileLocation/compiledTileMapsDesiredWidth);
                loadedTilemap.SetPixels(tileSizeInPixels * xPosToWrite, tileSizeInPixels * yposToWrite, tileSizeInPixels, tileSizeInPixels, col);
                string name = fileName + "_" + number.ToString("D4");
                float xSize = 1f / compiledTileMapsDesiredWidth;
                float ySize = 1f / compiledTileMapsDesiredHeight;
                const float offset = 0.0001f;
                Vector2[] UVArray = new Vector2[] {
                    new Vector2(xPosToWrite * xSize + offset, yposToWrite * ySize + offset), new Vector2((xPosToWrite + 1) * xSize - offset, yposToWrite * ySize + offset),
                    new Vector2(xPosToWrite * xSize + offset, (yposToWrite+1) * ySize - offset), new Vector2((xPosToWrite + 1) * xSize - offset, (yposToWrite+1) * ySize - offset)
                };

                translateTileNameToPositionOnTilemap.Add(name, UVArray);

                lastWrittenTileLocation++;
            }
        }
    }

    const string pokemonSpritesLocation = "Pokemon Sprites";
    public const int entitySpriteX = 40, entitySpriteY = 30;
    static void LoadSpriteLibrary()
    {
        FileInfo[] allFiles = Guildleader.FileAccess.GetAllFilesInDirectory(Guildleader.FileAccess.EntitySpritesLocation);

        foreach (FileInfo fi in allFiles)
        {
            if (fi.Extension != ".png")
            {
                continue;
            }
            Texture2D loadedSprite = new Texture2D(1, 1);
            byte[] loadedData = File.ReadAllBytes(fi.FullName);
            loadedSprite.LoadImage(loadedData);
            cutPokemonSpritesAndAddToLibrary(loadedSprite, fi.Name.Replace(".png", ""));
        }
    }
    static void cutPokemonSpritesAndAddToLibrary(Texture2D targetTex, string fileName)
    {
        int numberOfTilesInXDirection = Mathf.FloorToInt(targetTex.width / entitySpriteX);
        int yMax = targetTex.height / entitySpriteY;
        for (int y = 0; (y + 1) * entitySpriteY <= targetTex.height; y++)
        {
            for (int x = 0; (x + 1) * entitySpriteY <= targetTex.width; x++)
            {
                int number = (yMax - (y + 1)) * numberOfTilesInXDirection + x;
                Color[] col = targetTex.GetPixels(x * entitySpriteX, y * entitySpriteY, entitySpriteX, entitySpriteY, 0);

                string name = fileName + "_" + number.ToString("D4");
                Texture2D newPokemonTex = new Texture2D(entitySpriteX, entitySpriteY);
                newPokemonTex.filterMode = FilterMode.Point;
                newPokemonTex.SetPixels(col);
                pokemonSprites.Add(name, newPokemonTex);
                newPokemonTex.Apply();
            }
        }
    }
}

