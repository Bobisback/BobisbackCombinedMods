using System;
using System.Linq;
using UnityEngine;
using System.Timers;
using Timber_and_Stone;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;

namespace Plugin.Bobisback.CombinedMods
{

    public class GUIWindowCheatMenu : MonoBehaviour, IEventListener
    {
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(370, 200, 300, 237);
        private static readonly int WindowId = 503;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "Cheat Menu";

        private static readonly Timer UpdateTimer = new Timer(500);
        private int[] storageIndexCounts;
        private bool add999Resources;

        public static bool CustomInvasionMenu;
        public static bool StandardInvasionMenu;
        public static bool ReviveTheFallenMenu;

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

            EventManager.getInstance().Register(this);
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
                StandardInvasionMenu = true;
                CustomInvasionMenu = false;
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Custom Invasion Menu")) {
                CustomInvasionMenu = true;
                StandardInvasionMenu = false;
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Revive The Fallen")) {
                ReviveTheFallenMenu = true;
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
                if (SettingsManager.BoolSettings[(int)Preferences.UnlimitedResources]) return;

                IStorage storage = WorldManager.getInstance().PlayerFaction.storage;
                ResourceManager resourceManager = ResourceManager.getInstance();

                foreach (Resource resource in resourceManager.resources.Where(resource => resource != null)) {
                    storage.addResource(resource, 999);
                }
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Clear All Corpses"))
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
            guiMgr.DrawCheckBox(buttonRect, "Unlimited Resources", ref SettingsManager.BoolSettings[(int)Preferences.UnlimitedResources]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Invincible", ref SettingsManager.BoolSettings[(int)Preferences.Invincible]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Eternal Light", ref SettingsManager.BoolSettings[(int)Preferences.EternalLight]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Eternal Night", ref SettingsManager.BoolSettings[(int)Preferences.EternalNight]);

            //buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            //guiMgr.DrawCheckBox(buttonRect, "Disable Line Of Sight", ref SettingsManager.BoolSettings[(int)Preferences.DisableLOS]);

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            if (SettingsManager.BoolSettings[(int)Preferences.DisableLOS])
            {
                //TODO
            }

            if (SettingsManager.BoolSettings[(int)Preferences.EternalLight])
            {
                SettingsManager.BoolSettings[(int) Preferences.EternalNight] = false;
                TimeManager.getInstance().hour = 12;
                TimeManager.getInstance().time = 12f;
                TimeManager.getInstance().timeOfDay = "Eternal Light";
                guiMgr.UpdateTime(AManager<TimeManager>.getInstance().day, AManager<TimeManager>.getInstance().timeOfDay);
            }

            if (SettingsManager.BoolSettings[(int)Preferences.EternalNight])
            {
                TimeManager.getInstance().hour = 0;
                TimeManager.getInstance().time = 0f;
                TimeManager.getInstance().timeOfDay = "Eternal Night";
                guiMgr.UpdateTime(TimeManager.getInstance().day, TimeManager.getInstance().timeOfDay);
            }
            
            foreach (APlayableEntity settler in WorldManager.getInstance().PlayerFaction.units.OfType<APlayableEntity>().Where(x => x.isAlive())) {
                if (!SettingsManager.BoolSettings[(int)Preferences.Hunger]) {
                    settler.hunger = 0;
                }
            }

            if (SettingsManager.BoolSettings[(int)Preferences.UnlimitedResources]) {
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

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnEntityDeath(EventEntityDeath evt)
        {
            if (SettingsManager.BoolSettings[(int)Preferences.Invincible] && ReferenceEquals(evt.getUnit().faction, WorldManager.getInstance().PlayerFaction)) {
                evt.result = Result.Deny;
                evt.getUnit().hitpoints = evt.getUnit().maxHP;
            }
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnInvasionNormal(EventInvasion evt)
        {
            if (SettingsManager.BoolSettings[(int)Preferences.NoInvasions]) {
                evt.result = Result.Deny;
            }
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Monitor)]
        public void OnInvasionMonitor(EventInvasion evt)
        {
            if (evt.result == Result.Deny) {
                GUIManager.getInstance().AddTextLine("A " + evt.invasion.getName() + " invasion has been cancelled");
                return;
            }

            if (SettingsManager.BoolSettings[(int)Preferences.InvasionsInfo]) {
                GUIManager.getInstance().AddTextLine("A " + evt.invasion.getName() + " invasion of " + evt.invasion.getUnits().Count + " units has spawned.");
            }
        }
    }
}
