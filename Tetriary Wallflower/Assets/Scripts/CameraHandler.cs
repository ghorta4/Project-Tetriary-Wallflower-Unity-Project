using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader.Entities.BasicEntities;
using Guildleader;

public class CameraHandler : MonoBehaviour
{
    void Start()
    {
        transform.rotation = Quaternion.Euler(0,180,0);
    }

    // Update is called once per frame
    void Update()
    {
        int playerID = SessionManager.world.TrackedEntityID;
        bool success = EntitySpriteManager.DrawingNodes.TryGetValue(playerID, out EntityDrawingNode target);
        if (!success)
        {
            return;
        }
        Vector3 drawPosition =target.currentSpriteObjectForEntity.transform.position;
        transform.position = drawPosition + Vector3.forward * 10;
    }
}
