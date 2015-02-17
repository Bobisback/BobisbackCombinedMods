using System;
using System.Collections.Generic;
using Timber_and_Stone;
using Timber_and_Stone.API;
using Timber_and_Stone.Event;
using Timber_and_Stone.API.Event;
using EventHandler = Timber_and_Stone.API.Event.EventHandler;
using UnityEngine;

namespace Plugin.Trololo.ModTest {
    public class tl {
        public static List<string> tL;
        static tl() {
            tL = new List<string>();
            tL.Add("trait.hardworker");
            tL.Add("trait.lazy");
            tL.Add("trait.badvision");
            tL.Add("trait.goodvision");
            tL.Add("trait.courageous");
            tL.Add("trait.cowardly");
            tL.Add("trait.strongback");
            tL.Add("trait.weakback");
            tL.Add("trait.athletic");
            tL.Add("trait.sluggish");
            tL.Add("trait.charismatic");
            tL.Add("trait.disloyal");
            tL.Add("trait.overeater");
            tL.Add("trait.clumsy");
            tL.Add("trait.quicklearner");
        }
    }

    public class gl {
        public static List<string> gL;
        static gl() {
            gL = new List<string>();
            gL.Add("trait.hardworker");
            gL.Add("trait.goodvision");
            gL.Add("trait.courageous");
            gL.Add("trait.strongback");
            gL.Add("trait.athletic");
            gL.Add("trait.charismatic");
            gL.Add("trait.quicklearner");
        }
    }

    public class GW : MonoBehaviour {
        public bool isInitialized;
        private Rect wR = new Rect(20, 90, 220, 300);
        private float iww = 400f;
        public Rect wVR;
        private GUIManager gM;
        private UnitManager uM;
        private Vector2 hSP;
        private bool sT = false;
        private float val;
        private int WIN_ID = 101;

        public static bool dayLight = false;
        public static bool noSleep = false;
        public static bool noHunger = false;
        public static bool unlimitedResources = false;
        public static bool noEnemies = false;
        public static bool autoFixTraits = false;
        public static bool showOptionsDebug = false;
        public static bool clearEnemyCorpse = false;
        public static bool totalEnemiesCount = false;

        public int hourLastChecked = 0;
        public int tempHour = 0;
        public int hoursInDay = 0;

        public bool runOnce = false;

        private GW() {
            isInitialized = true;
            InitializeSettings();
            LoadPlayerSettings();
        }

        public void Start() {
            gM = AManager<GUIManager>.getInstance();
            this.hSP = Vector2.zero;
            this.uM = AManager<UnitManager>.getInstance();
            val = 430f;
            this.wR = new Rect(100f, 100f, this.iww, this.val);
            val = 385f;
            this.wVR = new Rect(100f, 100f, this.iww, this.val);
        }

        public void Update() {
            if (runOnce != true && ((int)Time.timeScale != 1)) {
                if (dayLight == true)
                    base.StartCoroutine("UpdateDayTime");
                if (noSleep == true)
                    base.StartCoroutine("CreateInsomniacs");
                if (noHunger == true)
                    base.StartCoroutine("FixHungerPains");
                if (autoFixTraits == true)
                    base.StartCoroutine("FixTraits");
                if (clearEnemyCorpse == true)
                    base.StartCoroutine("ClearEnemyCorpses");
                if (unlimitedResources == true)
                    base.StartCoroutine("LotsOfResources");
                runOnce = true;
            }


            if (this.sT && AManager<Options>.getInstance().GetCancelKey()) {
                this.sT = false;
            }

            if (this.sT && AManager<Options>.getInstance().GetMenuKey()) {
                this.sT = false;
            }

            if (Input.GetKeyDown(KeyCode.H)) {
                if (sT == false) {
                    sT = true;
                    AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else
                    sT = false;
            }

            if (hourLastChecked == 0) hourLastChecked = AManager<TimeManager>.getInstance().hour;

            tempHour = AManager<TimeManager>.getInstance().hour;
            if (hourLastChecked != tempHour) {
                if (dayLight == true) {
                    hoursInDay++;
                    if (hoursInDay >= 24) {
                        hoursInDay = 0;
                        AManager<TimeManager>.getInstance().day = AManager<TimeManager>.getInstance().day + 1;
                    }
                }
                hourLastChecked = tempHour;
                if (dayLight == true)
                    base.StartCoroutine("UpdateDayTime");
                if (noSleep == true)
                    base.StartCoroutine("CreateInsomniacs");
                if (noHunger == true)
                    base.StartCoroutine("FixHungerPains");
                if (autoFixTraits == true)
                    base.StartCoroutine("FixTraits");
                if (clearEnemyCorpse == true)
                    base.StartCoroutine("ClearEnemyCorpses");
                if (unlimitedResources == true)
                    base.StartCoroutine("LotsOfResources");
            }
        }

        public int CountTotalEnemies() {
            int num = 0;
            for (int i = 0; i < UnitManager.getInstance().enemyUnits.Count; i++) {
                if (UnitManager.getInstance().enemyUnits[i].tag == "Skeleton" ||
                    UnitManager.getInstance().enemyUnits[i].tag == "Goblin" ||
                    UnitManager.getInstance().enemyUnits[i].tag == "Wolf" ||
                    UnitManager.getInstance().enemyUnits[i].tag == "Spider" ||
                    UnitManager.getInstance().enemyUnits[i].tag == "Necromancer") {
                    if (UnitManager.getInstance().enemyUnits[i].GetComponent<Enemy>().action != "dead") {
                        num++;
                    }
                }
            }
            return num;
        }

        public void UpdateDayTime() {
            AManager<TimeManager>.getInstance().hour = 12;
            AManager<TimeManager>.getInstance().time = 12f;
            AManager<TimeManager>.getInstance().timeOfDay = "Eternal Light";
            AManager<GUIManager>.getInstance().UpdateTime(AManager<TimeManager>.getInstance().day, AManager<TimeManager>.getInstance().timeOfDay);
        }

        public void OnGUI() {
            if (!this.gM.inGame) {
                return;
            }

            if (showOptionsDebug == true) {
                if (dayLight == true)
                    gM.DrawTextLeftWhite(new Rect(10f, (float)(44 + 27), 300f, 24f), "Eternal Light: ON");
                if (unlimitedResources == true)
                    gM.DrawTextLeftWhite(new Rect(10f, (float)(44 + 27 + 27), 300f, 24f), "Unlimited Resources: ON");
                if (noHunger == true)
                    gM.DrawTextLeftWhite(new Rect(10f, (float)(44 + 27 + 27 + 27), 300f, 24f), "No Hunger: ON");
                if (noSleep == true)
                    gM.DrawTextLeftWhite(new Rect(10f, (float)(44 + 27 + 27 + 27 + 27), 300f, 24f), "No Sleep: ON");
                if (autoFixTraits == true)
                    gM.DrawTextLeftWhite(new Rect(10f, (float)(44 + 27 + 27 + 27 + 27 + 27), 300f, 24f), "Traits: ON");
                if (noEnemies == true)
                    gM.DrawTextLeftWhite(new Rect(10f, (float)(44 + 27 + 27 + 27 + 27 + 27 + 27), 300f, 24f), "No Enemies: ON");
                if (clearEnemyCorpse == true)
                    gM.DrawTextLeftWhite(new Rect(10f, (float)(44 + 27 + 27 + 27 + 27 + 27 + 27 + 27), 300f, 24f), "Remove Corpses: ON");
                if (totalEnemiesCount == true)
                    gM.DrawTextLeftWhite(new Rect(10f, (float)(44 + 27 + 27 + 27 + 27 + 27 + 27 + 27 + 27), 300f, 24f), "Total Enemies: " + CountTotalEnemies().ToString());
            }

            if (!this.sT) {
                return;
            }

            this.wR.width = Mathf.Min(this.iww, (float)(Screen.width - 4));
            this.wR = GUI.Window(WIN_ID, this.wR, new GUI.WindowFunction(this.RenderWindow), string.Empty, this.gM.hiddenButtonStyle);
            GUI.FocusWindow(WIN_ID);
            this.wR.x = Mathf.Clamp(this.wR.x, 2f, (float)Screen.width - this.wR.width - 2f);
            this.wR.y = Mathf.Clamp(this.wR.y, 40f, (float)Screen.height - this.wR.height - 2f);

            GUI.skin = gM.skin;
        }

        public void LoadPlayerSettings() {
            dayLight = Convert.ToBoolean(PlayerPrefs.GetInt("hoiz_daytime_only"));
            noSleep = Convert.ToBoolean(PlayerPrefs.GetInt("hoiz_no_sleep"));
            noHunger = Convert.ToBoolean(PlayerPrefs.GetInt("hoiz_no_hunger"));
            noEnemies = Convert.ToBoolean(PlayerPrefs.GetInt("hoiz_no_enemies"));
            autoFixTraits = Convert.ToBoolean(PlayerPrefs.GetInt("hoiz_auto_fix_traits"));
            unlimitedResources = Convert.ToBoolean(PlayerPrefs.GetInt("hoiz_lots_resources"));
            showOptionsDebug = Convert.ToBoolean(PlayerPrefs.GetInt("show_options_debug"));
            clearEnemyCorpse = Convert.ToBoolean(PlayerPrefs.GetInt("hoiz_auto_remove_corpses"));
        }

        public void InitializeSettings() {
            bool tmp = false;
            if (tmp = PlayerPrefs.GetInt("hoiz_daytime_only", -1) == -1) {
                dayLight = false;
                SaveSetting("hoiz_daytime_only", false);
            } else dayLight = tmp;
            if (tmp = tmp = PlayerPrefs.GetInt("hoiz_no_sleep", -1) == -1) {
                noSleep = false;
                SaveSetting("hoiz_no_sleep", false);
            } else noSleep = tmp;
            if (tmp = PlayerPrefs.GetInt("hoiz_no_hunger", -1) == -1) {
                noHunger = false;
                SaveSetting("hoiz_no_hunger", false);
            } else noHunger = tmp;
            if (tmp = PlayerPrefs.GetInt("hoiz_no_enemies", -1) == -1) {
                noEnemies = false;
                SaveSetting("hoiz_no_enemies", false);
            } else noEnemies = tmp;
            if (tmp = PlayerPrefs.GetInt("hoiz_auto_fix_traits", -1) == -1) {
                autoFixTraits = false;
                SaveSetting("hoiz_auto_fix_traits", false);
            } else autoFixTraits = tmp;
            if (tmp = PlayerPrefs.GetInt("hoiz_lots_resources", -1) == -1) {
                unlimitedResources = false;
                SaveSetting("hoiz_lots_resources", false);
            } else unlimitedResources = tmp;
            if (tmp = PlayerPrefs.GetInt("show_options_debug", -1) == -1) {
                showOptionsDebug = false;
                SaveSetting("show_options_debug", false);
            } else showOptionsDebug = tmp;
            if (tmp = PlayerPrefs.GetInt("hoiz_auto_remove_corpses", -1) == -1) {
                clearEnemyCorpse = false;
                SaveSetting("hoiz_auto_remove_corpses", false);
            } else clearEnemyCorpse = tmp;

            PlayerPrefs.Save();
        }

        public void SaveSetting(string mySetting, bool myVal) {
            if (myVal == true)
                PlayerPrefs.SetInt(mySetting, 1);
            else
                PlayerPrefs.SetInt(mySetting, 0);
        }

        public void CreateInsomniacs() {
            for (int i = 0; i < UnitManager.getInstance().playerUnits.Count; i++) {
                if (UnitManager.getInstance().playerUnits[i].isAlive()) {
                    UnitManager.getInstance().playerUnits[i].fatigue = 1f;
                }
            }
        }

        public void FixHungerPains() {
            for (int i = 0; i < UnitManager.getInstance().playerUnits.Count; i++) {
                if (UnitManager.getInstance().playerUnits[i].isAlive()) {
                    UnitManager.getInstance().playerUnits[i].hunger = 0f;
                }
            }
        }

        public void LotsOfResources() {
            GUIManager.getInstance().AddTextLine("Not functional yet.");
        }

        // Just blowing away all traits and reassigning.  Not keeping track of who has what traits and when.
        public void FixTraits() {
            for (int i = 0; i < UnitManager.getInstance().playerUnits.Count; i++) {
                if (UnitManager.getInstance().playerUnits[i].isAlive()) {
                    for (int j = 0; j < tl.tL.Count; j++) {
                        if (UnitManager.getInstance().playerUnits[i].preferences[tl.tL[j]] == true) {
                            UnitManager.getInstance().playerUnits[i].preferences[tl.tL[j]] = false;
                        }
                    }
                }
            }
            for (int i = 0; i < UnitManager.getInstance().playerUnits.Count; i++) {
                if (UnitManager.getInstance().playerUnits[i].isAlive()) {
                    UnitManager.getInstance().playerUnits[i].preferences[tl.tL[0]] = true;
                    UnitManager.getInstance().playerUnits[i].preferences[tl.tL[3]] = true;
                    UnitManager.getInstance().playerUnits[i].preferences[tl.tL[4]] = true;
                    UnitManager.getInstance().playerUnits[i].preferences[tl.tL[6]] = true;
                    UnitManager.getInstance().playerUnits[i].preferences[tl.tL[8]] = true;
                    UnitManager.getInstance().playerUnits[i].preferences[tl.tL[10]] = true;
                    UnitManager.getInstance().playerUnits[i].preferences[tl.tL[14]] = true;
                }
            }
        }

        public void AddAnimals() {
            if (AManager<UnitManager>.getInstance().wildFaunaUnits.Count >= 500 && AManager<UnitManager>.getInstance().wildFaunaUnits.Count <= 9999)
                GUIManager.getInstance().AddTextLine("Adding a few random animals...dont go overboard...");
            if (AManager<UnitManager>.getInstance().wildFaunaUnits.Count <= 9999) {
                for (int i = 0; i <= 25; i++) {
                    AManager<UnitManager>.getInstance().AddAnimal("random", Vector3.zero, false);
                }
                GUIManager.getInstance().AddTextLine("Total Animals : " + AManager<UnitManager>.getInstance().wildFaunaUnits.Count.ToString());
            }
        }

        public void SpawnMigrant() {
            Vector3 angle = UnitManager.getInstance().transform.eulerAngles;

            float edgePosition = AManager<WorldManager>.getInstance().GetEdgePosition();
            Vector3 spawnLocation = new Vector3(edgePosition * 0 + 1 * (0.1f + (float)UnityEngine.Random.Range(-256, 257) * 0.1f), 0f, edgePosition * 1 + 0 * (0.1f + (float)UnityEngine.Random.Range(-256, 257) * 0.1f));
            Vector3[] locationArray = AManager<ChunkManager>.getInstance().Pick(new Vector3(spawnLocation.x, AManager<WorldManager>.getInstance().topHeight, spawnLocation.z), Vector3.down, false);
            int blockID = (int)AManager<ChunkManager>.getInstance().GetBlock(Coordinate.FromChunkBlock(locationArray[0], locationArray[1])).properties.getID();
            if (blockID == 80 || blockID == 98) {
                spawnLocation = new Vector3(-spawnLocation.x, spawnLocation.y, -spawnLocation.z);
                locationArray = AManager<ChunkManager>.getInstance().Pick(new Vector3(spawnLocation.x, AManager<WorldManager>.getInstance().topHeight, spawnLocation.z), Vector3.down, false);
                blockID = (int)AManager<ChunkManager>.getInstance().GetBlock(Coordinate.FromChunkBlock(locationArray[0], locationArray[1])).properties.getID();
            }
            if (blockID != 80 && blockID != 98) {
                Transform transform = null;
                switch (UnityEngine.Random.Range(1, 13)) {
                    case 1:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("builder", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 2:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("miner", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 3:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("wood chopper", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 4:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("farmer", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 5:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("forager", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 6:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("archer", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 7:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("blacksmith", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 8:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("infantry", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 9:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("carpenter", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 10:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("stone mason", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 11:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("tailor", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 12:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("herder", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    case 13:
                        transform = AManager<UnitManager>.getInstance().AddHumanUnit("trader", spawnLocation + Vector3.zero, true, false, UnityEngine.Random.Range(0, 3) == 1);
                        break;
                    default:
                        break;
                }

                if (autoFixTraits == true) {
                    transform.GetComponent<APlayableEntity>().preferences["trait.hardworker"] = true;
                    transform.GetComponent<APlayableEntity>().preferences["trait.goodvision"] = true;
                    transform.GetComponent<APlayableEntity>().preferences["trait.charismatic"] = true;
                    transform.GetComponent<APlayableEntity>().preferences["trait.courageous"] = true;
                    transform.GetComponent<APlayableEntity>().preferences["trait.athletic"] = true;
                    transform.GetComponent<APlayableEntity>().preferences["trait.quicklearner"] = true;
                    transform.GetComponent<APlayableEntity>().preferences["trait.strongback"] = true;
                    transform.GetComponent<APlayableEntity>().preferences["trait.hardworker"] = true;
                    transform.GetComponent<APlayableEntity>().preferences["trait.weakback"] = false;
                    transform.GetComponent<APlayableEntity>().preferences["trait.cowardly"] = false;
                    transform.GetComponent<APlayableEntity>().preferences["trait.clumsy"] = false;
                    transform.GetComponent<APlayableEntity>().preferences["trait.sluggish"] = false;
                    transform.GetComponent<APlayableEntity>().preferences["trait.overeater"] = false;
                    transform.GetComponent<APlayableEntity>().preferences["trait.disloyal"] = false;
                    transform.GetComponent<APlayableEntity>().preferences["trait.badvision"] = false;
                    transform.GetComponent<APlayableEntity>().preferences["trait.lazy"] = false;
                    transform.GetComponent<APlayableEntity>().preferences["preference.waitinhallwhileidle"] = true;
                }

                UnitManager.getInstance().AddMigrantResources(transform.GetComponent<APlayableEntity>());

                GUIManager.getInstance().AddTextLine(transform.GetComponent<APlayableEntity>().unitName + " is on their way to your Hall.");
            }
        }

        public void SpawnMerchant() {
            // You need roads and a hall for merchant to show up.  I'm Not checking for hall.
            if (AManager<DesignManager>.getInstance().edgeRoads.Count < 1) {
                GUIManager.getInstance().AddTextLine("Build at least one road piece on edge of map. And a hall. Connect them.");
                GUIManager.getInstance().AddTextLine("Total Road Pieces = " + AManager<DesignManager>.getInstance().edgeRoads.Count.ToString());
                return;
            }

            for (int i = 0; i < 10; i++) {
                if (AManager<UnitManager>.getInstance().SpawnMerchant(Vector3.zero) != null)
                    break;
            }
        }

        public void SetDisableEnemies() {
            if (noEnemies == false)
                noEnemies = true;
            else
                noEnemies = false;

            SaveSetting("hoiz_no_enemies", noEnemies);
        }

        public void SetNoHunger() {
            if (noHunger == false)
                noHunger = true;
            else
                noHunger = false;

            SaveSetting("hoiz_no_hunger", noHunger);

            if (noHunger)
                FixHungerPains();
        }

        public void SetNoSleep() {
            if (noSleep == false)
                noSleep = true;
            else
                noSleep = false;

            SaveSetting("hoiz_no_sleep", noSleep);

            if (noSleep)
                CreateInsomniacs();
        }

        public void ClearEnemyCorpses() {
            for (int l = 0; l <= AManager<UnitManager>.getInstance().enemyUnits.Count; l++) {
                if (AManager<UnitManager>.getInstance().enemyUnits[l].GetComponent<Enemy>().action == "dead") {
                    AManager<UnitManager>.getInstance().RemoveEnemy(AManager<UnitManager>.getInstance().enemyUnits[l].transform);
                }
            }
        }

        public void SetClearEnemyCorpses() {
            if (clearEnemyCorpse == false)
                clearEnemyCorpse = true;
            else
                clearEnemyCorpse = false;

            SaveSetting("hoiz_auto_remove_corpses", clearEnemyCorpse);

            if (clearEnemyCorpse)
                ClearEnemyCorpses();
        }

        public void SetUnlimitedResources() {
            unlimitedResources = false;
            return;
        }

        public void SetDayLight() {
            if (dayLight == false)
                dayLight = true;
            else
                dayLight = false;

            SaveSetting("hoiz_daytime_only", dayLight);

            if (dayLight)
                UpdateDayTime();
        }

        public void SetAutoFixTraits() {
            if (autoFixTraits == false)
                autoFixTraits = true;
            else
                autoFixTraits = false;

            SaveSetting("hoiz_auto_fix_traits", autoFixTraits);

            if (autoFixTraits)
                FixTraits();
        }

        public void KillAllEnemies() {
            for (int i = 0; i < UnitManager.getInstance().enemyUnits.Count; i++) {

                UnitManager.getInstance().enemyUnits[i].GetComponent<Enemy>().health = 0f;
                UnitManager.getInstance().enemyUnits[i].GetComponent<Enemy>().EvaluateHealth(false);
            }
        }

        public static void FixMigrant(APlayableEntity unit) {
            unit.preferences["trait.hardworker"] = true;
            unit.preferences["trait.goodvision"] = true;
            unit.preferences["trait.charismatic"] = true;
            unit.preferences["trait.courageous"] = true;
            unit.preferences["trait.athletic"] = true;
            unit.preferences["trait.quicklearner"] = true;
            unit.preferences["trait.strongback"] = true;
            unit.preferences["trait.hardworker"] = true;
            unit.preferences["trait.weakback"] = false;
            unit.preferences["trait.cowardly"] = false;
            unit.preferences["trait.clumsy"] = false;
            unit.preferences["trait.sluggish"] = false;
            unit.preferences["trait.overeater"] = false;
            unit.preferences["trait.disloyal"] = false;
            unit.preferences["trait.badvision"] = false;
            unit.preferences["trait.lazy"] = false;
        }

        public void RenderWindow(int windowID) {
            float num = 25f;
            float num2 = 9f;
            Rect location = new Rect(0f, 0f, this.wR.width, this.wR.height);
            if (location.Contains(Event.current.mousePosition)) {
                this.sT = true;
                this.gM.mouseInGUI = true;
            }
            this.gM.DrawWindow(location, "Hoizon v1.0", false);
            if (GUI.Button(new Rect(location.xMax - 24f, location.yMin + 4f, 20f, 20f), string.Empty, this.gM.closeWindowButtonStyle)) {
                this.sT = false;
                return;
            }
            GUI.DragWindow(new Rect(0f, 0f, this.wR.width, 24f));
            if (this.wR.width < this.iww) {
                Rect position = new Rect(this.wR.x, this.wR.y - 25f, this.wR.width - num2, this.wR.height - num);
                this.hSP = GUI.BeginScrollView(position, this.hSP, this.wVR, true, false);
            }

            GUIManager iN = AManager<GUIManager>.getInstance();
            GUI.skin = iN.skin;

            Rect myRect;

            float inc = 40f;
            string buttTypeWord = "";
            string buttName = "";
            for (int z = 0; z <= 11; z++) {
                switch (z) {
                    case 0: {
                            buttTypeWord = "Spawn Migrant:";
                            buttName = "Select";
                            break;
                        }
                    case 1: {
                            buttTypeWord = "";
                            buttName = "";
                            break;
                        }
                    case 2: {
                            buttTypeWord = "Spawn Merchant:";
                            buttName = "Select";
                            break;
                        }
                    case 3: {
                            buttTypeWord = "Spawn Animal:";
                            buttName = "Select";
                            break;
                        }
                    case 4: {
                            buttTypeWord = "No Enemies:";
                            if (noEnemies == true)
                                buttName = "Enabled";
                            else
                                buttName = "Disabled";
                            break;
                        }
                    case 5: {
                            buttTypeWord = "No Food Needed:";
                            if (noHunger == true)
                                buttName = "Enabled";
                            else
                                buttName = "Disabled";
                            break;
                        }
                    case 6: {
                            buttTypeWord = "No Sleep Needed:";
                            if (noSleep == true)
                                buttName = "Enabled";
                            else
                                buttName = "Disabled";
                            break;
                        }
                    case 7: {
                            buttTypeWord = "Unlimited Resources:";
                            if (unlimitedResources == true)
                                buttName = "Enabled";
                            else
                                buttName = "Disabled";
                            break;
                        }
                    case 8: {
                            buttTypeWord = "Always Day:";
                            if (dayLight == true)
                                buttName = "Enabled";
                            else
                                buttName = "Disabled";
                            break;
                        }
                    case 9: {
                            buttTypeWord = "Kill Enemies";
                            buttName = "Button";
                            break;
                        }
                    case 10: {
                            buttTypeWord = "Clear Corpses";
                            if (clearEnemyCorpse == true)
                                buttName = "Enabled";
                            else
                                buttName = "Disabled";
                            break;
                        }
                    case 11: {
                            buttTypeWord = "Auto Fix Traits:";
                            if (autoFixTraits == true)
                                buttName = "Enabled";
                            else
                                buttName = "Disabled";
                            break;
                        }
                    default: {
                            buttTypeWord = "Unknown";
                            break;
                        }
                }

                if (buttTypeWord != "" && buttName != "") {
                    myRect = new Rect(15f, inc, 200f, 25f);
                    AManager<GUIManager>.getInstance().DrawTextLeftWhite(myRect, buttTypeWord);
                    Rect loc = new Rect(0f, myRect.y + 22f, myRect.width + 125f, 3f);
                    this.gM.DrawLineBlack(loc);
                    if (iN.DrawButton(new Rect(250f, inc, 125f, 25f), buttName)) {
                        switch (z) {
                            case 0: {
                                    SpawnMigrant();
                                    break;
                                }
                            case 1: {
                                    break;
                                }
                            case 2: {
                                    SpawnMerchant();

                                    break;
                                }
                            case 3: {
                                    AddAnimals();
                                    break;
                                }
                            case 4: {
                                    SetDisableEnemies();
                                    break;
                                }
                            case 5: {
                                    SetNoHunger();
                                    break;
                                }
                            case 6: {
                                    SetNoSleep();
                                    break;
                                }
                            case 7: {
                                    SetUnlimitedResources();
                                    break;
                                }
                            case 8: {
                                    SetDayLight();
                                    break;
                                }
                            case 9: {
                                    KillAllEnemies();
                                    break;
                                }
                            case 10: {
                                    SetClearEnemyCorpses();
                                    break;
                                }
                            case 11: {
                                    SetAutoFixTraits();
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                    inc = inc + 27f;
                }
            }
            // Draw Options Boxes
            this.gM.DrawCheckBox(new Rect(15f, inc, 200f, 28f), "Show Info", ref showOptionsDebug);
            inc = inc + 27;
            this.gM.DrawCheckBox(new Rect(15f, inc, 300f, 28f), "Enemy Count", ref totalEnemiesCount);
            inc = inc + 27;


            //inc = inc + 27;
        }
    }

    public class PluginMain : IEventListener {
        /// <summary>
        /// This is called when your plugin is first loaded into the game. Anything that needs to happen as your mod is loaded should happen here. In most use-cases, this'll be pretty much empty.
        /// </summary>
        public void OnLoad() {

            GUIManager.getInstance().AddTextLine("Hoizon v1.0 Loaded! <press H to access>");
            GUIManager.getInstance().gameObject.AddComponent(typeof(GW));
        }

        /// <summary>
        /// This is called when Timber and Stone enables your plugin. This is where you register yourself for events, register new blocks, etc.
        /// </summary>
        public void OnEnable() {
            EventManager.getInstance().Register(this);
        }

        public void OnDisable() {

        }

        [EventHandler(Priority.Normal)]
        public void onMigrant(EventMigrant evt) {
            if (GW.autoFixTraits == true)
                GW.FixMigrant(evt.unit);
        }

        [EventHandler(Priority.Normal)]
        public void onMigrantAccept(EventMigrantAccept evt) {
            if (GW.autoFixTraits == true)
                GW.FixMigrant(evt.unit);
        }

        [EventHandler(Priority.Normal)]
        public void onEventCraft(EventCraft evt) {

        }

        [EventHandler(Priority.Normal)]
        public void onMine(EventMine evt) {

        }

        [EventHandler(Priority.Normal)]
        public void onEventGrow(EventBlockGrow evt) {

        }

        [EventHandler(Priority.Normal)]
        public void onEventBlockSet(EventBlockSet evt) {

        }

        [EventHandler(Priority.Normal)]
        public void onEventBuildStructure(EventBuildStructure evt) {

        }

        [EventHandler(Priority.Normal)]
        public void onInvasionNormal(EventInvasion evt) {
            if (GW.noEnemies) {
                evt.result = Result.Deny;
            }
        }

        [EventHandler(Priority.Monitor)]
        public void onInvasionMonitor(EventInvasion evt) {
            if (GW.noEnemies) {
                if (evt.result != Result.Deny) return;
            }
        }
    }
}
