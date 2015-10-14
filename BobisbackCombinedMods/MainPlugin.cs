using System.Reflection;
using Timber_and_Stone.API;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;

namespace Plugin.Bobisback.CombinedMods {
    public class PluginMain : CSharpPlugin, IEventListener {

        /// <summary>
        /// This function is called when the mod loads at startup
        /// </summary>
        public override void OnLoad() {
            //add all of our GUI's to the game
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowTripleSpeed));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowIdleSettlers));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowCheatMenu));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowModOptions));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowControlGroup));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowInvasionMenus));
            //GUIManager.getInstance().gameObject.AddComponent(typeof(GUIDisplaySettlerCount));
            SettingsManager.LoadSettings(); //Load the settings for the game
            GUIWindowModOptions.DisplayMessage("Combined Mod Loaded", "Press '" + SettingsManager.HotKeys["toggleOptionsMenuHotKey"] + "' at any time to access the options menu. Thanks for checking this mod out!");
            GUIManager.getInstance().AddTextLine("Bobisback Combined Mods v" + Assembly.GetExecutingAssembly().GetName().Version + " Loaded");
        }

        /// <summary>
        /// This function is also called on mod startup. It is called after OnLoad
        /// </summary>
        public override void OnEnable() {
            EventManager.getInstance().Register(this);
            GUIManager.getInstance().AddTextLine("Bobisback Combined Mods v" + Assembly.GetExecutingAssembly().GetName().Version + " Enabled");
        }

        [EventHandler(Priority.Normal)]
        public void OnInvasionNormal(EventInvasion evt)
        {
            if (SettingsManager.BoolSettings[(int)Preferences.NoInvasions])
            {
                evt.result = Result.Deny;
            }
        }

        [EventHandler(Priority.Monitor)]
        public void OnInvasionMonitor(EventInvasion evt)
        {
            if (evt.result == Result.Deny)
            {
                GUIManager.getInstance().AddTextLine("A " + evt.invasion.getName() + " invasion has been cancelled");
                return;
            }

            if (SettingsManager.BoolSettings[(int)Preferences.InvasionsInfo]) {
                GUIManager.getInstance().AddTextLine("A " + evt.invasion.getName() + " invasion of " + evt.invasion.getUnits().Count + " units has spawned.");
            }
        }

        /// <summary>
        /// Becuase OnDisable() is not called at the moment, I use this function to save any settings before my mod gets unloaded. 
        /// This fucntion is called a deconstructor, it is called when the class is being deallocated from memory.
        /// </summary>
        ~PluginMain() {
            SettingsManager.SaveSettings();
        }
    }
}
