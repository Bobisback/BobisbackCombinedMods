using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Timers;

namespace Plugin.Bobisback.CombinedMods
{
    public class GUIWindowReviveTheFallen : MonoBehaviour
    {
        private const float ButtonHeight = 32;
        private const float LeftRightMargin = 15;
        private const float TopBottomMargin = 7.5f;
        private const float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(60, 140, 300, 126);
        private static readonly int WindowId = 506;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "Revive The Fallen";
        private readonly List<ALivingEntity> theFallen = new List<ALivingEntity>();

        private static readonly Timer UpdateTimer = new Timer(500);
        
        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();
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
                if (GUIWindowCheatMenu.ReviveTheFallenMenu) {
                    windowRect = GUI.Window(WindowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }
        }

        private void BuildOptionsMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, guiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                GUIWindowCheatMenu.ReviveTheFallenMenu = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            bool isOdd = true;

            //display all the idle settings in a neat way
            if (theFallen.Count == 0) {
                Rect labelRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + TopBottomMargin), windowRect.width - LeftRightMargin * 2, ButtonHeight);
                guiMgr.DrawTextCenteredBlack(labelRect, "None");
            } else {
                for (int i = 0; i < theFallen.Count; i++) {
                    isOdd = i % 2 != 0;
                    BuildLabel(theFallen.ElementAt(i), isOdd ? buttonAboveHeight += (ButtonHeight + InbetweenMargin) : buttonAboveHeight + (ButtonHeight + InbetweenMargin), isOdd);
                }
            }

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin + (isOdd ? 0 : ButtonHeight + InbetweenMargin);
            GUI.DragWindow();
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            if (!guiMgr.inGame || guiMgr.gameOver || !GUIWindowCheatMenu.ReviveTheFallenMenu) return;// make sure we are in the game

            foreach (ALivingEntity aLivingEntity in UnitManager.getInstance().allUnits) {
                if (!aLivingEntity.isAlive()) {
                    if (!theFallen.Contains(aLivingEntity)) {
                        theFallen.Add(aLivingEntity);
                    }
                }
            }

            theFallen.RemoveAll(x => x == null || x.isAlive());
        }
        
        /// <summary>
        /// This will build a button to display a dead settler. 
        /// It also has the logic in it to handle moving to and selecting a settler.
        /// </summary>
        /// <param name="settler">The settler that needs to be displayed</param>
        /// <param name="buttonAboveHeight">The y coord needed relative to all the views above it</param>
        /// <param name="isOdd">Is this a odd button.</param>
        private void BuildLabel(ALivingEntity settler, float buttonAboveHeight, bool isOdd)
        {
            Rect viewRect;
            if (isOdd) {
                viewRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight, (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            } else {
                viewRect = new Rect(LeftRightMargin, buttonAboveHeight, (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            }

            if (guiMgr.DrawButton(viewRect, settler.unitName.Split(' ').FirstOrDefault()))
            {
                if (settler is HumanEntity)
                {
                    settler.hitpoints = settler.maxHP;
                    settler.hunger = 0f;
                    settler.interruptTask();
                    settler.faction = WorldManager.getInstance().PlayerFaction;
                }
                else
                {
                    GUIManager.getInstance().AddTextLine("Can only revive Humans");
                }
            }
        }
    }
}
