using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using Guildleader.Entities;
using Guildleader.Entities.BasicEntities;

public static class EntitySpriteManager
{
    static Dictionary<int, PhysicalObject> Entities { get { return (WorldManager.currentWorld as ClientWorld).locallyLoadedEntities;  } }
    static Dictionary<int, EntityDrawingNode> DrawingNodes = new Dictionary<int, EntityDrawingNode>();
    public static void Update()
    {
        List<int> DrawingNodesToRemove = new List<int>(DrawingNodes.Keys);
        //first, create new nodes AND update current ones in one go
        foreach(var kvp in Entities)
        {
            DrawingNodesToRemove.Remove(kvp.Key);
            bool hasKey = DrawingNodes.TryGetValue(kvp.Key, out EntityDrawingNode node);
            if (hasKey)
            {
                node.UpdateFromEntity(kvp.Value);
            }
            else
            {
                node = new EntityDrawingNode(kvp.Value);
                DrawingNodes.Add(kvp.Key, node);
            }
            node.UpdateWorldPosition();
        }
        //and, finally, remove irrelevant ones
        foreach (int i in DrawingNodesToRemove)
        {
            DrawingNodes[i].Cleanup();
            DrawingNodes.Remove(i);
        }
    }
}

public class EntityDrawingNode
{
    public SpriteRenderer currentSpriteObjectForEntity;
    public Int3 positionLastDrawnAt, currentPosition;
    float positionTweenFraction; //from 1 to 0; 0 is its new positon, 1 is its old one
    string oldSpriteName;

    public EntityDrawingNode(PhysicalObject owner)
    {
        positionLastDrawnAt = currentPosition = owner.worldPositon;
        currentSpriteObjectForEntity = SpriteBatcher.TakeSpriteObject().GetComponent<SpriteRenderer>();
    }

    public void UpdateFromEntity(PhysicalObject ent)
    {
        currentPosition = positionLastDrawnAt = ent.worldPositon;
        if (oldSpriteName != ent.stringRetrievedFromServer)
        {
            oldSpriteName = ent.stringRetrievedFromServer;
            Texture2D tex = ImageLibrary.EntitySprites[ent.stringRetrievedFromServer];
            currentSpriteObjectForEntity.sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), Vector2.one * 0.5f);
        }
    }

    public void UpdateWorldPosition()
    {
        currentSpriteObjectForEntity.transform.position = new Vector3(currentPosition.x * SessionManager.unitScaling, currentPosition.y * SessionManager.unitScaling, currentPosition.z * SessionManager.unitScaling * SessionManager.zScaleStretch) + Vector3.one * 0.5f * SessionManager.unitScaling;
    }

    public void Cleanup()
    {
        SpriteBatcher.ReturnSpriteObject(currentSpriteObjectForEntity.gameObject);
    }
}
