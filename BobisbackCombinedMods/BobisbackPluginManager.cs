using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Timber_and_Stone.API;

namespace Plugin.Bobisback.CombinedMods {

    /// <summary>
    /// This class Keeps track of all the plugin information needed to load the plugin. 
    /// It will also keep track of the plugin being loaded if it needs to as well as if the 
    /// plugin needs to be unloaded or display a message.
    /// </summary>
    public class PluginInfo {
        public string fileName = string.Empty;
        public bool shouldLoadPlugin = false;
        public bool displayToggle = true;
        public bool removeFromListAtClose = false;
        public bool isLoaded = false;
        public AppDomain appDomain = null;
        public IPlugin plugin = null;
        public Assembly assembly = null;

        public PluginInfo(string fileName) {
            this.fileName = fileName;
            shouldLoadPlugin = false;
            appDomain = null;
            plugin = null;
            assembly = null;
        }
    }

    /// <summary>
    /// This class handles all loading and unloading of the .dll files. Plugin info just stores the info
    /// and the manager actually does something with the plugin info.
    /// </summary>
    class BobisbackPluginManager {

        /// <summary>
        /// This function will unload the plugin. Currently it does not work as the only way to unload a plugin
        /// is to unload the app domain it is in, Currently I cannot create app domains so all I do to unload plugin
        /// is flag it to make sure it does not get loaded on next start.
        /// </summary>
        /// <param name="pluginInfo">The plugin to get unlaoded from the mod.</param>
        public static void unLoadPlugin(PluginInfo pluginInfo) {
            if (pluginInfo.plugin != null) {
                pluginInfo.plugin.OnDisable();
            }
            if (pluginInfo.appDomain != null) {
                AppDomain.Unload(pluginInfo.appDomain);
            }
            pluginInfo.removeFromListAtClose = true;
            pluginInfo.shouldLoadPlugin = false;
            pluginInfo.appDomain = null;
            pluginInfo.plugin = null;
            pluginInfo.assembly = null;
        }

        /// <summary>
        /// This function will load the plugin. It will take hte plugin info make sure the file exsits
        /// and then load the dll, find the class that needs to be loaded and then load that class.
        /// There is no checking in place to make sure the plugins do not conflict.
        /// </summary>
        /// <param name="pluginInfo">This is the plugin that will be loaded.</param>
        public static void loadPlugin(PluginInfo pluginInfo) {
            if (pluginInfo.isLoaded) {
                return;
            }

            pluginInfo.shouldLoadPlugin = true;
            FileInfo info = new FileInfo("./saves/" + pluginInfo.fileName);
            if (info != null && info.Exists == true) { //make sure the file is there
                try {
                    //loading new domains do not work for some reason
                    //pluginInfo.appDomain = AppDomain.CreateDomain("Testdomain");
                    //AssemblyName assemblyName = new AssemblyName();
                    //assemblyName.CodeBase = pluginInfo.fileName;
                    //pluginInfo.assembly = pluginInfo.appDomain.Load(pluginInfo.fileName);

                    pluginInfo.assembly = Assembly.LoadFile("./saves/" + pluginInfo.fileName); //load file
                    if (pluginInfo.assembly != null) {
                        Type[] types = pluginInfo.assembly.GetTypes(); //get all classes out of file
                        for (int i = 0; i < types.Length; i++) {
                            Type type = types[i];
                            if (typeof(IPlugin).IsAssignableFrom(type) && type.IsClass) { //find the one that is a plugin
                                pluginInfo.plugin = (IPlugin)Activator.CreateInstance(type); //create that class
                            }
                        }
                    } else {
                        GUIWindowModOptions.displayErrorMessage("Assemply is null on load of '" + pluginInfo.fileName + "'");
                        AManager<GUIManager>.getInstance().AddTextLine("Assemply is null on load of '" + pluginInfo.fileName + "'");
                    }
                } catch (Exception ex) {
                    GUIWindowModOptions.displayErrorMessage("Could not load assembly: '" + pluginInfo.fileName + "'");
                    AManager<GUIManager>.getInstance().AddTextLine("Could not load assembly: '" + pluginInfo.fileName + "'");
                    AManager<GUIManager>.getInstance().AddTextLine(" " + ex.Message);
                    pluginInfo.shouldLoadPlugin = false;
                    pluginInfo.isLoaded = false;
                }

                if (pluginInfo.plugin != null) { //make sure we created the class
                    try {
                        pluginInfo.plugin.OnLoad(); //load hte plugin
                    } catch (Exception ex) {
                        GUIWindowModOptions.displayErrorMessage("Assembly " + pluginInfo.assembly.GetName().Name + " crashed in OnLoad");
                        AManager<GUIManager>.getInstance().AddTextLine("Assembly " + pluginInfo.assembly.GetName().Name + " crashed in OnLoad with exception: " + ex.Message);
                        pluginInfo.shouldLoadPlugin = false;
                        pluginInfo.isLoaded = false;
                    }

                    try {
                        pluginInfo.plugin.OnEnable(); //enable the plugin
                    } catch (Exception ex) {
                        GUIWindowModOptions.displayErrorMessage("Assembly " + pluginInfo.assembly.GetName().Name + " crashed in OnEnable");
                        AManager<GUIManager>.getInstance().AddTextLine("Assembly " + pluginInfo.assembly.GetName().Name + " crashed in OnEnable with exception: " + ex.Message);
                        pluginInfo.shouldLoadPlugin = false;
                        pluginInfo.isLoaded = false;
                    }
                    pluginInfo.isLoaded = true;
                } else {
                    GUIWindowModOptions.displayErrorMessage("Plugin is null");
                    AManager<GUIManager>.getInstance().AddTextLine("Plugin is null");
                    pluginInfo.shouldLoadPlugin = false;
                    pluginInfo.isLoaded = false;
                }
            } else {
                GUIWindowModOptions.displayErrorMessage("File name does not exist: '" + pluginInfo.fileName + "'");
                AManager<GUIManager>.getInstance().AddTextLine("File name does not exist: '" + pluginInfo.fileName + "'");
                pluginInfo.shouldLoadPlugin = false;
                pluginInfo.isLoaded = false;
            }
        }
    }
}
