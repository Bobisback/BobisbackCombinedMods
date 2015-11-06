using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class GUIWindowSettlerTraitsMenu : MonoBehaviour, IEventListener
    {
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(30, 120, 450, 237);
        private static readonly int WindowId = 514;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "Settler Trait's menu";

        private static readonly Timer UpdateTimer = new Timer(500);
        
        public static readonly Dictionary<string, bool> traitsDictionary = new Dictionary<string, bool>
        {
            {"trait.athletic", false}, {"trait.sluggish",  false}, {"trait.clumsy",  false}, 
            {"trait.quicklearner",  false}, {"trait.cowardly",  false}, {"trait.strongback", false}, 
            {"trait.weakback",  false}, {"trait.hardworker",  false}, {"trait.lazy",  false}, 
            {"trait.badvision",  false}, {"trait.goodvision", false}, {"trait.charismatic",  false}, 
            {"trait.courageous",  false}, {"trait.disloyal",  false}, {"trait.overeater", false}, 
        };

        private bool applyTraitsToSettlers;

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();

            windowRect.x = Screen.width - 350 - windowRect.width;

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
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleSettlerTraitsMenu]) {
                    windowRect = GUI.Window(WindowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }

            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }

        private void BuildOptionsMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, guiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                SettingsManager.BoolSettings[(int)Preferences.ToggleSettlerTraitsMenu] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Remove Selected From Current Settlers"))
            {
                applyTraitsToSettlers = true;
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Remove Selected From New Settlers", ref SettingsManager.BoolSettings[(int)Preferences.ApplyTraitsToNewSettlers]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);            
            guiMgr.DrawTextCenteredBlack(buttonRect, "Select Traits To Remove Below:");

            bool isOdd = true;

            for (int i = 0; i < traitsDictionary.Keys.Count(); i++) {
                isOdd = i % 2 != 0;
                BuildLabel(traitsDictionary.Keys.ElementAt(i), i % 2 != 0 ? buttonAboveHeight += (ButtonHeight + InbetweenMargin) : buttonAboveHeight + (ButtonHeight + InbetweenMargin), isOdd);
            }

            windowRect.height = windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin + (isOdd ? 0 : ButtonHeight + InbetweenMargin);
            
            GUI.DragWindow();
        }

        private void BuildLabel(string key, float buttonAboveHeight, bool isOdd)
        {
            Rect viewRect;
            if (isOdd) {
                viewRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight, (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            } else {
                viewRect = new Rect(LeftRightMargin, buttonAboveHeight, (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            }

            bool boolValue = traitsDictionary[key];
            guiMgr.DrawCheckBox(viewRect, key.Replace("trait.", ""), ref boolValue);
            traitsDictionary[key] = boolValue;
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            if (applyTraitsToSettlers)
            {
                foreach (APlayableEntity entity in WorldManager.getInstance().PlayerFaction.units.OfType<APlayableEntity>().Where(x => x.isAlive()))
                {
                    foreach (KeyValuePair<string, bool> trait in traitsDictionary.Where(x => x.Value))
                    {
                        entity.preferences[trait.Key] = false;
                    }
                }
                applyTraitsToSettlers = false;
            }
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnEntityDeath(EventMigrant evt)
        {
            if (SettingsManager.BoolSettings[(int)Preferences.ApplyTraitsToNewSettlers]) {
                foreach (KeyValuePair<string, bool> trait in traitsDictionary.Where(x => x.Value)) {
                    evt.unit.preferences[trait.Key] = false;
                }
            }
        }
    }
}
