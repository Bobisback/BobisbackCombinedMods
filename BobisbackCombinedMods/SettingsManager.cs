using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Timber_and_Stone.API;

namespace Plugin.Bobisback.CombinedMods {

    public enum Preferences : int {
        //GUIWindowTripleSpeed options
        toggleTripleSpeed,
        //GUIWindowIdleSettlers options
        toggleIdleSettlers, showOptions, excludeArcher, excludeInfantry, excludeTrader, excludeHerder, showNotifications,
        //GUIWindowModOptions options
        toggleOptionsMenu,
        //GUIWindowCheatMenu options
        toggleCheatMenu, hunger, fatigue, invincible,
        //Total number of options always needs to be last aka add more options above
        totalOptions
    };

    class SettingsManager {
        public static bool[] settings = new bool[(int)Preferences.totalOptions] { 
            true, //init GUIWindowTripleSpeed options
            true, false, false, false, false, false, false, //init GUIWindowIdleSettlers options
            true, //init GUIWindowModOptions options
            false, true, true, false };//init GUIWindowCheatMenu options 

        public static List<PluginInfo> pluginList = new List<PluginInfo>();

        public static void loadSettings() {
            String buffer;

            try {
                StreamReader sr = new StreamReader("./saves/BobisbackCombinedModsSettings.txt");
                buffer = sr.ReadToEnd();

                string[] preferenceNames = Enum.GetNames(typeof(Preferences));

                for (int i = 0; i < settings.Length && i < preferenceNames.Length; i++) {
                    extractBoolean(preferenceNames[i], buffer, ref settings[i]);
                }

                int numberOfPlugins = 0;
                while (buffer.Contains("PluginFileName" + numberOfPlugins)) {

                    handlePluginFile(numberOfPlugins, buffer);

                    numberOfPlugins++;
                }

                sr.Close();
                GUIManager.getInstance().AddTextLine("Settings Loaded");
            } catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
                GUIManager.getInstance().AddTextLine("Settings Failed To Load");
            }
        }

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

        public static void saveSettings() {
            try {
                StreamWriter sw = new StreamWriter("./saves/BobisbackCombinedModsSettings.txt");

                sw.WriteLine("Bobisback Combined Mods Settings File Version 1.0");
                sw.WriteLine("Order does not matter. Variable names must match, must have a space after the var name.");
                sw.WriteLine("Boolean's must be set to 'True' or 'False' otherwise they will be ignored.");

                string[] preferenceNames = Enum.GetNames(typeof(Preferences));

                for (int i = 0; i < settings.Length && i < preferenceNames.Length; i++) {
                    sw.WriteLine(preferenceNames[i] + " " + settings[i]);
                }

                
                pluginList.RemoveAll(x => x.removeFromListAtClose);

                for (int i = 0; i < pluginList.Count; i++) {
                    if (!pluginList[i].removeFromListAtClose) {
                        sw.WriteLine("PluginFileName" + i + " " + pluginList[i].fileName + " " + pluginList[i].shouldLoadPlugin);
                    }
                }

                sw.Close();
            } catch (Exception e) {
                //File.WriteAllText("saves\\log.txt", "Settings Failed To Save Exception: " + e.Message);
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        
    }
}
