using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Timber_and_Stone.API;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using EventHandler = Timber_and_Stone.API.Event.EventHandler;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Timers;

namespace Plugin.Bobisback.CombinedMods {

    /// <summary>
    /// This class handles all the logic and display of the options menu, this includes
    /// loading mods, truning mods on and off and unloading mods.
    /// </summary>
    class GUIWindowModOptions : MonoBehaviour {

        //all vars needed for displaying the windows
        private static float buttonHeight = 32;
        private static float leftRightMargin = 15;
        private static float topBottomMargin = 7.5f;
        private static float inbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(180, 300, 240, 148);
        private static int windowId = 502;

        private GUIManager guiMgr = GUIManager.getInstance();
        private String guiName = "Active Mod GUI's";

        private bool displayGetDllName = false;
        private string tempName = "plugin2.dll";

        //vars for displaying messages error and regular.
        private static Timer updateTimer = new Timer(5000);
        private static bool showErrorDialog = false;
        private static string errorMessageTitle = "An Error has Occurred";
        private static string errorMessage = "";

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        void Start() {
            updateTimer.Elapsed += updateDisplay;
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        void Update() {
            if (Input.GetKeyDown(SettingsManager.hotKeys["toggleOptionsMenuHotKey"])) {
                if (SettingsManager.boolSettings[(int)Preferences.toggleOptionsMenu] == false) {
                    SettingsManager.boolSettings[(int)Preferences.toggleOptionsMenu] = true;
                    AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.boolSettings[(int)Preferences.toggleOptionsMenu] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        void OnGUI() {
            if (SettingsManager.boolSettings[(int)Preferences.toggleOptionsMenu]) {
                windowRect = GUI.Window(windowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
            }

            if (showErrorDialog) {
                displayErrorDialog();
                updateTimer.Start();
            }

            if (displayGetDllName) {
                getDllName();
            }
        }

        public static void displayErrorMessage(string error) {
            displayMessage("An Error has Occurred", error);
        }

        public static void displayMessage(string title, string error) {
            errorMessageTitle = title;
            errorMessage = error;
            showErrorDialog = true;
        }

        private void updateDisplay(object sender, ElapsedEventArgs e) {
            showErrorDialog = false;
            updateTimer.Stop();
        }

        private void displayErrorDialog() {
            Rect displayErrorRect = new Rect(Screen.width / 2 - 160, Screen.height - 130, 320, 120);
            guiMgr.DrawWindow(displayErrorRect, errorMessageTitle, false);
            displayErrorRect.y += 50;
            displayErrorRect.height += 50;
            guiMgr.DrawTextCenteredWhite(displayErrorRect, errorMessage);
        }

        private void BuildOptionsMenu(int id) {

            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, guiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, this.guiMgr.closeWindowButtonStyle)) {
                SettingsManager.boolSettings[(int)Preferences.toggleOptionsMenu] = false;
                return;
            }

            float buttonAboveHeight = topBottomMargin;

            Rect buttonRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Idle Settlers Mod", ref SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers]);

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Control Groups", ref SettingsManager.boolSettings[(int)Preferences.enableControlGroups]);

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Game Speed Mod", ref SettingsManager.boolSettings[(int)Preferences.toggleTripleSpeed]);

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Cheat Menu Mod", ref SettingsManager.boolSettings[(int)Preferences.toggleCheatMenu]);

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawTextCenteredBlack(buttonRect, "3rd Party mods");

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            if (guiMgr.DrawButton(buttonRect, "Add 3rd Party dll")) {
                displayGetDllName = true;
            }

            for (int i = 0; i < SettingsManager.pluginList.Count; i++) {
                if (!SettingsManager.pluginList[i].removeFromListAtClose) {
                    buttonRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2) - 20, buttonHeight);
                    guiMgr.DrawCheckBox(buttonRect, SettingsManager.pluginList[i].fileName, ref SettingsManager.pluginList[i].shouldLoadPlugin);

                    if (SettingsManager.pluginList[i].shouldLoadPlugin) {
                        if (SettingsManager.pluginList[i].displayToggle) {
                            displayMessage("Plugin Loaded", "Plugin '" + SettingsManager.pluginList[i].fileName + "' Loaded.");
                        }
                        SettingsManager.pluginList[i].displayToggle = false;
                        BobisbackPluginManager.loadPlugin(SettingsManager.pluginList[i]);
                    } else {
                        if (!SettingsManager.pluginList[i].displayToggle) {
                            displayMessage("Plugin Unloaded", "Plugin '" + SettingsManager.pluginList[i].fileName + "' unloaded.\nRestart Required.");
                        }
                        SettingsManager.pluginList[i].displayToggle = true;
                    }

                    buttonRect.x += buttonRect.width;
                    buttonRect.height = 20f;
                    buttonRect.width = 20f;
                    if (GUI.Button(buttonRect, string.Empty, this.guiMgr.closeWindowButtonStyle)) {
                        displayMessage("Remove Plugin", "The plugin named '" + SettingsManager.pluginList[i].fileName + "' was unloaded and removed. Restart is Required.");
                        SettingsManager.pluginList[i].removeFromListAtClose = true;
                    }
                }
            }

            windowRect.height = buttonAboveHeight + buttonHeight + inbetweenMargin + topBottomMargin;

            GUI.DragWindow();
        }

        private void getDllName() {
            Rect location = new Rect((float)(Screen.width - 380), 300f, 320f, 120f);
            this.guiMgr.DrawWindow(location, "Add dll From Another Mod", false);
            if (location.Contains(Event.current.mousePosition)) {
                this.guiMgr.mouseInGUI = true;
            }
            GUI.Box(new Rect(location.xMin + 20f, location.yMin + 42f, location.width - 40f, 24f), string.Empty, this.guiMgr.boxStyle);
            tempName = this.guiMgr.DrawTextFieldCenteredWhite("dll Name", new Rect(location.xMin + 16f, location.yMin + 42f, location.width - 32f, 24f), tempName, 20);
            if (guiMgr.DrawButton(new Rect(location.xMin + 24f, location.yMin + 80f, 100f, 28f), "Confirm")) {
                if (IsValidFilename(tempName)) {
                    FileInfo info = new FileInfo("./saves/" + tempName);
                    if (info != null && info.Exists == true) {//{ ... };
                    //if (File.Exists("saves\\" + tempName)) {
                        int index = SettingsManager.pluginList.FindIndex(x => x.fileName == tempName);
                        if (index == -1) { //plugin is not in list make a new plugin
                            PluginInfo newPlugin = new PluginInfo(tempName);
                            SettingsManager.pluginList.Add(newPlugin);
                            displayMessage("Plugin Added", "Activation Needed\n(Check the box next to it)");
                        } else {
                            SettingsManager.pluginList[index].removeFromListAtClose = false;
                            SettingsManager.pluginList[index].shouldLoadPlugin = true;
                        }
                    } else {
                        displayErrorMessage("File does not exist fileName: " + "saves\\" + tempName);
                    }
                } else {
                    displayErrorMessage("Invalid file name. Make sure there are no spaces");
                }
                displayGetDllName = false;
            }
            if (guiMgr.DrawButton(new Rect(location.xMax - 24f - 100f, location.yMin + 80f, 100f, 28f), "Cancel")) {
                displayGetDllName = false;
            }
        }

        private bool IsValidFilename(string testName) {
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(System.IO.Path.GetInvalidPathChars())) + "]");
            if (containsABadCharacter.IsMatch(testName) || testName.Contains(' ')) {
                return false;
            };
            return true;
        }
    }
}
