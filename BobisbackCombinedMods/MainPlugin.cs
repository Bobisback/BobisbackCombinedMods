using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using Timber_and_Stone.API;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using UnityEngine;
using EventHandler = Timber_and_Stone.API.Event.EventHandler;
using System.Reflection;

namespace Plugin.Bobisback.CombinedMods {
    public class PluginMain : CSharpPlugin, IEventListener {

        public override void OnLoad() {
            GUIManager.getInstance().AddTextLine("Bobisback Combined Mods Loaded");
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowTripleSpeed));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowIdleSettlers));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowCheatMenu));
            GUIManager.getInstance().gameObject.AddComponent(typeof(GUIWindowModOptions));
            SettingsManager.loadSettings();
            GUIWindowModOptions.displayMessage("Combined Mod Loaded", "Press '" + SettingsManager.hotKeys["toggleOptionsMenuHotKey"] + "' at any time to access the options menu. Thanks for checking this mod out!");
        }

        public override void OnEnable() {
            GUIManager.getInstance().AddTextLine("Bobisback Combined Mods Enabled");
            EventManager.getInstance().Register(this);
        }

        //this is a deconstructor, it is called when the object is freed. We wanna save our settings just before we disappear
        ~PluginMain() {
            SettingsManager.saveSettings();
        }
    }
}
