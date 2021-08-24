using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Guildleader;
using System;

//Alot of these act as functions for the buttons associated with this screen. Without it, there isn't much operation.
namespace Assets.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {

        //In hopes to push this ontime without having to overthink this bullshit, just having two public variables for the two different canvases.
        public Canvas MainCanvas;
        public Canvas OptionsCanvas;

        Dictionary<string, UIElement> d_elements;

        // Start is called before the first frame update
        void Start()
        {
            //For starters, we need to be sure we have the directories, and since we go here upon startup, best to just do it here.
            //For now it appears it just pokes at all necessary directories to see if they're valid, but we might need more initialization code later
            //So I'm calling initialize.
            Guildleader.FileAccess.Initialize(); //Will poke each directory to see if they exist. We will focus on LocalFilesLocation.

            //We should have the canvases set, by no means should they be null. So we can just initialize what we need to for them
            //The start menu should be visible
            MainCanvas.enabled = true;

            //Options disabled, because we should not see that until prompted to.
            OptionsCanvas.enabled = false;

            //And now to set up options
            Guildleader.FileAccess.SetDefaultDirectory(Guildleader.FileAccess.LocalFilesLocation);

            d_elements = new Dictionary<string, UIElement>();

            foreach (UIElement elm in FindObjectsOfType<UIElement>())
            {
                d_elements.Add(elm.m_UIText, elm);
                Debug.Log(elm.m_UIText);
            }

        }

        public void OnOptionsClicked()
        {
            //Opposite of init code, enable options, disable main.
            MainCanvas.enabled = false;
            OptionsCanvas.enabled = true;
            OptionsCanvas.GetComponent<OptionsMenu>().PerformInit();
        }

        public void OnOptionsExit()
        {
            //So obvious and so awful
            MainCanvas.enabled = true;
            OptionsCanvas.enabled = false;
        }

        public void Quit()
        {
            Application.Quit();
        }


        //Added this but not doing much since I wanted simply to connect the button to the event.
        public void ConnectToIP()
        {
            //Fuck this, this code is already fucked as is and i have to redo it all later anyways.
            GameObject IP = GameObject.Find("IP Field");
            GameObject Port = GameObject.Find("Port Field");

            Debug.Log("IP: " + IP.GetComponent<InputField>().text);
            Debug.Log("Port: " + Port.GetComponent<InputField>().text);
            //I hate everything

        }

    }
}