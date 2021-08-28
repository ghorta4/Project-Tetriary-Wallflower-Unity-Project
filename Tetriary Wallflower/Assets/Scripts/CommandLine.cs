using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Guildleader;
using System.Linq;

public static class CommandLine
{
    public static bool lineActive;
    static string currentTextEntry;

    static Texture2D blank;
    public static void Initialze()
    {
        blank = new Texture2D(1,1);
        blank.SetPixel(0,0, new Color(0,0,0,0.7f));
    }
    public static void DrawCommandLine()
    {
        if (lineActive)
        {
            displayNotifsTimeRemaining = 3;
        }
        GUI.color = Color.black;
        int boxHeight = notifications.Count * 30;
        SetFadeLevel();
        GUI.DrawTexture(new Rect(0, Screen.height - 70 - boxHeight, Screen.width, boxHeight), blank);

        for (int i = 0; i < notifications.Count; i ++)
        {
            var tuple = notifications[notifications.Count - 1 - i];
            GUI.color = tuple.Item2;
            SetFadeLevel();
            GUI.Label(new Rect(0,Screen.height - (i) * 30 - 100, Screen.width, boxHeight), tuple.Item1);
        }
        if (!lineActive)
        {
            return;
        }
        GUI.color = Color.grey;
        Rect linePosition = new Rect(0, Screen.height - 50, Screen.width, 50);
        GUI.DrawTexture(linePosition, blank);
        GUI.color = Color.white;
        currentTextEntry = GUI.TextField(linePosition, currentTextEntry);

        //draw message log below
        Rect logPosition = new Rect(0, Screen.height - 50, Screen.width, 50);
    }
    static void SetFadeLevel()
    {
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, Mathf.Min(displayNotifsTimeRemaining, 2) / 2);
    }

    public static List<Tuple<string, Color>> notifications = new List<Tuple<string, Color>> {
        new Tuple<string, Color>("Command line active.", new Color(0.9f, 0.2f, 0.5f))
    };

    public static void ExecuteCurrentLine()
    {
        string command = currentTextEntry.ToLower();
        string[] keywords = command.Split();
        string[] args = keywords.Skip(1).Take(keywords.Length - 1).ToArray();
        switch (keywords[0].ToLower())
        {
            case "position":
            case "pos":
            case "location":
                notifications.Add(new Tuple<string, Color>($"Current location- Chunk {SessionManager.world.TrackedEntity.GetChunkPosition()}, Tile position {SessionManager.world.TrackedEntity.worldPositon}.", Color.white));
                break;
            case "gettile":
                TileGetter(args);
                break;
            case "greet":
                notifications.Add(new Tuple<string, Color> ("Hello, world!", Color.white));
                break;
            default:
                notifications.Add(new Tuple<string, Color>($"Unrecognized command '{keywords[0]}'.", Color.yellow));
                break;
        }
        currentTextEntry = "";
        lineActive = false;
        displayNotifsTimeRemaining = 10;
        while (notifications.Count > 10)
        {
            notifications.RemoveAt(0);
        }
    }

    static float displayNotifsTimeRemaining = 10;
    public static void Update(float timestep)
    {
        displayNotifsTimeRemaining = Mathf.Max(0, displayNotifsTimeRemaining - timestep);

        if (Input.GetKeyDown("`"))
        {
            lineActive = !lineActive;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteCurrentLine();
        }
    }

    static void TileGetter(string[] arguments)
    {
        if (arguments.Length < 3)
        {
            notifications.Add(new Tuple<string, Color>("Not enough arguments passed to the tile getter!", Color.red));
            return;
        }
        int[] locations = new int[3];
        for (int i = 0; i < locations.Length; i++)
        {
            bool success = int.TryParse(arguments[i], out locations[i]);
            if (!success)
            {
                notifications.Add(new Tuple<string, Color>($"Failed to parse '{arguments[i]}' to int.", Color.red));
                return;
            }
        }
        Int3 tilePos = new Int3(locations[0], locations[1], locations[2]);
        Chunk c = SessionManager.world.GetChunk(WorldDataStorageModuleGeneric.GetChunkPositionBasedOnTilePosition(tilePos.x, tilePos.y, tilePos.z));
        if (c == null)
        {
            notifications.Add(new Tuple<string, Color>($"Failed to find chunk at {WorldDataStorageModuleGeneric.GetChunkPositionBasedOnTilePosition(tilePos.x, tilePos.y, tilePos.z)}.", Color.red));
            return;
        }
        notifications.Add(new Tuple<string, Color>($"Tile at location {tilePos} has the ID of {SessionManager.world.GetTileAtLocation(tilePos).tileID}.", Color.white));
    }
}
