using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Timers;
using Timber_and_Stone;
using Timber_and_Stone.Invasion;
using Timber_and_Stone.Utility;
using Random = System.Random;

namespace Plugin.Bobisback.CombinedMods
{

    public class GUIWindowCheatMenu : MonoBehaviour
    {
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(370, 200, 300, 237);
        private static readonly int WindowId = 503;
        private static readonly int InvasionWindowId = 504;
        private static readonly int CustomWindowId = 505;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "Cheat Menu";
        private readonly String invasionGUIName = "Standard Invasion Menu";
        private readonly String customInvasionGUIName = "Custom Invasion Menu";

        private static readonly Timer UpdateTimer = new Timer(500);
        private int[] storageIndexCounts;
        private bool add999Resources;
        private bool resetResouces;
        private bool unlimitedResources;

        private bool standardInvasionMenu;
        private float wealth;
        private int weightedWealth;
        private bool customInvasionMenu;
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

            storageIndexCounts = new int[11];
            foreach (Resource resource in ResourceManager.getInstance().resources.Where(resource => resource != null)) {
                storageIndexCounts[resource.storageIndex]++;
            }

            shownInvasionPoints = "0";
            invasionPoints = 0;
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update()
        {
            if (Input.GetKeyDown(SettingsManager.HotKeys["toggleCheatMenuHotKey"])) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu] == false) {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu] = true;
                    WorldManager.getInstance().PlayerFaction.DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        public void OnGUI()
        {
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleCheatMenu]) {
                    windowRect = GUI.Window(WindowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
                }
                if (standardInvasionMenu) {
                    windowRect = GUI.Window(InvasionWindowId, windowRect, BuildStandardInvasionMenu, string.Empty, guiMgr.windowBoxStyle);
                }
                if (customInvasionMenu) {
                    windowRect = GUI.Window(CustomWindowId, windowRect, BuildCustomInvasionMenu, string.Empty, guiMgr.windowBoxStyle);
                }
            }
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

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Merchant")) {
                UnitManager.getInstance().SpawnMerchant(Vector3.zero);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Migrant")) {
                UnitManager.getInstance().Migrate(Vector3.zero);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Animal")) {
                UnitManager.getInstance().AddAnimal("random", Vector3.zero, false);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Standard Invasion Menu")) {
                //GUIWindowModOptions.DisplayMessage("Disabled", "Feature Disabled For right now. Full invasion menu in the works. Maybe....");
                standardInvasionMenu = true;
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Custom Invasion Menu")) {
                //GUIWindowModOptions.DisplayMessage("Disabled", "Feature Disabled For right now. Full invasion menu in the works. Maybe....");
                customInvasionMenu = true;
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Kill All Enemies")) {
                IFaction playerFaction = WorldManager.getInstance().PlayerFaction;
                // NOTE: UnityEngine.Object.FindObjectsOfType is slow
                foreach (ALivingEntity unit in (ALivingEntity[])FindObjectsOfType(typeof(ALivingEntity))) {
                    if (playerFaction.getAlignmentToward(unit.faction) == Alignment.Enemy) {
                        unit.hitpoints = 0;
                        unit.spottedTimer = float.PositiveInfinity;
                    }
                }
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Add 999 Resources")) {
                if (unlimitedResources) return;

                IStorage storage = WorldManager.getInstance().PlayerFaction.storage;
                ResourceManager resourceManager = ResourceManager.getInstance();

                foreach (Resource resource in resourceManager.resources.Where(resource => resource != null)) {
                    storage.addResource(resource, 999);
                }
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Clear Enemy Corpses"))
            {
                IFaction playerFaction = WorldManager.getInstance().PlayerFaction;
                foreach (ALivingEntity unit in (ALivingEntity[])FindObjectsOfType(typeof(ALivingEntity))) {
                    if (playerFaction.getAlignmentToward(unit.faction) == Alignment.Enemy && !unit.isAlive()) {
                        unit.Destroy();
                    }
                }
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "No Hunger", ref SettingsManager.BoolSettings[(int)Preferences.Hunger]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "No Invasions", ref SettingsManager.BoolSettings[(int)Preferences.NoInvasions]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Show Invasion Info", ref SettingsManager.BoolSettings[(int)Preferences.InvasionsInfo]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Unlimited Resources", ref unlimitedResources);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Invincible", ref SettingsManager.BoolSettings[(int)Preferences.Invincible]);

            //buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            //guiMgr.DrawCheckBox(buttonRect, "Disable Line Of Sight", ref SettingsManager.BoolSettings[(int)Preferences.DisableLOS]);

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void BuildStandardInvasionMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, invasionGUIName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                standardInvasionMenu = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Wealth: " + wealth);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Weighted Wealth: " + weightedWealth);
            
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Easy Invasion (" + (weightedWealth / 2) + ")")) {
                WorldManager.getInstance().SpawnInvasion(weightedWealth / 2);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Normal Invasion (" + weightedWealth + ")")) {
                WorldManager.getInstance().SpawnInvasion(weightedWealth);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Hard Invasion (" + (weightedWealth * 2) + ")")) {
                WorldManager.getInstance().SpawnInvasion(weightedWealth * 2);
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Insane Invasion (" + (weightedWealth * 4) + ")")) {
                WorldManager.getInstance().SpawnInvasion(weightedWealth * 4);
            }

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }
        private void BuildCustomInvasionMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, customInvasionGUIName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                customInvasionMenu = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Wealth: " + wealth);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Weighted Wealth: " + weightedWealth);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, " Necromancer Invasion", ref necromancer);

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
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    invasionPoints -= 10;
                } 
                else
                {
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
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    invasionPoints += 10;
                } else {
                    invasionPoints++;
                }
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Spawn Random Invasion"))
            {
                SpawnInvasion();
            }

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void SpawnInvasion()
        {
            List<IInvasionGenerator> generators = new List<IInvasionGenerator>();

            if (wolf)
            {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is WolfInvasionGenerator));
            }
            if (spider)
            {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is SpiderInvasionGenerator));
            }
            if (necromancer)
            {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is NecromancerInvasionGenerator));
            }
            if (undead)
            {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is SkeletonInvasionGenerator));
            }
            if (goblin)
            {
                generators.Add(WorldManager.getInstance().InvasionGenerators.First(x => x is GoblinInvasionGenerator));
            }

            if (generators.Count == 0)
            {
                GUIWindowModOptions.DisplayMessage("No Invasion Started", "You need at least one invasion type selected.");
                return;
            }
            
            var randomNumber = new Random().Next(generators.Count);
            WorldManager.getInstance().SpawnInvasion(generators[randomNumber].CreateInvasion(invasionPoints));
        }

        private int CalculateWeightedWealth()
        {
            float num = -650f;
            num += AManager<ResourceManager>.getInstance().getWealth();
            num += 100f * AManager<WorldManager>.getInstance().PlayerFaction.LiveUnitCount();
            num += 50f * AManager<TimeManager>.getInstance().day;
            return Mathf.FloorToInt(num);
        }

        private Vector3 GetRandomPosition()
        {
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
            return new Vector3(AManager<WorldManager>.getInstance().GetEdgePosition() * num2 + num3 * (0.1f + UnityEngine.Random.Range(-256, 257) * 0.1f), 0f, AManager<WorldManager>.getInstance().GetEdgePosition() * num3 + num2 * (0.1f + UnityEngine.Random.Range(-256, 257) * 0.1f));
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            wealth = AManager<ResourceManager>.getInstance().getWealth();
            weightedWealth = CalculateWeightedWealth();

            if (SettingsManager.BoolSettings[(int)Preferences.DisableLOS])
            {
                //TODO
            }

            foreach (APlayableEntity settler in WorldManager.getInstance().PlayerFaction.units.OfType<APlayableEntity>().Where(x => x.isAlive())) {
                if (!SettingsManager.BoolSettings[(int)Preferences.Hunger]) {
                    settler.hunger = 0;
                }
                if (SettingsManager.BoolSettings[(int)Preferences.Invincible]) {
                    settler.maxHP = 200f;
                    settler.hitpoints = settler.maxHP;
                } else {
                    settler.maxHP = 100f;
                }
            }

            if (unlimitedResources) {
                IStorageController storage = WorldManager.getInstance().PlayerFaction.storage as IStorageController;

                if (storage == null) {
                    GUIManager.getInstance().AddTextLine("Storage failed");
                    return;
                }

                ResourceManager resourceManager = ResourceManager.getInstance();

                foreach (Resource resource in resourceManager.resources.Where(resource => resource != null)) {
                    storage.setStorageCap(resource.storageIndex, 10000);
                    var totalInStorageIndex = storageIndexCounts[resource.storageIndex];

                    if (totalInStorageIndex == 0 || resource.mass == 0) {
                        continue;
                    }

                    var totalMassAvailablePerStorageIndex = (7999 / totalInStorageIndex);

                    var qty = (int)(totalMassAvailablePerStorageIndex / resource.mass);

                    storage.setResource(resource, qty);
                }
            }

            if (add999Resources) {
                add999Resources = false;

                IStorage storage = WorldManager.getInstance().PlayerFaction.storage;
                ResourceManager resourceManager = ResourceManager.getInstance();

                foreach (Resource resource in resourceManager.resources) {
                    storage.addResource(resource, 999);
                }
            }
        }
    }
}
