using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods {

    /// <summary>
    /// This enum allows indexing into the bool array called boolSettings. 
    /// This enum is used to help keep track of all the settings in the mod.
    /// </summary>
    public enum Preferences {
        //GUIWindowInvasionDifficultyMenu
        ToggleInvasionDifficultyMenu, DifficultySettingsEnabled, NoWolfDifficultySetting, NoNecromancerDifficultySetting,
        NoGoblinDifficultySetting, NoSpiderDifficultySetting, NoUndeadDifficultySetting,
        //Door Hitpoints Menu
        ToggleDoorHitpointsMenu, DoorHpEnabled, ShowDoorInfo, ShowHealthBars,
        //Settler count window
        ToggleSettlerCount,
        //GUIWindowTripleSpeed options
        ToggleTripleSpeed,
        //GUIWindowIdleSettlers options
        ToggleIdleSettlers, ShowOptions, ExcludeArcher, ExcludeInfantry, ExcludeTrader, ExcludeHerder, ShowNotifications,
        //GUIWindowModOptions options
        ToggleOptionsMenu,
        //GUIWindowCheatMenu options
        ToggleCheatMenu, NoHunger, NoInvasions, Invincible, InvasionsInfo, DisableLOS, EternalLight, EternalNight, UnlimitedResources,
        //Control group options
        EnableControlGroups,
        //Total number of options always needs to be last aka add more options above
        TotalOptions,
    };

    /// <summary>
    /// This class handles everything related to settings in the project. This includes but is not limited to loading and saving settings, 
    /// keeping track of hot keys andcontrol groups, as well as any bool preferences in the mod.
    /// </summary>
    class SettingsManager {
        /// <summary>
        /// This is the array yhat holds the boolean settings in the mod
        /// </summary>
        public static bool[] BoolSettings = { 
            false, false, false, false, false, false, false, //init GUIWindowInvasionDifficultyMenu 
            false, false, false, true,//init door hitpoints menu
            false, //init settler count window
            true, //init GUIWindowTripleSpeed options
            true, false, false, false, false, false, false, //init GUIWindowIdleSettlers options
            true, //init GUIWindowModOptions options
            false, false, false, false, false, false, false, false, false, //init GUIWindowCheatMenu options 
            true //init control groups
        };

        /// <summary>
        /// This list holds any information for all 3rd party plugins that are loaded into the mod.
        /// </summary>
        public static List<PluginInfo> PluginList = new List<PluginInfo>();

        /// <summary>
        /// This keeps track of the hotkeys in the app. There is a key for the hotkey 
        /// combined with a Keycode which is the value for the key.
        /// </summary>
        public static Dictionary<string, KeyCode> HotKeys = new Dictionary<string, KeyCode>() {
            {"toggleTripleSpeedHotKey", KeyCode.N},
            {"toggleIdleSettlersHotKey", KeyCode.B},
            {"toggleOptionsMenuHotKey", KeyCode.V},
            {"toggleCheatMenuHotKey", KeyCode.M},
            {"ToggleDoorHitpointsMenuHotKey", KeyCode.Z},
            {"previousIdleSettler", KeyCode.Comma},
            {"nextIdleSettler", KeyCode.Period},
            {"previousGameSpeedHotkey", KeyCode.LeftArrow},
            {"nextGameSpeedHotkey", KeyCode.RightArrow},
            {"toggleInvasionDifficultyMenuHotKey", KeyCode.L}
        };

        /// <summary>
        /// This list keeps track of the settlers in the respective control groups.
        /// </summary>
        public static List<string> ControlGroupSettlers = new List<string>(11) {
            string.Empty, string.Empty, string.Empty, 
            string.Empty, string.Empty, string.Empty, 
            string.Empty, string.Empty, string.Empty, 
            string.Empty,  string.Empty
        };

        public static float CurrentFenceHp = GUIDoorHitPointsMenu.DefaultFenceHp;
        public static float CurrentTimberHp = GUIDoorHitPointsMenu.DefaultTimberHp;
        public static float CurrentBracedHp = GUIDoorHitPointsMenu.DefaultBracedHp;
        public static float CurrentStuddedHp = GUIDoorHitPointsMenu.DefaultStuddedHp;
        public static float CurrentDungeonHp = GUIDoorHitPointsMenu.DefaultDungeonHp;
        public static float CurrentCastleHp = GUIDoorHitPointsMenu.DefaultCastleHp;

        public static int DifficultyPrecentAsInt = 100;

        /// <summary>
        /// This will load all the settings for the mod form a hard coded settings file name.
        /// </summary>
        public static void LoadSettings() {
            String buffer;

            try {
                StreamReader sr = new StreamReader("./saves/BobisbackCombinedModsSettings.txt");
                buffer = sr.ReadToEnd();

                //get the bool settings arry from teh file
                string[] preferenceNames = Enum.GetNames(typeof(Preferences));
                for (int i = 0; i < BoolSettings.Length && i < preferenceNames.Length; i++) {
                    ExtractBoolean(preferenceNames[i], buffer, ref BoolSettings[i]);
                }

                //get the hotkeys fro mteh file
                for (int i = 0; i < HotKeys.Count; i++) {
                    var hotKey = HotKeys.ElementAt(i);
                    if (buffer.Contains(hotKey.Key)) {
                        ExtractHotKey(hotKey, buffer);
                    }
                }

                //get all the control group settler names from the file
                for (int i = 0; i < ControlGroupSettlers.Count; i++) {
                    if (buffer.Contains("<controlGroup=" + i + ">")) {
                        ExtractSettlerControlGroup("<controlGroup=" + i + ">", buffer, i);
                    }
                }

                //get any mod filenames that need to be loaded from the file
                int numberOfPlugins = 0;
                while (buffer.Contains("PluginFileName" + numberOfPlugins)) {
                    HandlePluginFile(numberOfPlugins, buffer);

                    numberOfPlugins++;
                }

                //get all the door hp values from the file
                ExtractDoorHpValues(buffer);

                ExtractInt(buffer, "DifficultyPrecentAsInt", ref DifficultyPrecentAsInt);

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
        public static void SaveSettings() {
            try {
                StreamWriter sw = new StreamWriter("./saves/BobisbackCombinedModsSettings.txt");

                //write the bool settings file
                sw.WriteLine("//Bobisback Combined Mods Settings File Version 1.0");
                sw.WriteLine("//Order does not matter. Variable names must match, must have a space after the var name.");
                sw.WriteLine("//Boolean's must be set to 'True' or 'False' otherwise they will be ignored.");
                string[] preferenceNames = Enum.GetNames(typeof(Preferences));
                for (int i = 0; i < BoolSettings.Length && i < preferenceNames.Length; i++) {
                    sw.WriteLine(preferenceNames[i] + " " + BoolSettings[i]);
                }

                //write the hotkeys to the file
                sw.WriteLine("//Below are all the hot keys. All keys must be assigned a string from UnityEngine.KeyCode (Google It)");
                foreach (var hotKey in HotKeys) {
                    sw.WriteLine(hotKey.Key + " " + hotKey.Value.ToString());
                }

                //write the control groups for hte files
                sw.WriteLine("//This are the current settlers assigned to control groups");
                for (int i = 0; i < ControlGroupSettlers.Count; i++) {
                    sw.WriteLine("<controlGroup=" + i + ">" + (string.IsNullOrEmpty(ControlGroupSettlers[i]) ? "" : ControlGroupSettlers[i]) + "</controlGroup>");
                }

                //save all the door hp values to the file
                sw.WriteLine("//This is the door hp values");
                sw.WriteLine("CurrentFenceHp " + CurrentFenceHp);
                sw.WriteLine("CurrentTimberHp " + CurrentTimberHp);
                sw.WriteLine("CurrentBracedHp " + CurrentBracedHp);
                sw.WriteLine("CurrentStuddedHp " + CurrentStuddedHp);
                sw.WriteLine("CurrentDungeonHp " + CurrentDungeonHp);
                sw.WriteLine("CurrentCastleHp " + CurrentCastleHp);
                sw.WriteLine("DifficultyPrecentAsInt " + DifficultyPrecentAsInt);

                //write any loaded plugins to the file
                sw.WriteLine("//Below are all all plugins being loaded. Plugins have the structure 'PluginFileName(n) fileName shouldLoadPlugin'");
                sw.WriteLine("//With n being the amount of plugins (has to start at 0), file name, then weather the plugin should be loaded at game start.");
                PluginList.RemoveAll(x => x.RemoveFromListAtClose);
                for (int i = 0; i < PluginList.Count; i++) {
                    if (!PluginList[i].RemoveFromListAtClose) {
                        sw.WriteLine("PluginFileName" + i + " " + PluginList[i].FileName + " " + PluginList[i].ShouldLoadPlugin);
                    }
                }

                sw.Close();
            } catch (Exception e) {
                File.WriteAllText("./saves/BobisbackLog.txt", "Settings Failed To Save Exception: " + e.Message);
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        private static void ExtractDoorHpValues(string buffer)
        {
            ExtractFloat(buffer, "CurrentFenceHp", ref CurrentFenceHp);
            ExtractFloat(buffer, "CurrentTimberHp", ref CurrentTimberHp);
            ExtractFloat(buffer, "CurrentBracedHp", ref CurrentBracedHp);
            ExtractFloat(buffer, "CurrentStuddedHp", ref CurrentStuddedHp);
            ExtractFloat(buffer, "CurrentDungeonHp", ref CurrentDungeonHp);
            ExtractFloat(buffer, "CurrentCastleHp", ref CurrentCastleHp);
        }

        private static void ExtractFloat(string buffer, string variableName, ref float variable)
        {
            if (buffer.Contains(variableName)) {
                var index = buffer.IndexOf(variableName, StringComparison.Ordinal);

                if (index != -1) {
                    var temp = buffer.Substring(index);
                    string keyString = temp.Split()[1];
                    try {
                        variable = float.Parse(keyString);
                    } catch (Exception e) {
                        Console.WriteLine("Exception: " + e.Message);
                        GUIManager.getInstance().AddTextLine("There was a error in loading the settings for " + variableName);
                    }
                }
            }
        }

        private static void ExtractInt(string buffer, string variableName, ref int variable) {
            if (buffer.Contains(variableName)) {
                var index = buffer.IndexOf(variableName, StringComparison.Ordinal);

                if (index != -1) {
                    var temp = buffer.Substring(index);
                    string keyString = temp.Split()[1];
                    try {
                        variable = int.Parse(keyString);
                    } catch (Exception e) {
                        Console.WriteLine("Exception: " + e.Message);
                        GUIManager.getInstance().AddTextLine("There was a error in loading the settings for " + variableName);
                    }
                }
            }
        }

        //This function will extract a settler name out of the buffer string
        private static void ExtractSettlerControlGroup(string stringToSearchFor, string buffer, int controlGroupIndex) {
            var start = buffer.IndexOf(stringToSearchFor, StringComparison.Ordinal) + stringToSearchFor.Length;
            var end = buffer.IndexOf("</controlGroup>", start, StringComparison.Ordinal);
            var settlerName = buffer.Substring(start, end - start);
            settlerName = string.IsNullOrEmpty(settlerName) ? string.Empty : settlerName;
            ControlGroupSettlers[controlGroupIndex] = settlerName;
        }

        //this function will extract a hot key out of the buffer
        private static void ExtractHotKey(KeyValuePair<string, KeyCode> hotKey, string buffer) {
            var index = buffer.IndexOf(hotKey.Key, StringComparison.Ordinal);

            if (index == -1) return;

            var temp = buffer.Substring(index);
            string keyString = temp.Split()[1];
            try {
                HotKeys[hotKey.Key] = (KeyCode)Enum.Parse(typeof(KeyCode), keyString); 
            } catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
                GUIManager.getInstance().AddTextLine("There was a error in loading the settings for hotkey name '" + hotKey.Key + "' reverting to default hotkey.");
            }
        }

        //this function will extract a plugin info out of the buffer.
        private static void HandlePluginFile(int numberOfPlugins, string buffer)
        {
            var index = buffer.IndexOf("PluginFileName" + numberOfPlugins, StringComparison.Ordinal);
            
            if (index == -1) return;

            var temp = buffer.Substring(index);
            string[] settings = temp.Split();
            try {
                PluginInfo pluginInfo = new PluginInfo(settings[1].Trim())
                {
                    ShouldLoadPlugin = Convert.ToBoolean(settings[2].Trim())
                };

                PluginList.Add(pluginInfo);
            } catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
                GUIManager.getInstance().AddTextLine("Failed to load settings for plugin '" + settings[1] + "' Plugin Info will be discarded.");
            }
        }

        //this will get a bool out of the buffer
        private static void ExtractBoolean(string stringToSearchFor, string buffer, ref bool boolToChange) {
            var index = buffer.IndexOf(stringToSearchFor, StringComparison.Ordinal);
            
            if (index == -1) return;

            var temp = buffer.Substring(index);
            temp = temp.Split()[1];

            if (temp == default(string)) return;

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