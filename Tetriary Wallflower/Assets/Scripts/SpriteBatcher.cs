using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteBatcher
{
    static Queue<GameObject> SpriteObjects = new Queue<GameObject>();

    public static GameObject TakeSpriteObject()
    {
        GameObject toGive = null;
        if (SpriteObjects.Count <= 0)
        {
            toGive = CreateSpriteobject();
        }else
        {
            toGive = SpriteObjects.Dequeue();
        }
        toGive.SetActive(true);

        return toGive;
    }

    public static void ReturnSpriteObject(GameObject toReturn)
    {
        toReturn.SetActive(false);
        SpriteObjects.Enqueue(toReturn);
    }

    static GameObject CreateSpriteobject()
    {
        GameObject holster = new GameObject("Entity sprite");
        holster.AddComponent<SpriteRenderer>();
        holster.SetActive(false);
        return holster;
    }
}
