using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Timber_and_Stone.API;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using EventHandler = Timber_and_Stone.API.Event.EventHandler;
using System.Timers;

namespace Plugin.Bobisback.CombinedMods {

    class GUIWindowCheatMenu : MonoBehaviour {
        private static float buttonHeight = 32;
        private static float leftRightMargin = 15;
        private static float topBottomMargin = 7.5f;
        private static float inbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(370, 200, 240, 237);

        private GUIManager guiMgr = GUIManager.getInstance();
        private String guiName = "Cheat Menu";

        private static Timer updateTimer = new Timer(500);

        void Start() {
            updateTimer.Elapsed += updateGameVariables;
            updateTimer.Start();
        }

        void OnGUI() {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.settings[(int)Preferences.toggleCheatMenu]) {
                    windowRect = GUI.Window(503, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
                    guiMgr.DrawWindow(windowRect, guiName, false);
                }
            }
        }

        private void BuildOptionsMenu(int id) {
            float buttonAboveHeight = topBottomMargin;

            Rect buttonRect = new Rect(leftRightMargin, buttonAboveHeight += buttonHeight, windowRect.width - (leftRightMargin * 2), buttonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Merchant")) {
                UnitManager.getInstance().SpawnMerchant(Vector3.zero);
            }

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - (leftRightMargin * 2), buttonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Migrant")) {
                UnitManager.getInstance().Migrate(Vector3.zero);
            }

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - (leftRightMargin * 2), buttonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Animal")) {
                UnitManager.getInstance().AddAnimal("random", Vector3.zero, false);
            }

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - (leftRightMargin * 2), buttonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Enemy")) {
                int ranNum = UnityEngine.Random.Range(1, 6);
                string enemyToSpawn = "";
                switch (ranNum) {
                    case 1: enemyToSpawn = "goblin";
                        break;
                    case 2: enemyToSpawn = "mountedGoblin";
                        break;
                    case 3: enemyToSpawn = "skeleton";
                        break;
                    case 4: enemyToSpawn = "wolf";
                        break;
                    case 5: enemyToSpawn = "spider";
                        break;
                    case 6: enemyToSpawn = "necromancer";
                        break;
                    case 7: enemyToSpawn = "spiderLord";
                        break;
                }
                
                UnitManager.getInstance().AddEnemy(enemyToSpawn, getRandomPosition(), false, -1);
            }

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - (leftRightMargin * 2), buttonHeight);
            if (guiMgr.DrawButton(buttonRect, "Kill All Enemies")) {
                UnitManager.getInstance().killAllEnemies();
            }

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Hunger", ref SettingsManager.settings[(int)Preferences.hunger]);

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Fatigue", ref SettingsManager.settings[(int)Preferences.fatigue]);

            buttonRect = new Rect(leftRightMargin, buttonAboveHeight += (buttonHeight + inbetweenMargin), windowRect.width - (leftRightMargin * 2), buttonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Invincible", ref SettingsManager.settings[(int)Preferences.invincible]);

            windowRect.height = buttonAboveHeight + buttonHeight + inbetweenMargin + topBottomMargin;

            GUI.DragWindow();
        }

        private Vector3 getRandomPosition() {
            int num = UnityEngine.Random.Range(1, 5);
            float num2 = 0f;
            float num3 = 0f;
            if (num == 1) {
                num3 = 1f;
                num2 = 0f;
            } else {
                if (num == 2) {
                    num3 = -1f;
                    num2 = 0f;
                } else {
                    if (num == 3) {
                        num3 = 0f;
                        num2 = -1f;
                    } else {
                        if (num == 4) {
                            num3 = 0f;
                            num2 = 1f;
                        }
                    }
                }
            }
            return new Vector3(AManager<WorldManager>.getInstance().GetEdgePosition() * num2 + num3 * (0.1f + (float)UnityEngine.Random.Range(-256, 257) * 0.1f), 0f, AManager<WorldManager>.getInstance().GetEdgePosition() * num3 + num2 * (0.1f + (float)UnityEngine.Random.Range(-256, 257) * 0.1f));
        }

        private void updateGameVariables(object sender, ElapsedEventArgs e) {
            if (SettingsManager.settings[(int)Preferences.toggleCheatMenu]) {
                if (!SettingsManager.settings[(int)Preferences.hunger]) {
                    foreach (APlayableEntity settler in UnitManager.getInstance().playerUnits) {
                        settler.hunger = 0;
                    }
                }
                if (!SettingsManager.settings[(int)Preferences.fatigue]) {
                    foreach (APlayableEntity settler in UnitManager.getInstance().playerUnits) {
                        settler.fatigue = 1;
                    }
                }
                if (SettingsManager.settings[(int)Preferences.invincible]) {
                    foreach (APlayableEntity settler in UnitManager.getInstance().playerUnits) {
                        settler.maxHP = 200f;
                        settler.hitpoints = settler.maxHP;
                    }
                }
            }
        }
    }
}
