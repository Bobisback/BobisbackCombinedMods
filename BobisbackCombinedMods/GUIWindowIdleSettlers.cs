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
        //private static float buttonStartHeight = 0f;
        private static float inbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(60, 140, 300, 126);

        private GUIManager guiMgr = GUIManager.getInstance();
        private String guiName = "Idle Settlers";
        private List<APlayableEntity> idleSettlers = new List<APlayableEntity>();

        //we only wanna update hte idle setters once ever second or so
        private static Timer updateTimer = new Timer(500);

        

        void Start() {
            updateTimer.Elapsed += getIdleSettlers;
            updateTimer.Start();
        }

        void OnGUI() {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.settings[(int)Preferences.toggleIdleSettlers]) {
                    windowRect = GUI.Window(501, windowRect, BuildIdleSettlerMenu, string.Empty, guiMgr.windowBoxStyle);
                    guiMgr.DrawWindow(windowRect, guiName, false);
                }
            }
        }

        public void getIdleSettlers(object sender, ElapsedEventArgs e) {
            if (guiMgr.inGame && !guiMgr.gameOver) { // make sure we are in the game
                if (SettingsManager.settings[(int)Preferences.toggleIdleSettlers]) {
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
                            if (SettingsManager.settings[(int)Preferences.showNotifications]) {
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

            if (SettingsManager.settings[(int)Preferences.excludeArcher]) {
                if (profession.getProfessionName().Equals("Archer")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.settings[(int)Preferences.excludeInfantry]) {
                if (profession.getProfessionName().Equals("Infantry")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.settings[(int)Preferences.excludeTrader]) {
                if (profession.getProfessionName().Equals("Trader")) {
                    checkPassed = false;
                }
            }
            if (SettingsManager.settings[(int)Preferences.excludeHerder]) {
                if (profession.getProfessionName().Equals("Herder")) {
                    checkPassed = false;
                }
            }
            return checkPassed;
        }

        void BuildIdleSettlerMenu(int windowID) {
            float buttonAboveHeight = topBottomMargin;

            Rect checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight + inbetweenMargin, windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(checkBoxRect, "Show Exclude Options", ref SettingsManager.settings[(int)Preferences.showOptions]);
            if (SettingsManager.settings[(int)Preferences.showOptions]) {
                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight + (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Archer", ref SettingsManager.settings[(int)Preferences.excludeArcher]);

                checkBoxRect = new Rect((leftRightMargin) + ((windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2)) + inbetweenMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Infantry", ref SettingsManager.settings[(int)Preferences.excludeInfantry]);

                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight + (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Trader", ref SettingsManager.settings[(int)Preferences.excludeTrader]);

                checkBoxRect = new Rect((leftRightMargin) + ((windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2)) + inbetweenMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), (windowRect.width - leftRightMargin * 2) / 2 - (inbetweenMargin / 2), buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Herder", ref SettingsManager.settings[(int)Preferences.excludeHerder]);

                checkBoxRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - leftRightMargin * 2, buttonHeight);
                guiMgr.DrawCheckBox(checkBoxRect, "Show Notifications", ref SettingsManager.settings[(int)Preferences.showNotifications]);
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
                selectSettler(settler);
            }
        }

        public void selectSettler(APlayableEntity settler) {
            //if (objectToSelect.GetComponent<APlayableEntity>() != null) {
            //if (settler.faction == this) {
            
                /*controlPlayer.selectedObject = settler;
                settler.Select();

                controlPlayer.entityHighlight.renderer.enabled = true;
                controlPlayer.entityDestination.renderer.enabled = true;
                AManager<GUIManager>.getInstance().GetComponent<HumanSettlerWindow>().entity = settler;*/
            //}
            /*} else {
                if (objectToSelect.GetComponent<BuildStructure>() != null) {
                    BuildStructure component2 = objectToSelect.GetComponent<BuildStructure>();
                    this.selectedObject = component2;
                    component2.Select();
                } else {
                    if (objectToSelect.GetComponent<MonoBehaviour>() != null && objectToSelect.GetComponent<MonoBehaviour>() is IGUIWindow) {
                        controlPlayer.selectedObject = objectToSelect.GetComponent<MonoBehaviour>();
                    }
                }
            }*/
            //AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(settler.transform, false);
            //AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(UnitManager.getInstance().playerUnits.Find(x => x.unitName == settler.unitName).transform, false);
            //AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().MoveToPosition(settler.coordinate.world);
        }

        void Update() { }
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