using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Timber_and_Stone;
using Timber_and_Stone.API;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using Timber_and_Stone.Tasks;
using EventHandler = Timber_and_Stone.API.Event.EventHandler;
using System.Reflection;
using System.Timers;
using System.IO;

namespace Plugin.Bobisback.CombinedMods {
    public class GuiWindowIdleSettlers : MonoBehaviour {

        //vars needed for window placement
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(60, 140, 300, 126);
        private static readonly int WindowId = 501;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "Idle Settlers";
        private List<APlayableEntity> idleSettlers = new List<APlayableEntity>();

        //we only wanna update The idle setters once ever second or so
        private static readonly Timer UpdateTimer = new Timer(500);

        private static int _currentIdleSettlerIndex = -1;
        private static APlayableEntity _settlerToSelect = null;
        private static bool _selectSettler = false;

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        void Start() {
            UpdateTimer.Elapsed += GetIdleSettlers;
            UpdateTimer.Start();
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        void Update() {
            if (_selectSettler && _settlerToSelect != null) { //if we are suppose to select a settler and there is a settler to select
                MonoBehaviour selectedObject = UnitManager.getInstance().controllerObj.GetComponent<ControlPlayer>().selectedObject;
                bool openSettlerWindow = false;
                if (selectedObject != null && selectedObject.gameObject.tag == "ControllableUnit" && AManager<GUIManager>.getInstance().GetComponent<HumanSettlerWindow>().entity == selectedObject) {
                    openSettlerWindow = true;
                }

                AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().MoveToPosition(_settlerToSelect.coordinate.world);
                AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(_settlerToSelect.transform, openSettlerWindow);
                _selectSettler = false;
            }

            if (Input.GetKeyDown(SettingsManager.HotKeys["previousIdleSettler"])) {
                IdleSettlerTab(false);
            }

            if (Input.GetKeyDown(SettingsManager.HotKeys["nextIdleSettler"])) {
                IdleSettlerTab(true);
            }

            if (Input.GetKeyDown(SettingsManager.HotKeys["toggleIdleSettlersHotKey"])) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleIdleSettlers] == false) {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleIdleSettlers] = true;
                    AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleIdleSettlers] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        void OnGui() {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleIdleSettlers]) {
                    windowRect = GUI.Window(WindowId, windowRect, BuildIdleSettlerMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }
        }

        /// <summary>
        /// This function will move through idle settlers forward and backwords depending on the bool
        /// </summary>
        /// <param name="next">Go to the next idle settler or the previous idle settler.</param>
        private void IdleSettlerTab(bool next) {
            if (idleSettlers.Count == 0) {
                return;
            }

            ControlPlayer controlPlayer = AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>();
            MonoBehaviour selectedObject = UnitManager.getInstance().controllerObj.GetComponent<ControlPlayer>().selectedObject;
            bool openSettlerWindow = false;
            if (selectedObject != null && selectedObject.gameObject.tag == "ControllableUnit" && AManager<GUIManager>.getInstance().GetComponent<HumanSettlerWindow>().entity == selectedObject) {
                openSettlerWindow = true;
            }
            if (!AManager<GUIManager>.getInstance().gameOver) {
                if (next) {
                    _currentIdleSettlerIndex++; //go to next settler
                    if (_currentIdleSettlerIndex >= idleSettlers.Count) { //do we need to go to the begining of hte list?
                        _currentIdleSettlerIndex = 0;
                    }
                    while (!idleSettlers[_currentIdleSettlerIndex].isAlive()) { //make sure it is not dead, if it is go to the next one.
                        _currentIdleSettlerIndex++;
                        if (_currentIdleSettlerIndex >= idleSettlers.Count) {
                            _currentIdleSettlerIndex = 0;
                        }
                    }
                } else {
                    _currentIdleSettlerIndex--; //go to the previous settler
                    if (_currentIdleSettlerIndex < 0) { //if we are at the begining of the list go to the last
                        _currentIdleSettlerIndex = idleSettlers.Count - 1;
                    }
                    while (!idleSettlers[_currentIdleSettlerIndex].isAlive()) { //make sure we are not selecting dead settlers
                        _currentIdleSettlerIndex--;
                        if (_currentIdleSettlerIndex < 0) {
                            _currentIdleSettlerIndex = idleSettlers.Count - 1;
                        }
                    }
                }
            }

            //move to the settler and select him
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().MoveToPosition(idleSettlers[_currentIdleSettlerIndex].coordinate.world);
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(idleSettlers[_currentIdleSettlerIndex].transform, openSettlerWindow);
        }

        /// <summary>
        /// This function is called every time the time goes off as we do not want to update a 1000 times a second.
        /// Once this function is called it will heck to make sure it should run and then go get all the idle settlers
        /// in the game. It does this by checking the getWhatImDoing() function on the settler.
        /// </summary>
        /// <param name="sender">This is not used but needed for the timer</param>
        /// <param name="e">This is not used but needed for the timer</param>
        private void GetIdleSettlers(object sender, ElapsedEventArgs e) {
            if (guiMgr.inGame && !guiMgr.gameOver) { // make sure we are in the game
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleIdleSettlers]) { //make sure hte mod is enabled
                    foreach (APlayableEntity settler in UnitManager.getInstance().playerUnits) { //get all settlers
                        if (settler.getWhatImDoing() != null) { //make sure there is something to get
                            //see if the settler is waiting or doing nothing, then exclude any prefessions we need to exclude
                            if ((settler.getWhatImDoing().Contains("Waiting") || settler.getWhatImDoing().Equals("")) && PassProfessionCheck(settler)) {
                                if (!idleSettlers.Contains(settler)) { //if it is not in the list put it in there
                                    idleSettlers.Add(settler);
                                } //otherwise leave it in there
                            } else { // if it does not meet the above conditions we do not want it in the list
                                idleSettlers.Remove(settler);
                            }
                            if (SettingsManager.BoolSettings[(int)Preferences.ShowNotifications]) {
                                GUIManager.getInstance().AddTextLine("Idle Settlers: " + idleSettlers.Count);
                            }
                            //sort the list based on unit name
                            idleSettlers = idleSettlers.OrderBy(o => o.unitName).ToList();
                        }
                    }
                }

            }
        }

        /// <summary>
        /// This will check are settings and decide to include or exculde certain settlers based on those settings.
        /// </summary>
        /// <param name="settler">The settler to check if we want to include or exclude them</param>
        /// <returns>Should the settler be excluded or included.</returns>
        private bool PassProfessionCheck(APlayableEntity settler) {
            bool checkPassed = true;
            AProfession profession = settler.getProfession();

            if (SettingsManager.BoolSettings[(int)Preferences.ExcludeArcher]) {
                if (profession.getProfessionName().Equals("Archer")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.BoolSettings[(int)Preferences.ExcludeInfantry]) {
                if (profession.getProfessionName().Equals("Infantry")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.BoolSettings[(int)Preferences.ExcludeTrader]) {
                if (profession.getProfessionName().Equals("Trader")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.BoolSettings[(int)Preferences.ExcludeHerder]) {
                if (profession.getProfessionName().Equals("Herder")) {
                    checkPassed = false;
                }
            }
            return checkPassed;
        }

        /// <summary>
        /// This function will build the idle settler window.
        /// </summary>
        /// <param name="windowId">The id of the window we are building.</param>
        void BuildIdleSettlerMenu(int windowId) {

            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, guiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, this.guiMgr.closeWindowButtonStyle)) {
                SettingsManager.BoolSettings[(int)Preferences.ToggleIdleSettlers] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            //show options
            Rect checkBoxRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight + InbetweenMargin, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(checkBoxRect, "Show Exclude Options", ref SettingsManager.BoolSettings[(int)Preferences.ShowOptions]);

            //build the options menu if needed
            if (SettingsManager.BoolSettings[(int)Preferences.ShowOptions]) {
                checkBoxRect = new Rect(LeftRightMargin, buttonAboveHeight + (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Archer", ref SettingsManager.BoolSettings[(int)Preferences.ExcludeArcher]);

                checkBoxRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Infantry", ref SettingsManager.BoolSettings[(int)Preferences.ExcludeInfantry]);

                checkBoxRect = new Rect(LeftRightMargin, buttonAboveHeight + (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Trader", ref SettingsManager.BoolSettings[(int)Preferences.ExcludeTrader]);

                checkBoxRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Herder", ref SettingsManager.BoolSettings[(int)Preferences.ExcludeHerder]);

                checkBoxRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - LeftRightMargin * 2, ButtonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Show Notifications", ref SettingsManager.BoolSettings[(int)Preferences.ShowNotifications]);
            }

            bool isOdd = true;

            //display all the idle settings in a neat way
            if (idleSettlers.Count == 0) {
                Rect labelRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + TopBottomMargin), windowRect.width - LeftRightMargin * 2, ButtonHeight);
                guiMgr.DrawTextCenteredBlack(labelRect, "None");
            } else {
                for (int i = 0; i < idleSettlers.Count; i++) {
                    isOdd = i % 2 != 0;
                    BuildLabel(idleSettlers.ElementAt(i), isOdd ? buttonAboveHeight += (ButtonHeight + InbetweenMargin) : buttonAboveHeight + (ButtonHeight + InbetweenMargin), isOdd);
                }
            }

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin + (isOdd ? 0 : ButtonHeight + InbetweenMargin);
            GUI.DragWindow();
        }

        /// <summary>
        /// This will build a button to display a idle settler. 
        /// It also has the logic in it to handle moving to and selecting a settler.
        /// </summary>
        /// <param name="settler">The settler that needs to be displayed</param>
        /// <param name="buttonAboveHeight">The y coord needed relative to all the views above it</param>
        /// <param name="isOdd">Is this a odd button.</param>
        private void BuildLabel(APlayableEntity settler, float buttonAboveHeight, bool isOdd) {

            Rect viewRect;
            if (isOdd) {
                viewRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight, (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            } else {
                viewRect = new Rect(LeftRightMargin, buttonAboveHeight, (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            }

            if (guiMgr.DrawButton(viewRect, settler.unitName.Split(' ').FirstOrDefault())) {
                _settlerToSelect = settler;
                _selectSettler = true;
            }
        }
    }
}