using System;
using System.Linq;
using UnityEngine;
using System.Timers;
using Timber_and_Stone;

namespace Plugin.Bobisback.CombinedMods
{

    public class GUIWindowCheatMenu : MonoBehaviour
    {
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(370, 200, 260, 237);
        private static readonly int WindowId = 503;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "Cheat Menu";

        private static readonly Timer UpdateTimer = new Timer(500);
        private int[] storageIndexCounts;
        private bool add999Resources;
        private bool resetResouces;
        private bool unlimitedResources;

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
            if (guiMgr.DrawButton(buttonRect, "Spawn Enemy")) {

                GUIWindowModOptions.DisplayMessage("Disabled", "Feature Disabled For right now. Full invasion menu in the works. Maybe....");
                //int ranNum = UnityEngine.Random.Range(1, 6);
                //string enemyToSpawn = "";
                //switch (ranNum) {
                //    case 1: enemyToSpawn = "goblin";
                //        break;
                //    case 2: enemyToSpawn = "mountedGoblin";
                //        break;
                //    case 3: enemyToSpawn = "skeleton";
                //        break;
                //    case 4: enemyToSpawn = "wolf";
                //        break;
                //    case 5: enemyToSpawn = "spider";
                //        break;
                //    case 6: enemyToSpawn = "necromancer";
                //        break;
                //    case 7: enemyToSpawn = "spiderLord";
                //        break;
                //}

                //TODO UnitManager.getInstance().AddEnemy(enemyToSpawn, getRandomPosition(), false, -1);
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
            guiMgr.DrawCheckBox(buttonRect, "No Hunger", ref SettingsManager.BoolSettings[(int)Preferences.Hunger]);

            //buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            //guiMgr.DrawCheckBox(buttonRect, "Fatigue", ref SettingsManager.BoolSettings[(int)Preferences.Fatigue]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Unlimited Resources", ref unlimitedResources);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Invincible", ref SettingsManager.BoolSettings[(int)Preferences.Invincible]);

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
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
            if (!SettingsManager.BoolSettings[(int) Preferences.ToggleCheatMenu]) return;

            foreach (APlayableEntity settler in WorldManager.getInstance().PlayerFaction.units.OfType<APlayableEntity>().Where(x => x.isAlive())) {
                if (!SettingsManager.BoolSettings[(int)Preferences.Hunger]) {
                    settler.hunger = 0;
                }
                //if (!SettingsManager.BoolSettings[(int)Preferences.Fatigue]) {
                //    settler.fatigue = 1;
                //}
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
            //else
            //{
            //    resetResouces = true;
            //}

            //if (resetResouces)
            //{
            //    resetResouces = false;

            //    IStorageController storage = WorldManager.getInstance().PlayerFaction.storage as IStorageController;
            //    ResourceManager resourceManager = ResourceManager.getInstance();

            //    if (storage == null) {
            //        GUIManager.getInstance().AddTextLine("Storage failed");
            //        return;
            //    }

            //    storage.recalculateStorageUsed();

            //    foreach (Resource resource in resourceManager.resources) {

            //        if (resource == null) {
            //            continue;
            //        }

            //        var storageCap = storage.getStorageCap(resource.storageIndex);
            //        var totalInStorageIndex = storageIndexCounts[resource.storageIndex];

            //        if (totalInStorageIndex == 0 || resource.mass == 0) {
            //            continue;
            //        }

            //        var totalMassAvailablePerStorageIndex = (storageCap / totalInStorageIndex);

            //        var qty = (int)(totalMassAvailablePerStorageIndex / resource.mass);

            //        storage.setResource(resource, qty);
            //    }
            //}

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
