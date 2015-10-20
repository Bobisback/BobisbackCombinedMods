using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class GUIDoorHitPointsMenu : MonoBehaviour, IEventListener
    {
        private const float ButtonHeight = 32;
        private const float LeftRightMargin = 15;
        private const float TopBottomMargin = 7.5f;
        private const float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(0, 40, 450, 237);
        private const int WindowId = 507;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private const string GUIName = "Door Hitpoints menu";
        private List<BuildStructure> theDoors = new List<BuildStructure>();

        private static readonly Timer UpdateTimer = new Timer(500);

        public const float DefaultFenceHp = 50;
        public const float DefaultTimberHp = 60;
        public const float DefaultBracedHp = 170;
        public const float DefaultStuddedHp = 150;
        public const float DefaultDungeonHp = 250;
        public const float DefaultCastleHp = 900;

        private float newFenceHp;
        private float newTimberHp;
        private float newBracedHp;
        private float newStuddedHp;
        private float newDungeonHp;
        private float newCastleHp;

        private string shownFenceHp;
        private string shownTimberHp;
        private string shownBracedHp;
        private string shownStuddedHp;
        private string shownDungeonHp;
        private string shownCastleHp;

        private int amountToMutiplyDefault;
        private string shownAmountToMutiplyDefault;

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();

            amountToMutiplyDefault = 1;
            UpdateMutiplayValues();

            windowRect.x = Screen.width - 130 - windowRect.width;

            EventManager.getInstance().Register(this);
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update()
        {
            if (Input.GetKeyDown(SettingsManager.HotKeys["ToggleDoorHitpointsMenuHotKey"])) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleDoorHitpointsMenu] == false) {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleDoorHitpointsMenu] = true;
                    WorldManager.getInstance().PlayerFaction.DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleDoorHitpointsMenu] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        public void OnGUI()
        {
            if (SettingsManager.BoolSettings[(int)Preferences.ToggleDoorHitpointsMenu]) {
                windowRect = GUI.Window(WindowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
            }
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.BoolSettings[(int)Preferences.ShowHealthBars])
                {
                    foreach (BuildStructure structure in theDoors) {
                        DrawHpBarAboveStructure(structure);
                    }   
                }
            }

            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }

        private void BuildOptionsMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, GUIName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                SettingsManager.BoolSettings[(int)Preferences.ToggleDoorHitpointsMenu] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight + (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Show Door Hp", ref SettingsManager.BoolSettings[(int)Preferences.ShowHealthBars]);

            buttonRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Show Door Info", ref SettingsManager.BoolSettings[(int)Preferences.ShowDoorInfo]);

            //Mutiply default textfield
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Mutiply Door Hp By: ");
            buttonRect.x += 275;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (amountToMutiplyDefault >= 11)
                    {
                        amountToMutiplyDefault -= 10;
                    }
                } 
                else
                {
                    if (amountToMutiplyDefault >= 2)
                    {
                        amountToMutiplyDefault--;
                    }
                }
                UpdateMutiplayValues();
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownAmountToMutiplyDefault = "" + amountToMutiplyDefault;
            guiMgr.DrawTextCenteredWhite(buttonRect,shownAmountToMutiplyDefault);
            int.TryParse(shownAmountToMutiplyDefault, out amountToMutiplyDefault);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    amountToMutiplyDefault += 10;
                } else
                {
                    amountToMutiplyDefault++;
                }
                UpdateMutiplayValues();
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "\t\t\t\tDefault Hp\tCurrent Hp\tNew Hp");

            BuildTextField("Fence Gate\t\t  ", DefaultFenceHp, ref buttonAboveHeight, ref newFenceHp, ref shownFenceHp, ref SettingsManager.CurrentFenceHp);
            BuildTextField("Timber Door\t\t  ", DefaultTimberHp, ref buttonAboveHeight, ref newTimberHp, ref shownTimberHp, ref SettingsManager.CurrentTimberHp);
            BuildTextField("Braced Door\t\t", DefaultBracedHp, ref buttonAboveHeight, ref newBracedHp, ref shownBracedHp, ref SettingsManager.CurrentBracedHp);
            BuildTextField("Studded Door\t", DefaultStuddedHp, ref buttonAboveHeight, ref newStuddedHp, ref shownStuddedHp, ref SettingsManager.CurrentStuddedHp);
            BuildTextField("Dungeon Door\t", DefaultDungeonHp, ref buttonAboveHeight, ref newDungeonHp, ref shownDungeonHp, ref SettingsManager.CurrentDungeonHp);
            BuildTextField("Castle Gate\t\t", DefaultCastleHp, ref buttonAboveHeight, ref newCastleHp, ref shownCastleHp, ref SettingsManager.CurrentCastleHp);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Reset All to Defaults And Apply")) {
                SetCurrentHpToDefault();
                UpdateHpOnAllDoors();
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Reset All to Recommended And Apply")) {
                SetCurrentHpToRecommended();
                UpdateHpOnAllDoors();
            }

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Apply New Settings to All Doors"))
            {
                SetCurrentHpToNew();
                UpdateHpOnAllDoors();
            }

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void BuildTextField(string textToShow, float defaultHp, ref float buttonAboveHeight, ref float newHp, ref string shownHp, ref float currentHp) {
            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, textToShow + defaultHp + "\t" + currentHp);
            buttonRect.x += 275;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (newHp >= 11) {
                        newHp -= 10;
                    }
                } else {
                    if (newHp >= 2) {
                        newHp--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownHp = "" + newHp;
            shownHp = guiMgr.DrawTextFieldCenteredWhite("textField", buttonRect, shownHp, 6);
            float.TryParse(shownHp, out newHp);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    newHp += 10;
                } else {
                    newHp++;
                }
            }
        }

        private void DrawHpBarAboveStructure(BuildStructure structure) {
            if (!structure.isBuilt) return;

            float hpPrecent = 0f;

            switch (structure.structureName) {
                case "Fence Gate":
                    hpPrecent = structure.health / SettingsManager.CurrentFenceHp;
                    break;
                case "Timber Door":
                    hpPrecent = structure.health / SettingsManager.CurrentTimberHp;
                    break;
                case "Braced Door":
                    hpPrecent = structure.health / SettingsManager.CurrentBracedHp;
                    break;
                case "Studded Door":
                    hpPrecent = structure.health / SettingsManager.CurrentStuddedHp;
                    break;
                case "Dungeon Door":
                    hpPrecent = structure.health / SettingsManager.CurrentDungeonHp;
                    break;
                case "Castle Arch Gate":
                case "Castle Gate":
                    hpPrecent = structure.health / SettingsManager.CurrentCastleHp;
                    break;
            }

            if (hpPrecent != 1f) {
                Vector3 vector = Camera.main.WorldToViewportPoint(structure.transform.position + new Vector3(0f, 0.6f, 0f));
                guiMgr.DrawProgressBar(new Rect(Screen.width * vector.x - 40f, Screen.height - Screen.height * vector.y, 80f, 16f), hpPrecent, true);
            }
        }

        private void UpdateHpOnAllDoors() {
            foreach (BuildStructure structure in WorldManager.getInstance().PlayerFaction.structures) {
                if (structure.structureName.Contains("Gate") || structure.structureName.Contains("Door")) {
                    UpdateDoorHp(structure);
                }
            }
        }

        private void UpdateDoorHp(BuildStructure structure) {
            switch (structure.structureName) {
                case "Fence Gate":
                    structure.health = SettingsManager.CurrentFenceHp;
                    break;
                case "Timber Door":
                    structure.health = SettingsManager.CurrentTimberHp;
                    break;
                case "Braced Door":
                    structure.health = SettingsManager.CurrentBracedHp;
                    break;
                case "Studded Door":
                    structure.health = SettingsManager.CurrentStuddedHp;
                    break;
                case "Dungeon Door":
                    structure.health = SettingsManager.CurrentDungeonHp;
                    break;
                case "Castle Arch Gate":
                case "Castle Gate":
                    structure.health = SettingsManager.CurrentCastleHp;
                    break;
            }

            if (SettingsManager.BoolSettings[(int)Preferences.ShowDoorInfo]) {
                GUIManager.getInstance().AddTextLine(structure.structureName + " has " + structure.health + " health.");
            }
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            if (SettingsManager.BoolSettings[(int)Preferences.ShowHealthBars])
            {
                theDoors.Clear();
                foreach (BuildStructure structure in WorldManager.getInstance().PlayerFaction.structures) {
                    if (structure.structureName.Contains("Gate") || structure.structureName.Contains("Door"))
                    {
                        if (!theDoors.Contains(structure))
                        {
                            theDoors.Add(structure);
                        }
                    }
                    else
                    {
                        theDoors.Remove(structure);
                    }
                }
            }
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnBuildStructure(EventBuildStructure evt)
        {
            if (evt.structure.structureName.Contains("Gate") || evt.structure.structureName.Contains("Door")) {
                UpdateDoorHp(evt.structure);
            }
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnGameLoad(EventGameLoad evt)
        {
            UpdateDoorHpOnGameLoad();
        }

        private void UpdateDoorHpOnGameLoad() {
            foreach (BuildStructure structure in WorldManager.getInstance().PlayerFaction.structures) {
                if (structure.structureName.Contains("Gate") || structure.structureName.Contains("Door")) {
                    switch (structure.structureName) {
                        case "Fence Gate":
                            if (structure.health == DefaultFenceHp) {
                                structure.health = SettingsManager.CurrentFenceHp;
                            }
                            break;
                        case "Timber Door":
                            if (structure.health == DefaultTimberHp) {
                                structure.health = SettingsManager.CurrentTimberHp;
                            }
                            break;
                        case "Braced Door":
                            if (structure.health == DefaultBracedHp) {
                                structure.health = SettingsManager.CurrentBracedHp;
                            }
                            break;
                        case "Studded Door":
                            if (structure.health == DefaultStuddedHp) {
                                structure.health = SettingsManager.CurrentStuddedHp;
                            }
                            break;
                        case "Dungeon Door":
                            if (structure.health == DefaultDungeonHp) {
                                structure.health = SettingsManager.CurrentDungeonHp;
                            }
                            break;
                        case "Castle Arch Gate":
                        case "Castle Gate":
                            if (structure.health == DefaultCastleHp) {
                                structure.health = SettingsManager.CurrentCastleHp;
                            }
                            break;
                    }

                    if (SettingsManager.BoolSettings[(int)Preferences.ShowDoorInfo]) {
                        GUIManager.getInstance().AddTextLine(structure.structureName + " has " + structure.health + " health.");
                    }
                }
            }
        }

        private void SetCurrentHpToDefault() {
            SettingsManager.CurrentFenceHp = DefaultFenceHp;
            SettingsManager.CurrentTimberHp = DefaultTimberHp;
            SettingsManager.CurrentBracedHp = DefaultBracedHp;
            SettingsManager.CurrentStuddedHp = DefaultStuddedHp;
            SettingsManager.CurrentDungeonHp = DefaultDungeonHp;
            SettingsManager.CurrentCastleHp = DefaultCastleHp;
        }

        private void SetCurrentHpToRecommended() {
            SettingsManager.CurrentFenceHp = 150;
            SettingsManager.CurrentTimberHp = 250;
            SettingsManager.CurrentBracedHp = 450;
            SettingsManager.CurrentStuddedHp = 600;
            SettingsManager.CurrentDungeonHp = 1000;
            SettingsManager.CurrentCastleHp = 2500;
        }

        private void SetCurrentHpToNew() {
            SettingsManager.CurrentFenceHp = newFenceHp;
            SettingsManager.CurrentTimberHp = newTimberHp;
            SettingsManager.CurrentBracedHp = newBracedHp;
            SettingsManager.CurrentStuddedHp = newStuddedHp;
            SettingsManager.CurrentDungeonHp = newDungeonHp;
            SettingsManager.CurrentCastleHp = newCastleHp;
        }

        private void UpdateMutiplayValues() {
            newBracedHp = DefaultBracedHp * amountToMutiplyDefault;
            newCastleHp = DefaultCastleHp * amountToMutiplyDefault;
            newDungeonHp = DefaultDungeonHp * amountToMutiplyDefault;
            newFenceHp = DefaultFenceHp * amountToMutiplyDefault;
            newStuddedHp = DefaultStuddedHp * amountToMutiplyDefault;
            newTimberHp = DefaultTimberHp * amountToMutiplyDefault;
        }
    }
}
