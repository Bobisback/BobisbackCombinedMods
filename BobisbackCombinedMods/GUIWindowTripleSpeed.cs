using UnityEngine;

namespace Plugin.Bobisback.CombinedMods {

    /// <summary>
    /// This class handles all game logic and display of the Increased game speed mod.
    /// </summary>
    public class GuiWindowTripleSpeed : MonoBehaviour {
      
        private readonly GUIManager guiMgr = GUIManager.getInstance();

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        void Update() {
            if (Input.GetKeyDown(SettingsManager.HotKeys["toggleTripleSpeedHotKey"])) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed] == false) {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed] = true;
                    AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        void OnGui() {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed]) {
                    GameSpeedGui();
                }
            }
        }

        //This function just displays the Gmae Speed GUI at the top right of the screen.
        private void GameSpeedGui() {
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