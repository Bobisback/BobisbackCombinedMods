using UnityEngine;

namespace Plugin.Bobisback.CombinedMods {

    /// <summary>
    /// This class handles all game logic and display of the Increased game speed mod.
    /// </summary>
    public class GUIWindowTripleSpeed : MonoBehaviour {
      
        private readonly GUIManager guiMgr = GUIManager.getInstance();

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update() 
        {
            if (Input.GetKeyDown(SettingsManager.HotKeys["toggleTripleSpeedHotKey"])) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed] == false) {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed] = true;
                    WorldManager.getInstance().PlayerFaction.DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        public void OnGUI()
        {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleTripleSpeed]) {
                    GameSpeedGUI();
                }
            }
        }

        //This function just displays the Gmae Speed GUI at the top right of the screen.
        private void GameSpeedGUI() {
            int num = Screen.width - 120;
            int num2 = 88;
            if (Screen.width > 1520) {
                num2 -= 40;
            }
            guiMgr.DrawWindow(new Rect(num-10, num2 + 5, 128f, 24f), string.Empty, false, true);

            if (GUI.Button(new Rect(num - 5, num2 + 4, 26f, 26f), "3x", guiMgr.blankButtonStyle)) {
                AManager<TimeManager>.getInstance().play(3f);
                GUIManager.getInstance().AddTextLine("3x Speed");
            }
            if (GUI.Button(new Rect(num + 27, num2 + 4, 26f, 26f), "4x", guiMgr.blankButtonStyle)) {
                AManager<TimeManager>.getInstance().play(4f);
                GUIManager.getInstance().AddTextLine("4x Speed");
            }
            if (GUI.Button(new Rect(num + 58, num2 + 4, 26f, 26f), "5x", guiMgr.blankButtonStyle)) {
                AManager<TimeManager>.getInstance().play(5f);
                GUIManager.getInstance().AddTextLine("5x Speed");
            }
            if (GUI.Button(new Rect(num + 84, num2 + 4, 26f, 26f), "6x", guiMgr.blankButtonStyle)) {
                AManager<TimeManager>.getInstance().play(6f);
                GUIManager.getInstance().AddTextLine("6x Speed");
            }
            if (Time.timeScale == 3f) {
                GUI.DrawTexture(new Rect(num - 8, num2 + 1, 32f, 32f), guiMgr.gameSpeedSelector);
            } else {
                if (Time.timeScale == 4f) {
                    GUI.DrawTexture(new Rect(num + 26, num2 + 1, 32f, 32f), guiMgr.gameSpeedSelector);
                } else if (Time.timeScale == 5f) {
                    GUI.DrawTexture(new Rect(num + 57, num2 + 1, 32f, 32f), guiMgr.gameSpeedSelector);
                } else if (Time.timeScale == 6f) {
                    GUI.DrawTexture(new Rect(num + 83, num2 + 1, 32f, 32f), guiMgr.gameSpeedSelector);
                }
            }
        }
    }
}