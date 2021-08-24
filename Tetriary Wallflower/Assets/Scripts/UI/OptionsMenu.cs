using Guildleader;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//Alot of these act as functions for the buttons associated with this screen. Without it, there isn't much operation.
namespace Assets.Scripts.UI
{
    public class OptionsMenu : MonoBehaviour
    {
        //Right now, redundant. But it might be useful for the server side later on, or for input binding.
        Dictionary<string, string> prefDefaults;

        public const string USER_PREF_FILE = "UserPrefs.bin";

        // Start is called before the first frame update
        void Start()
        {

            prefDefaults = new Dictionary<string, string>();

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PerformInit()
        {
            //We should always load in the defaults first, see if they're actually there
            //Chances are they will be, but in cases like now where they are not or if a dumb user deletes the file, we gotta step in.
            LoadDefaults();


            if (LoadPreferences() == false)
            {
                //And this is why we load defaults first, we now create a new set of preferences based on defaults.
                SavePreferences();
            }

        }


        public void SavePreferences()
        {
            //Data conversion may only be done in convert class, meaning byte tables are a necessity.

            List<byte> _data = new List<byte>();

            foreach (Option op in FindObjectsOfType<Option>())
            {

                _data.AddRange(op.GetBytes());

            }

            Guildleader.FileAccess.WriteBytesInsideCurrentDefaultDirectoryInSubfolder(_data.ToArray(), USER_PREF_FILE, "");
        }

        public bool LoadPreferences()
        {

            //First, see if it exists.
            if (!Guildleader.FileAccess.FileExists(USER_PREF_FILE))
            {
                //Last minute thing, i guess unity really does hate me and force me to put UnityEngine in front of ALL DEBUG FUNCTIONS!!!
                UnityEngine.Debug.Log("Failed to load user preferences: Preferences file not found.");
                UnityEngine.Debug.Log("Creating new Preference file.");
                return false;
            }

            //Load in the user's preferences
            byte[] _prefs = Guildleader.FileAccess.LoadFile(USER_PREF_FILE);

            //Error check, see if something went wrong when loading. In such a case, loadfile returns null.
            if (_prefs == null)
            {
                //So we ran into an error...
                UnityEngine.Debug.LogError("Failed to load user preferences: something went bad...");
                return false;
            }

            Option[] _options = FindObjectsOfType<Option>();

            int optionCounter = 0;
            
            //We loop through the options in order, then we get the number of bits we need for said option. Hoping that it's all in order.
            for (int i = 0; i < _options.Length; i++)
            {
                Option op = _options[i];

                if (op.uiElement.GetType() == typeof(Slider))
                {
                    Slider tmp = (Slider)op.uiElement;
                    tmp.value = Guildleader.Convert.ToFloat(_prefs, i * sizeof(float));

                }

                if (op.uiElement.GetType() == typeof(Dropdown))
                {
                    Dropdown tmp = (Dropdown)op.uiElement;
                    tmp.value = Guildleader.Convert.ToInt(_prefs, i * (sizeof(int)));

                }

                if (op.uiElement.GetType() == typeof(Toggle))
                {
                    Toggle tmp = (Toggle)op.uiElement;
                    tmp.isOn = System.Convert.ToBoolean((Guildleader.Convert.ToInt(_prefs, i * sizeof(int))));

                }

            }

            return true;
        }

        //Literally just sets up the default value on runtime.
        public void LoadDefaults()
        {
            foreach (Option op in FindObjectsOfType<Option>())
            {
                if (!prefDefaults.ContainsKey(op.Identifier))
                {
                    prefDefaults.Add(op.Identifier, op.DefaultValue);
                }
            }
        }

        //Literally just resets the value of every option available.
        public void ResetToDefault()
        {
            foreach (Option op in FindObjectsOfType<Option>())
            {
                op.ResetValue();
            }
        }

    }
}