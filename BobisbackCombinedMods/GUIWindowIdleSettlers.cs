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
    public class GUIWindowIdleSettlers : MonoBehaviour {

        //vars needed for window placement
        private static float buttonHeight = 32;
        private static float leftRightMargin = 15;
        private static float topBottomMargin = 7.5f;
        private static float inbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(60, 140, 300, 126);
        private static int windowId = 501;

        private GUIManager guiMgr = GUIManager.getInstance();
        private String guiName = "Idle Settlers";
        private List<APlayableEntity> idleSettlers = new List<APlayableEntity>();

        //we only wanna update The idle setters once ever second or so
        private static Timer updateTimer = new Timer(500);

        private static int currentIdleSettlerIndex = -1;
        private static APlayableEntity settlerToSelect = null;
        private static bool selectSettler = false;

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        void Start() {
            updateTimer.Elapsed += getIdleSettlers;
            updateTimer.Start();
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        void Update() {
            if (selectSettler && settlerToSelect != null) { //if we are suppose to select a settler and there is a settler to select
                MonoBehaviour selectedObject = UnitManager.getInstance().controllerObj.GetComponent<ControlPlayer>().selectedObject;
                bool openSettlerWindow = false;
                if (selectedObject != null && selectedObject.gameObject.tag == "ControllableUnit" && AManager<GUIManager>.getInstance().GetComponent<HumanSettlerWindow>().entity == selectedObject) {
                    openSettlerWindow = true;
                }

                AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().MoveToPosition(settlerToSelect.coordinate.world);
                AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(settlerToSelect.transform, openSettlerWindow);
                selectSettler = false;
            }

            if (Input.GetKeyDown(SettingsManager.hotKeys["previousIdleSettler"])) {
                idleSettlerTab(false);
            }

            if (Input.GetKeyDown(SettingsManager.hotKeys["nextIdleSettler"])) {
                idleSettlerTab(true);
            }

            if (Input.GetKeyDown(SettingsManager.hotKeys["toggleIdleSettlersHotKey"])) {
                if (SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers] == false) {
                    SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers] = true;
                    AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        void OnGUI() {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers]) {
                    windowRect = GUI.Window(windowId, windowRect, BuildIdleSettlerMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }
        }

        /// <summary>
        /// This function will move through idle settlers forward and backwords depending on the bool
        /// </summary>
        /// <param name="next">Go to the next idle settler or the previous idle settler.</param>
        private void idleSettlerTab(bool next) {
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
                    currentIdleSettlerIndex++; //go to next settler
                    if (currentIdleSettlerIndex >= idleSettlers.Count) { //do we need to go to the begining of hte list?
                        currentIdleSettlerIndex = 0;
                    }
                    while (!idleSettlers[currentIdleSettlerIndex].isAlive()) { //make sure it is not dead, if it is go to the next one.
                        currentIdleSettlerIndex++;
                        if (currentIdleSettlerIndex >= idleSettlers.Count) {
                            currentIdleSettlerIndex = 0;
                        }
                    }
                } else {
                    currentIdleSettlerIndex--; //go to the previous settler
                    if (currentIdleSettlerIndex < 0) { //if we are at the begining of the list go to the last
                        currentIdleSettlerIndex = idleSettlers.Count - 1;
                    }
                    while (!idleSettlers[currentIdleSettlerIndex].isAlive()) { //make sure we are not selecting dead settlers
                        currentIdleSettlerIndex--;
                        if (currentIdleSettlerIndex < 0) {
                            currentIdleSettlerIndex = idleSettlers.Count - 1;
                        }
                    }
                }
            }

            //move to the settler and select him
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().MoveToPosition(idleSettlers[currentIdleSettlerIndex].coordinate.world);
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(idleSettlers[currentIdleSettlerIndex].transform, openSettlerWindow);
        }

        /// <summary>
        /// This function is called every time the time goes off as we do not want to update a 1000 times a second.
        /// Once this function is called it will heck to make sure it should run and then go get all the idle settlers
        /// in the game. It does this by checking the getWhatImDoing() function on the settler.
        /// </summary>
        /// <param name="sender">This is not used but needed for the timer</param>
        /// <param name="e">This is not used but needed for the timer</param>
        private void getIdleSettlers(object sender, ElapsedEventArgs e) {
            if (guiMgr.inGame && !guiMgr.gameOver) { // make sure we are in the game
                if (SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers]) { //make sure hte mod is enabled
                    foreach (APlayableEntity settler in UnitManager.getInstance().playerUnits) { //get all settlers
                        if (settler.getWhatImDoing() != null) { //make sure there is something to get
                            //see if the settler is waiting or doing nothing, then exclude any prefessions we need to exclude
                            if ((settler.getWhatImDoing().Contains("Waiting") || settler.getWhatImDoing().Equals("")) && passProfessionCheck(settler)) {
                                if (!idleSettlers.Contains(settler)) { //if it is not in the list put it in there
                                    idleSettlers.Add(settler);
                                } //otherwise leave it in there
                            } else { // if it does not meet the above conditions we do not want it in the list
                                idleSettlers.Remove(settler);
                            }
                            if (SettingsManager.boolSettings[(int)Preferences.showNotifications]) {
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
        private bool passProfessionCheck(APlayableEntity settler) {
            bool checkPassed = true;
            AProfession profession = settler.getProfession();

            if (SettingsManager.boolSettings[(int)Preferences.excludeArcher]) {
                if (profession.getProfessionName().Equals("Archer")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.boolSettings[(int)Preferences.excludeInfantry]) {
                if (profession.getProfessionName().Equals("Infantry")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.boolSettings[(int)Preferences.excludeTrader]) {
                if (profession.getProfessionName().Equals("Trader")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.boolSettings[(int)Preferences.excludeHerder]) {
                if (profession.getProfessionName().Equals("Herder")) {
                    checkPassed = false;
                }
            }
            return checkPassed;
        }

        /// <summary>
        /// This function will build the idle settler window.
        /// </summary>
        /// <param name="windowID">The id of the window we are building.</param>
        void BuildIdleSettlerMenu(int windowID) {

            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, guiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, this.guiMgr.closeWindowButtonStyle)) {
                SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers] = false;
                return;
            }

            float buttonAboveHeight = topBottomMargin;

            //show options
            Rect checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(checkBoxRect, "Show Exclude Options", ref SettingsManager.boolSettings[(int)Preferences.showOptions]);

            //build the options menu if needed
            if (SettingsManager.boolSettings[(int)Preferences.showOptions]) {
                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight + (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Archer", ref SettingsManager.boolSettings[(int)Preferences.excludeArcher]);

                checkBoxRect = new Rect((leftRightMargin) + ((windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2)) + inbetweenMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Infantry", ref SettingsManager.boolSettings[(int)Preferences.excludeInfantry]);

                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight + (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Trader", ref SettingsManager.boolSettings[(int)Preferences.excludeTrader]);

                checkBoxRect = new Rect((leftRightMargin) + ((windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2)) + inbetweenMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Herder", ref SettingsManager.boolSettings[(int)Preferences.excludeHerder]);

                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - leftRightMargin * 2, buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Show Notifications", ref SettingsManager.boolSettings[(int)Preferences.showNotifications]);
            }

            bool isOdd = true;

            //display all the idle settings in a neat way
            if (idleSettlers.Count == 0) {
                Rect labelRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + topBottomMargin), windowRect.width - leftRightMargin * 2, buttonHeight);
                guiMgr.DrawTextCenteredBlack(labelRect, "None");
            } else {
                for (int i = 0; i < idleSettlers.Count; i++) {
                    isOdd = i % 2 != 0;
                    buildLabel(idleSettlers.ElementAt(i), isOdd ? buttonAboveHeight += (buttonHeight + inbetweenMargin) : buttonAboveHeight + (buttonHeight + inbetweenMargin), isOdd);
                }
            }

            windowRect.height = buttonAboveHeight + buttonHeight + inbetweenMargin + topBottomMargin + (isOdd ? 0 : buttonHeight + inbetweenMargin);
            GUI.DragWindow();
        }

        /// <summary>
        /// This will build a button to display a idle settler. 
        /// It also has the logic in it to handle moving to and selecting a settler.
        /// </summary>
        /// <param name="settler">The settler that needs to be displayed</param>
        /// <param name="buttonAboveHeight">The y coord needed relative to all the views above it</param>
        /// <param name="isOdd">Is this a odd button.</param>
        private void buildLabel(APlayableEntity settler, float buttonAboveHeight, bool isOdd) {

            Rect viewRect;
            if (isOdd) {
                viewRect = new Rect((leftRightMargin) + ((windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2)) + inbetweenMargin, buttonAboveHeight, (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
            } else {
                viewRect = new Rect(leftRightMargin, buttonAboveHeight, (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
            }

            if (guiMgr.DrawButton(viewRect, settler.unitName.Split(' ').FirstOrDefault())) {
                settlerToSelect = settler;
                selectSettler = true;
            }
        }
    }
}