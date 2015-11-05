using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Timers;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using Timber_and_Stone.Invasion;
using Timber_and_Stone.Utility;

namespace Plugin.Bobisback.CombinedMods
{

    public class GUIWindowCreateInvasionMenu : MonoBehaviour, IEventListener
    {
        private const float ButtonHeight = 32;
        private const float LeftRightMargin = 15;
        private const float TopBottomMargin = 7.5f;
        private const float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(0, 40, 360, 237);
        private const int CreateInvasionWindowId = 505;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String createInvasionGuiName = "Spawn Invasion Menu";

        private static readonly Timer UpdateTimer = new Timer(500);

        private float wealth;
        private int weightedWealth;
        private bool undead;
        private bool goblin;
        private bool spider;
        private bool wolf;
        private string shownInvasionPoints;
        private int invasionPoints;
        private bool necromancer;

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();
            shownInvasionPoints = "0";
            invasionPoints = 0;

            windowRect.x = Screen.width - 130 - windowRect.width;

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
                if (GUIWindowCheatMenu.CreateInvasionMenu) {
                    windowRect = GUI.Window(CreateInvasionWindowId, windowRect, BuildCreateInvasionMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }

            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }

        private void BuildCreateInvasionMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, createInvasionGuiName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                GUIWindowCheatMenu.CreateInvasionMenu = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Respect Difficulty Menu", ref SettingsManager.BoolSettings[(int)Preferences.RespectDifficultyMenu]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Wealth: " + wealth);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Weighted Wealth: " + weightedWealth);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Easy Invasion (" + (weightedWealth / 2) + ")")) {
                if (!SettingsManager.BoolSettings[(int)Preferences.RespectDifficultyMenu]) {
                    GUIWindowInvasionDifficultyMenu.InvasionTiggeredByMod = true;
                }
                WorldManager.getInstance().SpawnInvasion(weightedWealth / 2);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Normal Invasion (" + weightedWealth + ")")) {
                if (!SettingsManager.BoolSettings[(int)Preferences.RespectDifficultyMenu]) {
                    GUIWindowInvasionDifficultyMenu.InvasionTiggeredByMod = true;
                }
                WorldManager.getInstance().SpawnInvasion(weightedWealth);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Hard Invasion (" + (weightedWealth * 2) + ")")) {
                if (!SettingsManager.BoolSettings[(int)Preferences.RespectDifficultyMenu]) {
                    GUIWindowInvasionDifficultyMenu.InvasionTiggeredByMod = true;
                }
                WorldManager.getInstance().SpawnInvasion(weightedWealth * 2);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Insane Invasion (" + (weightedWealth * 4) + ")")) {
                if (!SettingsManager.BoolSettings[(int) Preferences.RespectDifficultyMenu])
                {
                    GUIWindowInvasionDifficultyMenu.InvasionTiggeredByMod = true;
                }
                WorldManager.getInstance().SpawnInvasion(weightedWealth * 4);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Necromancer Invasion", ref necromancer);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Undead Invasion", ref undead);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Golbin Invasion", ref goblin);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Spider Invasion", ref spider);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Wolf Invasion", ref wolf);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Invasion Points: ");
            buttonRect.x += 150;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    invasionPoints -= 10;
                } else {
                    invasionPoints--;
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownInvasionPoints = "" + invasionPoints;
            shownInvasionPoints = guiMgr.DrawTextFieldCenteredWhite("invasionPoints", buttonRect, shownInvasionPoints, 6);
            int.TryParse(shownInvasionPoints, out invasionPoints);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    invasionPoints += 10;
                } else {
                    invasionPoints++;
                }
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Invasion With Settings")) {
                SpawnInvasion();
            }

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void SpawnInvasion()
        {
            List<IInvasionGenerator> generators = new List<IInvasionGenerator>();

            if (wolf) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is WolfInvasionGenerator));
            }
            if (spider) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is SpiderInvasionGenerator));
            }
            if (necromancer) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is NecromancerInvasionGenerator));
            }
            if (undead) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is SkeletonInvasionGenerator));
            }
            if (goblin) {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is GoblinInvasionGenerator));
            }

            if (generators.Count == 0) {
                GUIWindowModOptions.DisplayMessage("No Invasion Started", "You need at least one invasion type selected.");
                return;
            }

            if (!SettingsManager.BoolSettings[(int)Preferences.RespectDifficultyMenu]) {
                GUIWindowInvasionDifficultyMenu.InvasionTiggeredByMod = true;
            }

            IInvasionGenerator invasionGenerator = generators.WeightedRandomElement(element => element.getPriority());

            if (invasionGenerator == null) {
                GUIManager.getInstance().AddTextLine("Invasion Failed to Spawn");
            } else {
                WorldManager.getInstance().SpawnInvasion(invasionGenerator.CreateInvasion(invasionPoints));
            }
        }

        private int CalculateWeightedWealth()
        {
            float num = -650f;
            num += AManager<ResourceManager>.getInstance().getWealth();
            num += 100f * AManager<WorldManager>.getInstance().PlayerFaction.LiveUnitCount();
            num += 50f * AManager<TimeManager>.getInstance().day;
            return Mathf.FloorToInt(num);
        }
        
        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            wealth = AManager<ResourceManager>.getInstance().getWealth();
            weightedWealth = CalculateWeightedWealth();
        }
    }
}
