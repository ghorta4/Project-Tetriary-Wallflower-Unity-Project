using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;
using Guildleader.Entities;
using Guildleader.Entities.BasicEntities;

public static class EntitySpriteManager
{
    static Dictionary<int, PhysicalObject> Entities { get { return (WorldManager.currentWorld as ClientWorld).locallyLoadedEntities;  } }
    public static Dictionary<int, EntityDrawingNode> DrawingNodes = new Dictionary<int, EntityDrawingNode>();
    public static void Update(float timestep)
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
            node.UpdateWorldPosition(timestep);
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
    public Int3 positionLastDrawnAt, targetPosition;
    float positionTweenFraction; //from 1 to 0; 0 is its new positon, 1 is its old one
    string oldSpriteName;

    public EntityDrawingNode(PhysicalObject owner)
    {
        positionLastDrawnAt = targetPosition = owner.worldPositon;
        currentSpriteObjectForEntity = SpriteBatcher.TakeSpriteObject().GetComponent<SpriteRenderer>();
    }

    public void UpdateFromEntity(PhysicalObject ent)
    {
        if (ent.worldPositon != targetPosition)
        {
            positionLastDrawnAt = targetPosition;
            targetPosition = ent.worldPositon;
            positionTweenFraction = 1;
        }
        if (oldSpriteName != ent.stringRetrievedFromServer)
        {
            oldSpriteName = ent.stringRetrievedFromServer;
            Texture2D tex = ImageLibrary.EntitySprites[ent.stringRetrievedFromServer];
            currentSpriteObjectForEntity.sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), Vector2.one * 0.5f);
        }
    }

    public void UpdateWorldPosition(float timeStep)
    {
        positionTweenFraction = Mathf.Max(0, positionTweenFraction - timeStep * 6);
        Vector3 start = new Vector3(positionLastDrawnAt.x, positionLastDrawnAt.y, positionLastDrawnAt.z);
        Vector3 end = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
        Vector3 offset = Vector3.one * 0.5f;
        currentSpriteObjectForEntity.transform.position = (start * positionTweenFraction + end * (1 - positionTweenFraction) + offset) * SessionManager.unitScaling;
    }

    public void Cleanup()
    {
        SpriteBatcher.ReturnSpriteObject(currentSpriteObjectForEntity.gameObject);
    }
}
