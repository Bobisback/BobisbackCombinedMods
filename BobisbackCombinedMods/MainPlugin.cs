using Timber_and_Stone.API;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;

namespace Plugin.Bobisback.CombinedMods {
    public class PluginMain : CSharpPlugin, IEventListener {

        /// <summary>
        /// This function is called when the mod loads at startup
        /// </summary>
        public override void OnLoad() {
            GUIManager.getInstance().AddTextLine("Bobisback Combined Mods Loaded");
            //add all of our GUI's to the game
            GUIManager.getInstance().gameObject.AddComponent(typeof(GuiWindowTripleSpeed));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GuiWindowIdleSettlers));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GuiWindowCheatMenu));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GuiWindowModOptions));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GuiWindowControlGroup));
            SettingsManager.LoadSettings(); //Load the settings for the game
            GuiWindowModOptions.DisplayMessage("Combined Mod Loaded", "Press '" + SettingsManager.HotKeys["toggleOptionsMenuHotKey"] + "' at any time to access the options menu. Thanks for checking this mod out!");
        }

        /// <summary>
        /// This function is also called on mod startup. It is called after OnLoad
        /// </summary>
        public override void OnEnable() {
            GUIManager.getInstance().AddTextLine("Bobisback Combined Mods Enabled");
            EventManager.getInstance().Register(this);
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
