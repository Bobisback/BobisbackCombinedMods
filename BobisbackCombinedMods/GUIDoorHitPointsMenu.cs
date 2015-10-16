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
        private Rect windowRect = new Rect(370, 200, 450, 237);
        private const int WindowId = 506;

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
            if (guiMgr.inGame && !guiMgr.gameOver) {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleDoorHitpointsMenu]) {
                    windowRect = GUI.Window(WindowId, windowRect, BuildOptionsMenu, string.Empty, guiMgr.windowBoxStyle);
                }

                if (SettingsManager.BoolSettings[(int)Preferences.ShowHealthBars])
                {
                    foreach (BuildStructure structure in theDoors) {
                        DrawHpBarAboveStructure(structure);
                    }   
                }
            }
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

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Apply Settings To New Doors", ref SettingsManager.BoolSettings[(int)Preferences.DoorHpEnabled]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight + (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Show Door Hp Bars", ref SettingsManager.BoolSettings[(int)Preferences.ShowHealthBars]);

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

            //Fence textfield
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Fence Gate\t\t" + DefaultFenceHp + "\t\t" + SettingsManager.CurrentFenceHp);
            buttonRect.x += 275;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (newFenceHp >= 11) {
                        newFenceHp -= 10;
                    }
                } else {
                    if (newFenceHp >= 2) {
                        newFenceHp--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownFenceHp = "" + newFenceHp;
            shownFenceHp = guiMgr.DrawTextFieldCenteredWhite("newFenceHp", buttonRect, shownFenceHp, 6);
            float.TryParse(shownFenceHp, out newFenceHp);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    newFenceHp += 10;
                } else {
                    newFenceHp++;
                }
            }

            //Timber Textfield
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Timber Door\t\t" + DefaultTimberHp + "\t\t" + SettingsManager.CurrentTimberHp);
            buttonRect.x += 275;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (newTimberHp >= 11) {
                        newTimberHp -= 10;
                    }
                } else {
                    if (newTimberHp >= 2) {
                        newTimberHp--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownTimberHp = "" + newTimberHp;
            shownTimberHp = guiMgr.DrawTextFieldCenteredWhite("newTimberHp", buttonRect, shownTimberHp, 6);
            float.TryParse(shownTimberHp, out newTimberHp);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    newTimberHp += 10;
                } else {
                    newTimberHp++;
                }
            }

            //Braced Textfeild
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Braced Door\t\t" + DefaultBracedHp + "\t" + SettingsManager.CurrentBracedHp);
            buttonRect.x += 275;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (newBracedHp >= 11) {
                        newBracedHp -= 10;
                    }
                } else {
                    if (newBracedHp >= 2) {
                        newBracedHp--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownBracedHp = "" + newBracedHp;
            shownBracedHp = guiMgr.DrawTextFieldCenteredWhite("newBracedHp", buttonRect, shownBracedHp, 6);
            float.TryParse(shownBracedHp, out newBracedHp);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    newBracedHp += 10;
                } else {
                    newBracedHp++;
                }
            }

            //Studded Textfield
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Studded Door\t" + DefaultStuddedHp + "\t" + SettingsManager.CurrentStuddedHp);
            buttonRect.x += 275;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (newStuddedHp >= 11) {
                        newStuddedHp -= 10;
                    }
                } else {
                    if (newStuddedHp >= 2) {
                        newStuddedHp--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownStuddedHp = "" + newStuddedHp;
            shownStuddedHp = guiMgr.DrawTextFieldCenteredWhite("newStuddedHp", buttonRect, shownStuddedHp, 6);
            float.TryParse(shownStuddedHp, out newStuddedHp);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    newStuddedHp += 10;
                } else {
                    newStuddedHp++;
                }
            }

            //Dungeon Textfeild
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Dungeon Door\t" + DefaultDungeonHp + "\t" + SettingsManager.CurrentDungeonHp);
            buttonRect.x += 275;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (newDungeonHp >= 11) {
                        newDungeonHp -= 10;
                    }
                } else {
                    if (newDungeonHp >= 2) {
                        newDungeonHp--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownDungeonHp = "" + newDungeonHp;
            shownDungeonHp = guiMgr.DrawTextFieldCenteredWhite("newDungeonHp", buttonRect, shownDungeonHp, 6);
            float.TryParse(shownDungeonHp, out newDungeonHp);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    newDungeonHp += 10;
                } else {
                    newDungeonHp++;
                }
            }

            //Castle textfield
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftWhite(buttonRect, "Castle Gates\t\t" + DefaultCastleHp + "\t" + SettingsManager.CurrentCastleHp);
            buttonRect.x += 275;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (newCastleHp >= 11) {
                        newCastleHp -= 10;
                    }
                } else {
                    if (newCastleHp >= 2) {
                        newCastleHp--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            shownCastleHp = "" + newCastleHp;
            shownCastleHp = guiMgr.DrawTextFieldCenteredWhite("newCastleHp", buttonRect, shownCastleHp, 6);
            float.TryParse(shownCastleHp, out newCastleHp);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    newCastleHp += 10;
                } else {
                    newCastleHp++;
                }
            }

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

        private void UpdateHpOnAllDoors()
        {
            foreach (BuildStructure structure in WorldManager.getInstance().PlayerFaction.structures) {
                if (structure.structureName.Contains("Gate") || structure.structureName.Contains("Door"))
                {
                    UpdateDoorHp(structure);
                }
            }
        }

        private void UpdateDoorHp(BuildStructure structure)
        {
            if (!SettingsManager.BoolSettings[(int) Preferences.DoorHpEnabled])
            {
                if (SettingsManager.BoolSettings[(int)Preferences.ShowDoorInfo]) {
                    GUIManager.getInstance().AddTextLine(structure.structureName + " has " + structure.health + " health.");
                }
                return;
            }

            switch (structure.structureName)
            {
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

            if (SettingsManager.BoolSettings[(int)Preferences.ShowDoorInfo])
            {
                GUIManager.getInstance().AddTextLine(structure.structureName + " has " + structure.health + " health.");                
            }
        }

        private void SetCurrentHpToDefault()
        {
            SettingsManager.CurrentFenceHp = DefaultFenceHp;
            SettingsManager.CurrentTimberHp = DefaultTimberHp;
            SettingsManager.CurrentBracedHp = DefaultBracedHp;
            SettingsManager.CurrentStuddedHp = DefaultStuddedHp;
            SettingsManager.CurrentDungeonHp = DefaultDungeonHp;
            SettingsManager.CurrentCastleHp = DefaultCastleHp;
        }

        private void SetCurrentHpToRecommended()
        {
            SettingsManager.CurrentFenceHp = 150;
            SettingsManager.CurrentTimberHp = 250;
            SettingsManager.CurrentBracedHp = 450;
            SettingsManager.CurrentStuddedHp = 600;
            SettingsManager.CurrentDungeonHp = 1000;
            SettingsManager.CurrentCastleHp = 4000;
        }

        private void SetCurrentHpToNew()
        {
            SettingsManager.CurrentFenceHp = newFenceHp;
            SettingsManager.CurrentTimberHp = newTimberHp;
            SettingsManager.CurrentBracedHp = newBracedHp;
            SettingsManager.CurrentStuddedHp = newStuddedHp;
            SettingsManager.CurrentDungeonHp = newDungeonHp;
            SettingsManager.CurrentCastleHp = newCastleHp;
        }

        private void UpdateMutiplayValues()
        {
            newBracedHp = DefaultBracedHp * amountToMutiplyDefault;
            newCastleHp = DefaultCastleHp * amountToMutiplyDefault;
            newDungeonHp = DefaultDungeonHp * amountToMutiplyDefault;
            newFenceHp = DefaultFenceHp * amountToMutiplyDefault;
            newStuddedHp = DefaultStuddedHp * amountToMutiplyDefault;
            newTimberHp = DefaultTimberHp * amountToMutiplyDefault;
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
                        GUIManager.getInstance().AddTextLine(structure.structureName + " has " + structure.health + " health.");                
                    }
                    else
                    {
                        theDoors.Remove(structure);
                    }
                }
            }
        }

        private void DrawHpBarAboveStructure(BuildStructure structure)
        {
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

            Vector3 vector = Camera.mainCamera.WorldToViewportPoint(structure.transform.position + new Vector3(0f, 0.6f, 0f));
            guiMgr.DrawProgressBar(new Rect(Screen.width * vector.x - 40f, Screen.height - Screen.height * vector.y, 80f, 16f), hpPrecent, true);
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnBuildStructure(EventBuildStructure evt)
        {
            if (evt.structure.structureName.Contains("Gate") || evt.structure.structureName.Contains("Door")) return;
            
            UpdateDoorHp(evt.structure);
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnGameLoad(EventGameLoad evt)
        {
            UpdateHpOnAllDoors();
        }
    }
}
