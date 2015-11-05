using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Timber_and_Stone;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class GUIWindowHumanSettlerTab : MonoBehaviour, IEventListener
    {
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(30, 120, 300, 237);
        private static readonly int WindowId = 512;

        private HumanSettlerWindow settlerWindow = GUIManager.getInstance().settlerWindow;
        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "";

        private static readonly Timer UpdateTimer = new Timer(500);
        private bool highLightTab;
        private Vector2 preferencesScrollPosition;
        private int prefWindowHeight;

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();

            EventManager.getInstance().Register(this);
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update()
        {
            /*if (Input.GetKeyDown(SettingsManager.HotKeys["toggleCheatMenuHotKey"])) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu] == false) {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu] = true;
                    WorldManager.getInstance().PlayerFaction.DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu] = false;
                }
            }*/
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        public void OnGUI()
        {
            if (this.guiMgr.inGame && settlerWindow.entity != null && !settlerWindow.rename) {
                DrawTabs();
                CheckForOtherTabClicks();
                //if (settlerWindow.inventorySlot == 0) {
                //    this.DrawTabs();
                //}
                //this.windowRect = GUI.Window(5, this.windowRect, new GUI.WindowFunction(this.RenderWindow), string.Empty, this.guiMgr.hiddenButtonStyle);
                GUI.depth = 4999;
                //if (this.inventorySlot > 0) {
                //    this.InventorySlotPrefs(this.windowRect, this.entity);
                //}
                //GUI.depth = 0;
                //GUI.FocusWindow(5);
                if (highLightTab)
                {
                    GUI.Window(5, this.windowRect, new GUI.WindowFunction(RenderWindow), string.Empty, this.guiMgr.hiddenButtonStyle);
                }
            }
            /*if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu]) {
                    windowRect = GUI.Window(WindowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }*/

            windowRect = settlerWindow.windowRect;
        }

        private void RenderWindow(int id)
        {
            Rect position = new Rect(windowRect.xMin + 6f, windowRect.yMin + 40f, windowRect.width - 20f, windowRect.height - 52f);
            Rect viewRect = new Rect(0f, 0f, 40f, ((prefWindowHeight + 1) * 24));
            preferencesScrollPosition = GUI.BeginScrollView(position, preferencesScrollPosition, viewRect);

            if (windowRect.Contains(Event.current.mousePosition)) {
                guiMgr.mouseInGUI = true;
            }

        }

        private void DrawTabs()
        {
            Rect location = new Rect(windowRect.x - 46f, windowRect.y + 210f, 48f, 48f);
            guiMgr.DrawWindow(location, string.Empty, false, true);
            if (location.Contains(Event.current.mousePosition)) {
                guiMgr.mouseInGUI = true;
            }
            if (GUI.Button(new Rect(location.x + 6f, location.y + 6f, 36f, 36f), string.Empty, this.guiMgr.hiddenButtonStyle))
            {
                highLightTab = true;
            }
            if (highLightTab) {
                GUI.DrawTexture(new Rect(location.x + 6f, location.y + 6f, 36f, 36f), this.guiMgr.tile_preferences2);
            } else {
                GUI.DrawTexture(new Rect(location.x + 6f, location.y + 6f, 36f, 36f), this.guiMgr.tile_preferences);
            }
        }

        private void CheckForOtherTabClicks()
        {
            
        }

        private void BuildOptionsMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, guiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            //Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            //if (guiMgr.DrawButton(buttonRect, "")) {
                
            //}

            //buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            //guiMgr.DrawCheckBox(buttonRect, "", ref SettingsManager.BoolSettings[(int)Preferences.EternalNight]);

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            
        }
    }
}
