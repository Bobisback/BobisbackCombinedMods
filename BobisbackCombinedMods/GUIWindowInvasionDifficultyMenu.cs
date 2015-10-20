using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using Timber_and_Stone.Invasion;
using Timber_and_Stone.Utility;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods 
{

    public class GUIWindowInvasionDifficultyMenu : MonoBehaviour, IEventListener
    {
        private const float ButtonHeight = 32;
        private const float LeftRightMargin = 15;
        private const float TopBottomMargin = 7.5f;
        private const float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(200, 40, 375, 237);
        private const int InvasionDifficultyWindowId = 504;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String invasionDifficultyGUIName = "Invasion Difficulty Menu";

        private static readonly Timer UpdateTimer = new Timer(500);

        private int weightedWealth;
        private string shownDifficultyPrecent;

        public static bool InvasionTiggeredByMod;

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start() {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();
            shownDifficultyPrecent = "100%";

            EventManager.getInstance().Register(this);
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update() {
            if (Input.GetKeyDown(SettingsManager.HotKeys["toggleInvasionDifficultyMenuHotKey"])) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleInvasionDifficultyMenu] == false) {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleInvasionDifficultyMenu] = true;
                    WorldManager.getInstance().PlayerFaction.DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleInvasionDifficultyMenu] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        public void OnGUI() {
            if (SettingsManager.BoolSettings[(int)Preferences.ToggleInvasionDifficultyMenu]) {
                windowRect = GUI.Window(InvasionDifficultyWindowId, windowRect, BuildInvasionDifficultyMenu, string.Empty, guiMgr.windowBoxStyle);
            }

            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }

        private void BuildInvasionDifficultyMenu(int id) {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, invasionDifficultyGUIName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                SettingsManager.BoolSettings[(int)Preferences.ToggleInvasionDifficultyMenu] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Difficulty Settings Enabled", ref SettingsManager.BoolSettings[(int)Preferences.DifficultySettingsEnabled]);
            
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "No Necromancer Invasions", ref SettingsManager.BoolSettings[(int)Preferences.NoNecromancerDifficultySetting]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "No Undead Invasions", ref SettingsManager.BoolSettings[(int)Preferences.NoUndeadDifficultySetting]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "No Golbin Invasions", ref SettingsManager.BoolSettings[(int)Preferences.NoGoblinDifficultySetting]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "No Spider Invasions", ref SettingsManager.BoolSettings[(int)Preferences.NoSpiderDifficultySetting]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "No Wolf Invasions", ref SettingsManager.BoolSettings[(int)Preferences.NoWolfDifficultySetting]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Difficulty Precentage: ");
            buttonRect.x += 225;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (SettingsManager.DifficultyPrecentAsInt >= 11) {
                        SettingsManager.DifficultyPrecentAsInt -= 10;
                    }
                } else {
                    if (SettingsManager.DifficultyPrecentAsInt >= 2) {
                        SettingsManager.DifficultyPrecentAsInt--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownDifficultyPrecent = SettingsManager.DifficultyPrecentAsInt + "%";
            shownDifficultyPrecent = guiMgr.DrawTextFieldCenteredWhite("invasionPoints", buttonRect, shownDifficultyPrecent, 4);
            int.TryParse(shownDifficultyPrecent.Replace("%", ""), out SettingsManager.DifficultyPrecentAsInt);
            if (SettingsManager.DifficultyPrecentAsInt > 999)
            {
                SettingsManager.DifficultyPrecentAsInt = 999;
            }
            if (SettingsManager.DifficultyPrecentAsInt < 1)
            {
                SettingsManager.DifficultyPrecentAsInt = 1;
            }
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (SettingsManager.DifficultyPrecentAsInt < 990)
                    {
                        SettingsManager.DifficultyPrecentAsInt += 10;
                    }
                } else {
                    if (SettingsManager.DifficultyPrecentAsInt < 999)
                    {
                        SettingsManager.DifficultyPrecentAsInt++;
                    }
                }
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Weighted Wealth: " + (!guiMgr.inGame ? "n/a" : "" + weightedWealth));

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "New Difficulty: " + (!guiMgr.inGame ? "n/a" : "" + weightedWealth * ((float)SettingsManager.DifficultyPrecentAsInt / 100)));

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void SpawnInvasion() {
            List<IInvasionGenerator> generators = new List<IInvasionGenerator>();

            if (!SettingsManager.BoolSettings[(int)Preferences.NoWolfDifficultySetting]) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is WolfInvasionGenerator));
            }
            if (!SettingsManager.BoolSettings[(int)Preferences.NoSpiderDifficultySetting]) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is SpiderInvasionGenerator));
            }
            if (!SettingsManager.BoolSettings[(int)Preferences.NoNecromancerDifficultySetting]) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is NecromancerInvasionGenerator));
            }
            if (!SettingsManager.BoolSettings[(int)Preferences.NoUndeadDifficultySetting]) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is SkeletonInvasionGenerator));
            }
            if (!SettingsManager.BoolSettings[(int)Preferences.NoGoblinDifficultySetting]) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is GoblinInvasionGenerator));
            }

            if (generators.Count == 0) {
                GUIWindowModOptions.DisplayMessage("No Invasion Started", "You need at least one invasion type selected.");
                return;
            }

            float difficultyPrecent = (float)SettingsManager.DifficultyPrecentAsInt/100;

            IInvasionGenerator invasionGenerator = generators.WeightedRandomElement(element => element.getPriority());

            if (invasionGenerator == null) {
                GUIManager.getInstance().AddTextLine("Invasion Failed to Spawn");
            } else {
                WorldManager.getInstance().SpawnInvasion(invasionGenerator.CreateInvasion((int)(weightedWealth * difficultyPrecent)));
            }
            //GUIManager.getInstance().AddTextLine("Spawned Invasion with weight of: " + (weightedWealth * difficultyPrecent));
        }

        private int CalculateWeightedWealth() {
            float num = -650f;
            num += AManager<ResourceManager>.getInstance().getWealth();
            num += 100f * AManager<WorldManager>.getInstance().PlayerFaction.LiveUnitCount();
            num += 50f * AManager<TimeManager>.getInstance().day;
            return Mathf.FloorToInt(num);
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e) {
            weightedWealth = CalculateWeightedWealth();

            guiMgr.AddTextLine("x: " + windowRect.x + " y: " + windowRect.y);
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnInvasionNormal(EventInvasion evt) {
            //if there are no invasions cancel it and move on
            if (SettingsManager.BoolSettings[(int)Preferences.NoInvasions]) {
                evt.result = Result.Deny;
                InvasionTiggeredByMod = false;
                return;
            }

            if (SettingsManager.BoolSettings[(int)Preferences.DifficultySettingsEnabled] && SettingsManager.DifficultyPrecentAsInt != 100)
            {
                //if it was not us that tiggered the invasion then cancel it and spawn a new one
                if (!InvasionTiggeredByMod)
                {
                    evt.result = Result.Deny;
                    InvasionTiggeredByMod = true;
                    SpawnInvasion(); //spawn a new invasion with the right difficulty
                }
                else
                {
                    InvasionTiggeredByMod = false;
                }
            }
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Monitor)]
        public void OnInvasionMonitor(EventInvasion evt) {

            if (SettingsManager.BoolSettings[(int) Preferences.InvasionsInfo])
            {
                if (evt.result == Result.Deny && SettingsManager.BoolSettings[(int)Preferences.NoInvasions])
                {
                    GUIManager.getInstance().AddTextLine("A " + evt.invasion.getName() + " invasion has been cancelled");
                    return;
                }

                //make sure the invasion is going to spawn before talking about it
                if (evt.result != Result.Deny)
                {
                    GUIManager.getInstance().AddTextLine("A " + evt.invasion.getName() + " invasion of " + evt.invasion.getUnits().Count + " units has spawned.");
                }
            }
        }
    }
}
