using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Guildleader;
using UnityEditor.UIElements;
using System.ComponentModel;

/**
 * Writing this as a bit of a generic class so we can just add an option, define the type of value the option is, and just run a call when it goes
 * through changes.
 */
namespace Assets.Scripts.UI
{
    //This code... why? Just why? 
    //I LITERALLY CANNOT FIND ANOTHER great WAY TO DO THIS WITH THE WAY CONVERSION WORKS!!!
    public class Option : MonoBehaviour
    {
        //Makes it unique, and allows users to identify what option it is
        //Could be volume, ui style, etc.
        public string Identifier;

        //The default value that is loaded in on start. Unity allows us to set values before start time, use these as the default value.
        public string DefaultValue;

        //The element associated with it. If you really think I'm just gonna make seperate classes for different elements, you're mistaken.
        public Selectable uiElement;

        //private bool awaitingInput;

        void Start()
        {
            if (DefaultValue.Length <= 0)
            {
                GetDefaultValue();
            }
        }

        void GetDefaultValue()
        {
            //For now, let's just do this.
            if (uiElement.GetType() == typeof(Slider))
            {
                //Unity hates casting confirmed.
                Slider tmp = ((Slider)uiElement);
                DefaultValue = tmp.value.ToString();
            }
            if (uiElement.GetType() == typeof(Dropdown))
            {
                //Dropdowns actually hold indexes as their value, so by default, we should get the default indexed value.
                Dropdown tmp = ((Dropdown)uiElement);
                DefaultValue = tmp.value.ToString();
            }
            if (uiElement.GetType() == typeof(Toggle))
            {
                Toggle tmp = ((Toggle)uiElement);
                DefaultValue = tmp.isOn.ToString();
            }
        }

        public void ChangeToggle()
        {
            if (uiElement.GetType() == typeof(Toggle))
            {
                Toggle tmp = ((Toggle)uiElement);
                tmp.isOn = !tmp.isOn;
            }
        }

        /* //This code can go unused until future notice.
                public void OnButtonClick()
                {
                    if (!awaitingInput && uiElement.GetType() == typeof(Button))
                    {
                        awaitingInput = true;
                        Button tmp = (Button)uiElement;
                        tmp.GetComponentInChildren<Text>().text = "...";
                    }
                }
        */

        //Truth be told, there is a way better way to obtain default values. But with everything needing to be 'easy to put it' it's overtly complex to determine what type of friendly ui element the nice user will use
        //I'm perfectly fine and sane.
        public void ResetValue()
        {
            //Yep yep you know the drill, play find the type with the ui element.
            //All this to be sure I don't have 5 different fun classes.
            if (uiElement.GetType() == typeof(Slider))
            {
                Slider tmp = ((Slider)uiElement);
                tmp.value = float.Parse(DefaultValue);
            }
            if (uiElement.GetType() == typeof(Dropdown))
            {
                Dropdown tmp = ((Dropdown)uiElement);
                tmp.value = Int32.Parse(DefaultValue);
            }
            if (uiElement.GetType() == typeof(Toggle))
            {
                Toggle tmp = ((Toggle)uiElement);
                tmp.isOn = bool.Parse(DefaultValue);
            }
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();

            //Read the string identifier first
            //bytes.AddRange(Guildleader.Convert.ToByte(Identifier));

            //And now depending on the element, read either a float, int, or short.
            if (uiElement.GetType() == typeof(Slider))
            {
                //Unity hates casting confirmed.
                Slider tmp = ((Slider)uiElement);
                //Cusion value maybe works?!?!?!
                bytes.AddRange(Guildleader.Convert.ToByte(tmp.value));
            }
            if (uiElement.GetType() == typeof(Dropdown))
            {
                //Dropdowns actually hold indexes as their value, so by default, we should get the default indexed value.
                Dropdown tmp = ((Dropdown)uiElement);
                bytes.AddRange(Guildleader.Convert.ToByte(tmp.value));
            }
            if (uiElement.GetType() == typeof(Toggle))
            {
                //Learned the hard way that shorts are a big no no. Will literally fuck up searching and make the float conversion fail.
                //Goodness i hate shorts.
                Toggle tmp = ((Toggle)uiElement);
                bytes.AddRange(Guildleader.Convert.ToByte(System.Convert.ToInt32(tmp.isOn)));
            }

            return bytes.ToArray();
        }

    }

}
