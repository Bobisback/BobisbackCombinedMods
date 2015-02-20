using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Timber_and_Stone.API;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods {

    /// <summary>
    /// This enum allows indexing into the bool array called boolSettings. 
    /// This enum is used to help keep track of all the settings in the mod.
    /// </summary>
    public enum Preferences : int {
        //GUIWindowTripleSpeed options
        toggleTripleSpeed,
        //GUIWindowIdleSettlers options
        toggleIdleSettlers, showOptions, excludeArcher, excludeInfantry, excludeTrader, excludeHerder, showNotifications,
        //GUIWindowModOptions options
        toggleOptionsMenu,
        //GUIWindowCheatMenu options
        toggleCheatMenu, hunger, fatigue, invincible,
        //Control group options
        enableControlGroups,
        //Total number of options always needs to be last aka add more options above
        totalOptions
    };

    /// <summary>
    /// This class handles everything related to settings in the project. This includes but is not limited to loading and saving settings, 
    /// keeping track of hot keys andcontrol groups, as well as any bool preferences in the mod.
    /// </summary>
    class SettingsManager {
        /// <summary>
        /// This is the array yhat holds the boolean settings in the mod
        /// </summary>
        public static bool[] boolSettings = new bool[(int)Preferences.totalOptions] { 
            true, //init GUIWindowTripleSpeed options
            true, false, false, false, false, false, false, //init GUIWindowIdleSettlers options
            true, //init GUIWindowModOptions options
            false, true, true, false, //init GUIWindowCheatMenu options 
            true //init control groups
        };

        /// <summary>
        /// This list holds any information for all 3rd party plugins that are loaded into the mod.
        /// </summary>
        public static List<PluginInfo> pluginList = new List<PluginInfo>();

        /// <summary>
        /// This keeps track of the hotkeys in the app. There is a key for the hotkey 
        /// combined with a Keycode which is the value for the key.
        /// </summary>
        public static Dictionary<string, KeyCode> hotKeys = new Dictionary<string, KeyCode>() {
            {"toggleTripleSpeedHotKey", KeyCode.N},
            {"toggleIdleSettlersHotKey", KeyCode.B},
            {"toggleOptionsMenuHotKey", KeyCode.V},
            {"toggleCheatMenuHotKey", KeyCode.M},
            {"previousIdleSettler", KeyCode.Comma},
            {"nextIdleSettler", KeyCode.Period},
        };

        /// <summary>
        /// This list keeps track of the settlers in the respective control groups.
        /// </summary>
        public static List<string> controlGroupSettlers = new List<string>(11) {
            string.Empty, string.Empty, string.Empty, 
            string.Empty, string.Empty, string.Empty, 
            string.Empty, string.Empty, string.Empty, 
            string.Empty,  string.Empty
        };

        /// <summary>
        /// This will load all the settings for the mod form a hard coded settings file name.
        /// </summary>
        public static void loadSettings() {
            String buffer;

            try {
                StreamReader sr = new StreamReader("./saves/BobisbackCombinedModsSettings.txt");
                buffer = sr.ReadToEnd();

                //get the bool settings arry from teh file
                string[] preferenceNames = Enum.GetNames(typeof(Preferences));
                for (int i = 0; i < boolSettings.Length && i < preferenceNames.Length; i++) {
                    extractBoolean(preferenceNames[i], buffer, ref boolSettings[i]);
                }

                //get the hotkeys fro mteh file
                for (int i = 0; i < hotKeys.Count; i++) {
                    var hotKey = hotKeys.ElementAt(i);
                    if (buffer.Contains(hotKey.Key)) {
                        extractHotKey(hotKey, buffer);
                    }
                }

                //get all the control group settler names from the file
                for (int i = 0; i < controlGroupSettlers.Count; i++) {
                    if (buffer.Contains("<controlGroup=" + i + ">")) {
                        extractSettlerControlGroup("<controlGroup=" + i + ">", buffer, i);
                    }
                }

                //get any mod filenames that need to be loaded from the file
                int numberOfPlugins = 0;
                while (buffer.Contains("PluginFileName" + numberOfPlugins)) {
                    handlePluginFile(numberOfPlugins, buffer);

                    numberOfPlugins++;
                }

                sr.Close();
                GUIManager.getInstance().AddTextLine("Settings Loaded");
            } catch (Exception e) {
                File.WriteAllText("./saves/BobisbackLog.txt", "Settings Failed To Load: " + e.Message);
                Console.WriteLine("Exception: " + e.Message);
                GUIManager.getInstance().AddTextLine("Settings Failed To Load");
            }
        }

        /// <summary>
        /// This function will save all the settings for the mod into a hard coded filename
        /// </summary>
        public static void saveSettings() {
            try {
                StreamWriter sw = new StreamWriter("./saves/BobisbackCombinedModsSettings.txt");

                //write the bool settings file
                sw.WriteLine("//Bobisback Combined Mods Settings File Version 1.0");
                sw.WriteLine("//Order does not matter. Variable names must match, must have a space after the var name.");
                sw.WriteLine("//Boolean's must be set to 'True' or 'False' otherwise they will be ignored.");
                string[] preferenceNames = Enum.GetNames(typeof(Preferences));
                for (int i = 0; i < boolSettings.Length && i < preferenceNames.Length; i++) {
                    sw.WriteLine(preferenceNames[i] + " " + boolSettings[i]);
                }

                //write the hotkeys to the file
                sw.WriteLine("//Below are all the hot keys. All keys must be assigned a string from UnityEngine.KeyCode (Google It)");
                foreach (var hotKey in hotKeys) {
                    sw.WriteLine(hotKey.Key + " " + hotKey.Value.ToString());
                }

                //write the control groups for hte files
                sw.WriteLine("//This are the current settlers assigned to control groups");
                for (int i = 0; i < controlGroupSettlers.Count; i++) {
                    sw.WriteLine("<controlGroup=" + i + ">" + (string.IsNullOrEmpty(controlGroupSettlers[i]) ? "" : controlGroupSettlers[i]) + "</controlGroup>");
                }

                //write any loaded plugins to the file
                sw.WriteLine("//Below are all all plugins being loaded. Plugins have the structure 'PluginFileName(n) fileName shouldLoadPlugin'");
                sw.WriteLine("//With n being the amount of plugins (has to start at 0), file name, then weather the plugin should be loaded at game start.");
                pluginList.RemoveAll(x => x.removeFromListAtClose);
                for (int i = 0; i < pluginList.Count; i++) {
                    if (!pluginList[i].removeFromListAtClose) {
                        sw.WriteLine("PluginFileName" + i + " " + pluginList[i].fileName + " " + pluginList[i].shouldLoadPlugin);
                    }
                }

                sw.Close();
            } catch (Exception e) {
                File.WriteAllText("./saves/BobisbackLog.txt", "Settings Failed To Save Exception: " + e.Message);
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        //This function will extract a settler name out of the buffer string
        private static void extractSettlerControlGroup(string stringToSearchFor, string buffer, int controlGroupIndex) {
            int start = 0, end = 0;
            string settlerName = string.Empty;
            start = buffer.IndexOf(stringToSearchFor) + stringToSearchFor.Length;
            end = buffer.IndexOf("</controlGroup>", start);
            settlerName = buffer.Substring(start, end - start);
            settlerName = string.IsNullOrEmpty(settlerName) ? string.Empty : settlerName;
            controlGroupSettlers[controlGroupIndex] = settlerName;
        }

        //this function will extract a hot key out of the buffer
        private static void extractHotKey(KeyValuePair<string, KeyCode> hotKey, string buffer) {
            int index = -1;
            string temp = default(string);
            index = buffer.IndexOf(hotKey.Key);
            if (index != -1) {
                temp = buffer.Substring(index);
                string keyString = temp.Split()[1];
                try {
                    hotKeys[hotKey.Key] = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyString); 
                } catch (Exception e) {
                    Console.WriteLine("Exception: " + e.Message);
                    GUIManager.getInstance().AddTextLine("There was a error in loading the settings for hotkey name '" + hotKey.Key + "' reverting to default hotkey.");
                }
            }
        }

        //this function will extract a plugin info out of the buffer.
        private static void handlePluginFile(int numberOfPlugins, string buffer) {
            int index = -1;
            string temp = default(string);
            index = buffer.IndexOf("PluginFileName" + numberOfPlugins);
            if (index != -1) {
                temp = buffer.Substring(index);
                string[] settings = temp.Split();
                try {
                    PluginInfo pluginInfo = new PluginInfo(settings[1].Trim());
                    pluginInfo.shouldLoadPlugin = Convert.ToBoolean(settings[2].Trim());

                    SettingsManager.pluginList.Add(pluginInfo);
                } catch (Exception e) {
                    Console.WriteLine("Exception: " + e.Message);
                    GUIManager.getInstance().AddTextLine("Failed to load settings for plugin '" + settings[1] + "' Plugin Info will be discarded.");
                }
            }
        }

        //this will get a bool out of the buffer
        private static void extractBoolean(string stringToSearchFor, string buffer, ref bool boolToChange) {
            int index = -1;
            string temp = default(string);
            index = buffer.IndexOf(stringToSearchFor);
            if (index != -1) {
                temp = buffer.Substring(index);
                temp = temp.Split()[1];
                if (temp != default(string)) {
                    temp = temp.Trim();
                    try {
                        boolToChange = Convert.ToBoolean(temp);
                    } catch (Exception e) {
                        Console.WriteLine("Exception: " + e.Message);
                        GUIManager.getInstance().AddTextLine("Failed to Convert '" + temp + "' to boolean. make sure it is 'True' or 'False'");
                    }
                }
            }
        }
    }
}