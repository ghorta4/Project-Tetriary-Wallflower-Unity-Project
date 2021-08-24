using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI
{
    //Honestly I only use this for IP Field and Port Field at the moment, and those are obtained through getting game object.
    //It might be good to remove this later, or find a way to make it useful. I'm not in a position to do that atm tho.
    public class UIElement : MonoBehaviour
    {

        public Renderer[] m_renderers;
        public string m_UIText;

        // Start is called before the first frame update
        void Start()
        {
            m_renderers = GetComponentsInParent<Renderer>();
        }

        // Update is called once per frame
        void Update()
        { }

        public void SetCallback(UnityAction call)
        { }
    }
}