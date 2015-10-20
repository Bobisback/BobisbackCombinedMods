using System;
using System.Timers;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using Timber_and_Stone.Tasks;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class GUIWindowTradeOverHaul : MonoBehaviour, IEventListener
    {
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(30, 240, 300, 237);
        private static readonly int tradeMenuSettingsWindowId = 510;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String tradeMenuSettingsName = "Trade Menu Settings";

        private static readonly Timer UpdateTimer = new Timer(500);
        private ALivingEntity merchant;
        
        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();

            windowRect.x = Screen.width - 30 - windowRect.width;

            EventManager.getInstance().Register(this);
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update()
        {
            
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        public void OnGUI()
        {
            if (guiMgr.inGame && !guiMgr.gameOver) {

                /*if (GUIManager.getInstance().merchantTrade != null)
                {
                    TimeManager.getInstance().play();
                    GUIManager.getInstance().merchantTrade = null;
                }*/
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleTradeSettingsMenu]) {
                    windowRect = GUI.Window(tradeMenuSettingsWindowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }

            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }

        private void BuildOptionsMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, tradeMenuSettingsName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                SettingsManager.BoolSettings[(int)Preferences.ToggleTradeSettingsMenu] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            //Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            //if (guiMgr.DrawButton(buttonRect, "Spawn Merchant")) {
            //    UnitManager.getInstance().SpawnMerchant(Vector3.zero);
            //}

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Activate New Trade Menu", ref SettingsManager.BoolSettings[(int)Preferences.ToggleNewTradeMenu]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Trade Going on", ref SettingsManager.BoolSettings[(int)Preferences.TradeOnGoing]);
            
            //TODO set some things like amount of gold the merchant has

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            /*if (merchant != null) {
                if (merchant.taskStackContains(typeof(TaskExitMapViaRoads))) {
                    SettingsManager.BoolSettings[(int)Preferences.TradeOnGoing] = false;
                }
                GUIManager.getInstance().AddTextLine("What is merchant doing: " + merchant.getWhatImDoing());
            } else {
                GUIManager.getInstance().AddTextLine("its null");
                SettingsManager.BoolSettings[(int)Preferences.TradeOnGoing] = false;
            }*/
        }
        
        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnMerchantTrade(EventTrade evt)
        {
            //evt.result = Result.Deny;
            //TimeManager.getInstance().play();
            
            /*if (trader != null)
            {
                trader.interruptTask();
                GUIManager.getInstance().AddTextLine("Trader interupted");
            }*/

            //GUIManager.getInstance().AddTextLine("Trade should be canceled");
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnMerchantTrade(EventMerchantArrived evt)
        {
            //SettingsManager.BoolSettings[(int) Preferences.TradeOnGoing] = true;
            //merchant = evt.getUnit();
            //evt.result = Result.Deny;
            //GUIManager.getInstance().AddTextLine("Name " + merchant.name);
        }
    }
}
