using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Timber_and_Stone.API;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using EventHandler = Timber_and_Stone.API.Event.EventHandler;

namespace Plugin.Bobisback.CombinedMods {
    public class GUIWindowTripleSpeed : MonoBehaviour {
      
        private GUIManager guiMgr = GUIManager.getInstance();

        void Update() {
            if (Input.GetKeyDown(SettingsManager.hotKeys["toggleTripleSpeedHotKey"])) {
                if (SettingsManager.boolSettings[(int)Preferences.toggleTripleSpeed] == false) {
                    SettingsManager.boolSettings[(int)Preferences.toggleTripleSpeed] = true;
                    AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.boolSettings[(int)Preferences.toggleTripleSpeed] = false;
                }
            }
        }

        void OnGUI() {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.boolSettings[(int)Preferences.toggleTripleSpeed]) {
                    GameSpeedGUI();
                }
            }
        }

        public void GameSpeedGUI() {
            int num = Screen.width - 120;
            int num2 = 88;
            if (Screen.width > 1520) {
                num2 -= 40;
            }
            guiMgr.DrawWindow(new Rect((float)(num-10), (float)(num2 + 5), 128f, 24f), string.Empty, false, true);

            if (GUI.Button(new Rect((float)(num - 5), (float)(num2 + 4), 26f, 26f), "3x", this.guiMgr.blankButtonStyle)) {
                AManager<TimeManager>.getInstance().play(3f);
                GUIManager.getInstance().AddTextLine("3x Speed");
            }
            if (GUI.Button(new Rect((float)(num + 27), (float)(num2 + 4), 26f, 26f), "4x", this.guiMgr.blankButtonStyle)) {
                AManager<TimeManager>.getInstance().play(4f);
                GUIManager.getInstance().AddTextLine("4x Speed");
            }
            if (GUI.Button(new Rect((float)(num + 58), (float)(num2 + 4), 26f, 26f), "5x", this.guiMgr.blankButtonStyle)) {
                AManager<TimeManager>.getInstance().play(5f);
                GUIManager.getInstance().AddTextLine("5x Speed");
            }
            if (GUI.Button(new Rect((float)(num + 84), (float)(num2 + 4), 26f, 26f), "6x", this.guiMgr.blankButtonStyle)) {
                AManager<TimeManager>.getInstance().play(6f);
                GUIManager.getInstance().AddTextLine("6x Speed");
            }
            if (Time.timeScale == 3f) {
                GUI.DrawTexture(new Rect((float)(num - 8), (float)(num2 + 1), 32f, 32f), this.guiMgr.gameSpeedSelector);
            } else {
                if (Time.timeScale == 4f) {
                    GUI.DrawTexture(new Rect((float)(num + 26), (float)(num2 + 1), 32f, 32f), this.guiMgr.gameSpeedSelector);
                } else if (Time.timeScale == 5f) {
                    GUI.DrawTexture(new Rect((float)(num + 57), (float)(num2 + 1), 32f, 32f), this.guiMgr.gameSpeedSelector);
                } else if (Time.timeScale == 6f) {
                    GUI.DrawTexture(new Rect((float)(num + 83), (float)(num2 + 1), 32f, 32f), this.guiMgr.gameSpeedSelector);
                }
            }
        }
    }
}