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

        void Start() {
            updateTimer.Elapsed += getIdleSettlers;
            updateTimer.Start();
        }

        void Update() {
            if (selectSettler && settlerToSelect != null) {
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
                    currentIdleSettlerIndex++;
                    if (currentIdleSettlerIndex >= idleSettlers.Count) {
                        currentIdleSettlerIndex = 0;
                    }
                    while (!idleSettlers[currentIdleSettlerIndex].isAlive()) {
                        currentIdleSettlerIndex++;
                        if (currentIdleSettlerIndex >= idleSettlers.Count) {
                            currentIdleSettlerIndex = 0;
                        }
                    }
                } else {
                    currentIdleSettlerIndex--;
                    if (currentIdleSettlerIndex < 0) {
                        currentIdleSettlerIndex = idleSettlers.Count - 1;
                    }
                    while (!idleSettlers[currentIdleSettlerIndex].isAlive()) {
                        currentIdleSettlerIndex--;
                        if (currentIdleSettlerIndex < 0) {
                            currentIdleSettlerIndex = idleSettlers.Count - 1;
                        }
                    }
                }
            }

            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().MoveToPosition(idleSettlers[currentIdleSettlerIndex].coordinate.world);
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(idleSettlers[currentIdleSettlerIndex].transform, openSettlerWindow);
        }

        void OnGUI() {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers]) {
                    windowRect = GUI.Window(windowId, windowRect, BuildIdleSettlerMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }
        }

        public void getIdleSettlers(object sender, ElapsedEventArgs e) {
            if (guiMgr.inGame && !guiMgr.gameOver) { // make sure we are in the game
                if (SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers]) {
                    foreach (APlayableEntity settler in UnitManager.getInstance().playerUnits) {
                        //if (settler.taskStackContains(typeof(TaskWait)) && !(settler.taskStackContains(typeof(WorkGuardPosition)) || settler.taskStackContains(typeof(WorkPatrolRoute)))) {
                        if (settler.getWhatImDoing() != null) {
                            if ((settler.getWhatImDoing().Contains("Waiting") || settler.getWhatImDoing().Equals("")) && passProfessionCheck(settler)) {
                                if (!idleSettlers.Contains(settler)) {
                                    idleSettlers.Add(settler);
                                }
                            } else {
                                idleSettlers.Remove(settler);
                            }
                            if (SettingsManager.boolSettings[(int)Preferences.showNotifications]) {
                                GUIManager.getInstance().AddTextLine("Idle Settlers: " + idleSettlers.Count);
                            }
                            idleSettlers = idleSettlers.OrderBy(o => o.unitName).ToList();
                        }
                    }
                }

            }
        }

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

        void BuildIdleSettlerMenu(int windowID) {

            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, guiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, this.guiMgr.closeWindowButtonStyle)) {
                SettingsManager.boolSettings[(int)Preferences.toggleIdleSettlers] = false;
                return;
            }

            float buttonAboveHeight = topBottomMargin;

            Rect checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(checkBoxRect, "Show Exclude Options", ref SettingsManager.boolSettings[(int)Preferences.showOptions]);
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

        public void buildLabel(APlayableEntity settler, float buttonAboveHeight, bool isOdd) {

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

        /*public void selectSettler(APlayableEntity settler) {
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().MoveToPosition(settler.coordinate.world);
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(settler.transform, false);
        }*/
    }
}

/*
float buttonAboveHeight = topBottomMargin;

            Rect checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(checkBoxRect, "Show Exclude Options", ref showOptions);
            if (showOptions) {
                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight + (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Archer", ref excludeArcher);

                checkBoxRect = new Rect((leftRightMargin) + ((windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2)) + inbetweenMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Infantry", ref excludeInfantry);

                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight + (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Trader", ref excludeTrader);

                checkBoxRect = new Rect((leftRightMargin) + ((windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2)) + inbetweenMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Herder", ref excludeHerder);

                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - leftRightMargin * 2, buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Show Notifications", ref showNotifications);
            }

            bool isOdd = true;

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
*/