using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guildleader;

public static class EffectAreaGenerator //stores information such as raycasts and other particle effects to help generate smooth effects in a way that can easily be shared over the network.
{
    static List<EffectNode> allNodes = new List<EffectNode>();

}

public abstract class EffectNode //container for things such as bounding boxes, raycasts, and other geometry based regions for the effect area generator
{

}