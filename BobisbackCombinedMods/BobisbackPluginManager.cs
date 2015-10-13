using System;
using System.IO;
using System.Reflection;
using Timber_and_Stone.API;

namespace Plugin.Bobisback.CombinedMods {

    /// <summary>
    /// This class Keeps track of all the plugin information needed to load the plugin. 
    /// It will also keep track of the plugin being loaded if it needs to as well as if the 
    /// plugin needs to be unloaded or display a message.
    /// </summary>
    public class PluginInfo {
        public string FileName;
        public bool ShouldLoadPlugin;
        public bool DisplayToggle = true;
        public bool RemoveFromListAtClose;
        public bool IsLoaded;
        public AppDomain AppDomain;
        public IPlugin Plugin;
        public Assembly Assembly;

        public PluginInfo(string fileName) {
            FileName = fileName;
            ShouldLoadPlugin = false;
            AppDomain = null;
            Plugin = null;
            Assembly = null;
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
        public static void UnLoadPlugin(PluginInfo pluginInfo) {
            if (pluginInfo.Plugin != null) {
                pluginInfo.Plugin.OnDisable();
            }
            if (pluginInfo.AppDomain != null) {
                AppDomain.Unload(pluginInfo.AppDomain);
            }
            pluginInfo.RemoveFromListAtClose = true;
            pluginInfo.ShouldLoadPlugin = false;
            pluginInfo.AppDomain = null;
            pluginInfo.Plugin = null;
            pluginInfo.Assembly = null;
        }

        /// <summary>
        /// This function will load the plugin. It will take hte plugin info make sure the file exsits
        /// and then load the dll, find the class that needs to be loaded and then load that class.
        /// There is no checking in place to make sure the plugins do not conflict.
        /// </summary>
        /// <param name="pluginInfo">This is the plugin that will be loaded.</param>
        public static void LoadPlugin(PluginInfo pluginInfo) {
            if (pluginInfo.IsLoaded) {
                return;
            }

            pluginInfo.ShouldLoadPlugin = true;
            FileInfo info = new FileInfo("./saves/" + pluginInfo.FileName);
            if (info.Exists) { //make sure the file is there
                try {
                    //loading new domains do not work for some reason
                    //pluginInfo.appDomain = AppDomain.CreateDomain("Testdomain");
                    //AssemblyName assemblyName = new AssemblyName();
                    //assemblyName.CodeBase = pluginInfo.fileName;
                    //pluginInfo.assembly = pluginInfo.appDomain.Load(pluginInfo.fileName);

                    pluginInfo.Assembly = Assembly.LoadFile("./saves/" + pluginInfo.FileName); //load file
                    if (pluginInfo.Assembly != null) {
                        Type[] types = pluginInfo.Assembly.GetTypes(); //get all classes out of file
                        for (int i = 0; i < types.Length; i++) {
                            Type type = types[i];
                            if (typeof(IPlugin).IsAssignableFrom(type) && type.IsClass) { //find the one that is a plugin
                                pluginInfo.Plugin = (IPlugin)Activator.CreateInstance(type); //create that class
                            }
                        }
                    } else {
                        GUIWindowModOptions.DisplayErrorMessage("Assemply is null on load of '" + pluginInfo.FileName + "'");
                        AManager<GUIManager>.getInstance().AddTextLine("Assemply is null on load of '" + pluginInfo.FileName + "'");
                    }
                } catch (Exception ex) {
                    GUIWindowModOptions.DisplayErrorMessage("Could not load assembly: '" + pluginInfo.FileName + "'");
                    AManager<GUIManager>.getInstance().AddTextLine("Could not load assembly: '" + pluginInfo.FileName + "'");
                    AManager<GUIManager>.getInstance().AddTextLine(" " + ex.Message);
                    pluginInfo.ShouldLoadPlugin = false;
                    pluginInfo.IsLoaded = false;
                }

                if (pluginInfo.Plugin != null) { //make sure we created the class
                    try {
                        pluginInfo.Plugin.OnLoad(); //load hte plugin
                    } catch (Exception ex) {
                        GUIWindowModOptions.DisplayErrorMessage("Assembly " + pluginInfo.Assembly.GetName().Name + " crashed in OnLoad");
                        AManager<GUIManager>.getInstance().AddTextLine("Assembly " + pluginInfo.Assembly.GetName().Name + " crashed in OnLoad with exception: " + ex.Message);
                        pluginInfo.ShouldLoadPlugin = false;
                        pluginInfo.IsLoaded = false;
                    }

                    try {
                        pluginInfo.Plugin.OnEnable(); //enable the plugin
                    } catch (Exception ex) {
                        GUIWindowModOptions.DisplayErrorMessage("Assembly " + pluginInfo.Assembly.GetName().Name + " crashed in OnEnable");
                        AManager<GUIManager>.getInstance().AddTextLine("Assembly " + pluginInfo.Assembly.GetName().Name + " crashed in OnEnable with exception: " + ex.Message);
                        pluginInfo.ShouldLoadPlugin = false;
                        pluginInfo.IsLoaded = false;
                    }
                    pluginInfo.IsLoaded = true;
                } else {
                    GUIWindowModOptions.DisplayErrorMessage("Plugin is null");
                    AManager<GUIManager>.getInstance().AddTextLine("Plugin is null");
                    pluginInfo.ShouldLoadPlugin = false;
                    pluginInfo.IsLoaded = false;
                }
            } else {
                GUIWindowModOptions.DisplayErrorMessage("File name does not exist: '" + pluginInfo.FileName + "'");
                AManager<GUIManager>.getInstance().AddTextLine("File name does not exist: '" + pluginInfo.FileName + "'");
                pluginInfo.ShouldLoadPlugin = false;
                pluginInfo.IsLoaded = false;
            }
        }
    }
}
