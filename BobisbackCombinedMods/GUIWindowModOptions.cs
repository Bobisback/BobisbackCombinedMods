using System;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;

namespace Plugin.Bobisback.CombinedMods {

    /// <summary>
    /// This class handles all the logic and display of the options menu, this includes
    /// loading mods, truning mods on and off and unloading mods.
    /// </summary>
    public class GUIWindowModOptions : MonoBehaviour, IEventListener
    {

        //all vars needed for displaying the windows
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(150, 300, 240, 148);
        private static readonly int WindowId = 502;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "Active Mod GUI's";

        private bool displayGetDllName;
        private string tempName = "plugin2.dll";

        //vars for displaying messages error and regular.
        private static readonly Timer UpdateTimer = new Timer(5000);
        private static bool _showErrorDialog;
        private static string _errorMessageTitle = "An Error has Occurred";
        private static string _errorMessage = "";

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start() {
            UpdateTimer.Elapsed += UpdateDisplay;

            EventManager.getInstance().Register(this);
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update()
        {
            if (Input.GetKeyDown(SettingsManager.HotKeys["toggleOptionsMenuHotKey"])) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleOptionsMenu] == false) {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleOptionsMenu] = true;
                    WorldManager.getInstance().PlayerFaction.DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleOptionsMenu] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        public void OnGUI()
        {
            if (SettingsManager.BoolSettings[(int)Preferences.ToggleOptionsMenu]) {
                windowRect = GUI.Window(WindowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
            }

            if (_showErrorDialog) {
                DisplayErrorDialog();
                UpdateTimer.Start();
            }

            if (displayGetDllName) {
                GetDllName();
            }

            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }

        public static void DisplayErrorMessage(string error) {
            DisplayMessage("An Error has Occurred", error);
        }

        public static void DisplayMessage(string title, string error) {
            _errorMessageTitle = title;
            _errorMessage = error;
            _showErrorDialog = true;
        }

        private void UpdateDisplay(object sender, ElapsedEventArgs e) {
            _showErrorDialog = false;
            UpdateTimer.Stop();
        }

        private void DisplayErrorDialog() {
            Rect displayErrorRect = new Rect(Screen.width / 2 - 160, Screen.height - 130, 320, 120);
            guiMgr.DrawWindow(displayErrorRect, _errorMessageTitle, false);
            displayErrorRect.y += 50;
            displayErrorRect.height += 50;
            guiMgr.DrawTextCenteredWhite(displayErrorRect, _errorMessage);
        }

        private void BuildOptionsMenu(int id) {

            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, guiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                SettingsManager.BoolSettings[(int)Preferences.ToggleOptionsMenu] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Idle Settlers Menu", ref SettingsManager.BoolSettings[(int)Preferences.ToggleIdleSettlers]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Control Groups", ref SettingsManager.BoolSettings[(int)Preferences.EnableControlGroups]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Game Speed GUI", ref SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Cheat Menu", ref SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Door HP Menu", ref SettingsManager.BoolSettings[(int)Preferences.ToggleDoorHitpointsMenu]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Difficulty Menu", ref SettingsManager.BoolSettings[(int)Preferences.ToggleInvasionDifficultyMenu]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "New Trade Menu", ref SettingsManager.BoolSettings[(int)Preferences.ToggleNewTradeMenu]);

            //buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            //guiMgr.DrawCheckBox(buttonRect, "Settler Count Mod", ref SettingsManager.BoolSettings[(int)Preferences.ToggleSettlerCount]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextCenteredBlack(buttonRect, "3rd Party mods");

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Add 3rd Party dll")) {
                displayGetDllName = true;
            }

            for (int i = 0; i < SettingsManager.PluginList.Count; i++) {
                if (!SettingsManager.PluginList[i].RemoveFromListAtClose) {
                    buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2) - 20, ButtonHeight);
                    guiMgr.DrawCheckBox(buttonRect, SettingsManager.PluginList[i].FileName, ref SettingsManager.PluginList[i].ShouldLoadPlugin);

                    if (SettingsManager.PluginList[i].ShouldLoadPlugin) {
                        if (SettingsManager.PluginList[i].DisplayToggle) {
                            DisplayMessage("Plugin Loaded", "Plugin '" + SettingsManager.PluginList[i].FileName + "' Loaded.");
                        }
                        SettingsManager.PluginList[i].DisplayToggle = false;
                        BobisbackPluginManager.LoadPlugin(SettingsManager.PluginList[i]);
                    } else {
                        if (!SettingsManager.PluginList[i].DisplayToggle) {
                            DisplayMessage("Plugin Unloaded", "Plugin '" + SettingsManager.PluginList[i].FileName + "' unloaded.\nRestart Required.");
                        }
                        SettingsManager.PluginList[i].DisplayToggle = true;
                    }

                    buttonRect.x += buttonRect.width;
                    buttonRect.height = 20f;
                    buttonRect.width = 20f;
                    if (GUI.Button(buttonRect, string.Empty, guiMgr.closeWindowButtonStyle)) {
                        DisplayMessage("Remove Plugin", "The plugin named '" + SettingsManager.PluginList[i].FileName + "' was unloaded and removed. Restart is Required.");
                        SettingsManager.PluginList[i].RemoveFromListAtClose = true;
                    }
                }
            }

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void GetDllName() {
            Rect location = new Rect(Screen.width - 380, 300f, 320f, 120f);
            guiMgr.DrawWindow(location, "Add dll From Another Mod", false);
            if (location.Contains(Event.current.mousePosition)) {
                guiMgr.mouseInGUI = true;
            }
            GUI.Box(new Rect(location.xMin + 20f, location.yMin + 42f, location.width - 40f, 24f), string.Empty, guiMgr.boxStyle);
            tempName = guiMgr.DrawTextFieldCenteredWhite("dll Name", new Rect(location.xMin + 16f, location.yMin + 42f, location.width - 32f, 24f), tempName, 20);
            if (guiMgr.DrawButton(new Rect(location.xMin + 24f, location.yMin + 80f, 100f, 28f), "Confirm")) {
                if (IsValidFilename(tempName)) {
                    FileInfo info = new FileInfo("./saves/" + tempName);
                    if (info.Exists) {//{ ... };
                    //if (File.Exists("saves\\" + tempName)) {
                        int index = SettingsManager.PluginList.FindIndex(x => x.FileName == tempName);
                        if (index == -1) { //plugin is not in list make a new plugin
                            PluginInfo newPlugin = new PluginInfo(tempName);
                            SettingsManager.PluginList.Add(newPlugin);
                            DisplayMessage("Plugin Added", "Activation Needed\n(Check the box next to it)");
                        } else {
                            SettingsManager.PluginList[index].RemoveFromListAtClose = false;
                            SettingsManager.PluginList[index].ShouldLoadPlugin = true;
                        }
                    } else {
                        DisplayErrorMessage("File does not exist fileName: " + "saves\\" + tempName);
                    }
                } else {
                    DisplayErrorMessage("Invalid file name. Make sure there are no spaces");
                }
                displayGetDllName = false;
            }
            if (guiMgr.DrawButton(new Rect(location.xMax - 24f - 100f, location.yMin + 80f, 100f, 28f), "Cancel")) {
                displayGetDllName = false;
            }
        }

        private bool IsValidFilename(string testName) {
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]");
            if (containsABadCharacter.IsMatch(testName) || testName.Contains(' ')) {
                return false;
            }
            return true;
        }
    }
}
