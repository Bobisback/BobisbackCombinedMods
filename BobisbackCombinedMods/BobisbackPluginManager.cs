using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Timber_and_Stone.API;

namespace Plugin.Bobisback.CombinedMods {

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

    class BobisbackPluginManager {

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

        public static void loadPlugin(PluginInfo pluginInfo) {
            if (pluginInfo.isLoaded) {
                return;
            }

            pluginInfo.shouldLoadPlugin = true;
            FileInfo info = new FileInfo("./saves/" + pluginInfo.fileName);
            if (info != null && info.Exists == true) {
                try {
                    //pluginInfo.appDomain = AppDomain.CreateDomain("Testdomain");
                    //AssemblyName assemblyName = new AssemblyName();
                    //assemblyName.CodeBase = pluginInfo.fileName;
                    //pluginInfo.assembly = pluginInfo.appDomain.Load(pluginInfo.fileName);

                    pluginInfo.assembly = Assembly.LoadFile("./saves/" + pluginInfo.fileName);
                    if (pluginInfo.assembly != null) {
                        Type[] types = pluginInfo.assembly.GetTypes();
                        for (int i = 0; i < types.Length; i++) {
                            Type type = types[i];
                            if (typeof(IPlugin).IsAssignableFrom(type) && type.IsClass) {
                                pluginInfo.plugin = (IPlugin)Activator.CreateInstance(type);
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

                if (pluginInfo.plugin != null) {
                    try {
                        pluginInfo.plugin.OnLoad();
                    } catch (Exception ex) {
                        GUIWindowModOptions.displayErrorMessage("Assembly " + pluginInfo.assembly.GetName().Name + " crashed in OnLoad");
                        AManager<GUIManager>.getInstance().AddTextLine("Assembly " + pluginInfo.assembly.GetName().Name + " crashed in OnLoad with exception: " + ex.Message);
                        pluginInfo.shouldLoadPlugin = false;
                        pluginInfo.isLoaded = false;
                    }

                    try {
                        pluginInfo.plugin.OnEnable();
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
