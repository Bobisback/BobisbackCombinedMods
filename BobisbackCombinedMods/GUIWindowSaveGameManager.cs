using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using Plugin.Bobisback.CombinedMods.Extensions;
using Timber_and_Stone;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using Timber_and_Stone.Invasion;
using Timber_and_Stone.Utility;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{

    /// <summary>
    /// This class reprosents the save game manager window. This window
    /// will include autosave configuration as well as backup configuration
    /// </summary>
    public class GUIWindowSaveGameManager : MonoBehaviour, IEventListener
    {
        private const float ButtonHeight = 32;
        private const float LeftRightMargin = 15;
        private const float TopBottomMargin = 7.5f;
        private const float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(200, 40, 425, 237);
        private const int SaveGameManagerWindowId = 515;

        private const String SavePath = "./saves/";
        private const String SaveBackupPath = SavePath + "./backups/";
        private const String SaveExtension = ".tass.gz";

        private readonly TimeManager timeManager = TimeManager.getInstance();
        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly WorldManager worldManager = WorldManager.getInstance();
        private FileSystemWatcher fileSystemWatcher;
        private readonly String saveGameManagerGUIName = "Save Game Manager";

        private static readonly Timer UpdateTimer = new Timer(500);
        private readonly Timer SaveGameTimer = new Timer();

        private string shownAutoSaveCount;
        private string shownBackupsCount;

        private bool shouldSave;
        private bool timerReadyToRun = true;

        private MemoryStream inRamMemoryStream;

        private float CurrentAutosaveMinCount
        {
            get { return SettingsManager.CurrentAutosaveMinCount; }
            set
            {
                if (value != SettingsManager.CurrentAutosaveMinCount)
                {
                    SettingsManager.CurrentAutosaveMinCount = value;
                    if (!guiMgr.inGame || guiMgr.gameOver) return;// make sure we are in the game
                    SaveGameTimer.Stop();
                    SaveGameTimer.Interval = CurrentAutosaveMinCount * 1000;
                    SaveGameTimer.Start();
                    timerReadyToRun = false;
                }
            }
        }

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();

            SaveGameTimer.Elapsed += SaveGameTimerEvent;

            EventManager.getInstance().Register(this);
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update()
        {
            if (Input.GetKeyDown(SettingsManager.HotKeys["toggleSaveGameManagerHotKey"]))
            {
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleSaveGameManager] == false)
                {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleSaveGameManager] = true;
                    WorldManager.getInstance().PlayerFaction.DeSelect();
                    AManager<GUIManager>.getInstance().GetComponent<MainMenus>().CloseAll();
                } else {
                    SettingsManager.BoolSettings[(int)Preferences.ToggleSaveGameManager] = false;
                }
            }
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        public void OnGUI()
        {
            if (SettingsManager.BoolSettings[(int)Preferences.ToggleSaveGameManager])
            {
                windowRect = GUI.Window(SaveGameManagerWindowId, windowRect, BuildSaveGameManager, string.Empty, guiMgr.windowBoxStyle);
            }

            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }

        private void BuildSaveGameManager(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, saveGameManagerGUIName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle))
            {
                SettingsManager.BoolSettings[(int)Preferences.ToggleSaveGameManager] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Autosave Configuration");

            buttonRect = new Rect(LeftRightMargin * 2, buttonAboveHeight + (ButtonHeight + InbetweenMargin), 150, ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "", ref SettingsManager.BoolSettings[(int)Preferences.SaveEveryMinEnabled]);

            CurrentAutosaveMinCount = BuildTextField("Save every ", " min.", ref buttonAboveHeight, CurrentAutosaveMinCount, ref shownAutoSaveCount);

            buttonRect = new Rect(LeftRightMargin * 2, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Save Every", ref SettingsManager.BoolSettings[(int)Preferences.SaveEveryGameHourEnabled]);

            buttonRect = new Rect(LeftRightMargin * 4, buttonAboveHeight + (ButtonHeight + InbetweenMargin), 150, ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Morning", ref SettingsManager.BoolSettings[(int)Preferences.SaveEveryMorning]);

            buttonRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), 150, ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Afternoon", ref SettingsManager.BoolSettings[(int)Preferences.SaveEveryAfternoon]);

            buttonRect = new Rect(LeftRightMargin * 4, buttonAboveHeight + (ButtonHeight + InbetweenMargin), 150, ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Evening", ref SettingsManager.BoolSettings[(int)Preferences.SaveEveryEvening]);

            buttonRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), 150, ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Midnight", ref SettingsManager.BoolSettings[(int)Preferences.SaveEveryMidnight]);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Backups Configuration");

            buttonRect = new Rect(LeftRightMargin * 2, buttonAboveHeight + (ButtonHeight + InbetweenMargin), 150, ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "", ref SettingsManager.BoolSettings[(int)Preferences.BackupsEnabled]);

            SettingsManager.CurrentBackupsCount = BuildTextField("Keep ", " backups.", ref buttonAboveHeight, SettingsManager.CurrentBackupsCount, ref shownBackupsCount);

            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Open Backups Manager"))
            {

            }

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private float BuildTextField(string textToShow, string endTextToShow, ref float buttonAboveHeight, float newValue, ref string shownValue)
        {
            Rect buttonRect = new Rect(LeftRightMargin * 4, buttonAboveHeight += ButtonHeight, windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, textToShow);
            buttonRect.x += 125;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (newValue >= 11)
                    {
                        newValue -= 10;
                    }
                } else {
                    if (newValue >= 2)
                    {
                        newValue--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 44;
            buttonRect.height = 24;
            shownValue = "" + newValue;
            shownValue = guiMgr.DrawTextFieldCenteredWhite("textField", buttonRect, shownValue, 3);
            float.TryParse(shownValue, out newValue);
            buttonRect.x += 44;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    newValue += 10;
                } else {
                    newValue++;
                }
            }
            buttonRect.x += 25;
            buttonRect.width = 100;
            buttonRect.height = ButtonHeight;
            guiMgr.DrawTextLeftBlack(buttonRect, endTextToShow);

            return newValue;
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            if (!guiMgr.inGame || guiMgr.gameOver) return;// make sure we are in the game

            if (SettingsManager.BoolSettings[(int)Preferences.SaveEveryMinEnabled] && timerReadyToRun)
            {
                SaveGameTimer.Interval = SettingsManager.CurrentAutosaveMinCount * 60000;
                SaveGameTimer.Start();
                timerReadyToRun = false;
            }

            if (!SettingsManager.BoolSettings[(int)Preferences.SaveEveryMinEnabled])
            {
                SaveGameTimer.Stop();
                timerReadyToRun = true;
            }

            switch (timeManager.hour)
            {
                case 1:
                case 9:
                case 15:
                case 21:
                case 7:
                case 13:
                case 19:
                case 23:
                    shouldSave = true;
                    break;
            }

            if (SettingsManager.BoolSettings[(int)Preferences.SaveEveryGameHourEnabled] && shouldSave)
            {
                if (timeManager.hour == 0 && SettingsManager.BoolSettings[(int)Preferences.SaveEveryMidnight])
                {
                    worldManager.SaveGame();
                    shouldSave = false;
                } else if (timeManager.hour == 8 && SettingsManager.BoolSettings[(int)Preferences.SaveEveryMorning])
                {
                    worldManager.SaveGame();
                    shouldSave = false;
                } else if (timeManager.hour == 14 && SettingsManager.BoolSettings[(int)Preferences.SaveEveryAfternoon])
                {
                    worldManager.SaveGame();
                    shouldSave = false;
                } else if (timeManager.hour == 20 && SettingsManager.BoolSettings[(int)Preferences.SaveEveryEvening])
                {
                    worldManager.SaveGame();
                    shouldSave = false;
                }
            }
        }

        private void SaveGameTimerEvent(object sender, ElapsedEventArgs e)
        {
            if (!guiMgr.inGame || guiMgr.gameOver) return;// make sure we are in the game
            worldManager.SaveGame();
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnGameLoad(EventGameLoad evt)
        {
            if (SettingsManager.BoolSettings[(int)Preferences.BackupsEnabled])
            {
                //load ram file into the memeory stream
                string saveGameFile = SavePath + worldManager.settlementName + SaveExtension;

                fileSystemWatcher = new FileSystemWatcher(SavePath);
                fileSystemWatcher.Path = Path.GetDirectoryName(saveGameFile);
                fileSystemWatcher.Filter = Path.GetFileName(saveGameFile);
                fileSystemWatcher.EnableRaisingEvents = true;
                fileSystemWatcher.Changed += FileSystemWatcherOnChanged;

                inRamMemoryStream = new MemoryStream();
                using (FileStream fileStream = File.OpenRead(saveGameFile))
                {
                    fileStream.CopyTo(inRamMemoryStream);
                }

                guiMgr.AddTextLine("File should be loaded into ram");
            }
        }

        private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            guiMgr.AddTextLine("File changed");

            try
            {
                if (SettingsManager.BoolSettings[(int)Preferences.BackupsEnabled])
                {
                    if (!Directory.Exists(SaveBackupPath))
                    {
                        Directory.CreateDirectory(SaveBackupPath);
                    }

                    string saveGameBackupFile = SaveBackupPath + worldManager.settlementName + 
                         1 + SaveExtension;

                    //DateTime.Now.ToString("_MM_dd_yyyy_hh_mm_ss")

                    //take current ram file write to disk as backup (do complicated create new backup stuffs)
                    using (FileStream fileStream = File.OpenWrite(saveGameBackupFile))
                    {
                        fileStream.Write(inRamMemoryStream.ToArray(), 0, inRamMemoryStream.ToArray().Length);
                    }
                    guiMgr.AddTextLine("Backup should be made");

                    //take new save file and load into ram file
                    string saveGameFile = SavePath + worldManager.settlementName + SaveExtension;

                    inRamMemoryStream = new MemoryStream();
                    using (FileStream fileStream = File.OpenRead(saveGameFile))
                    {
                        fileStream.CopyTo(inRamMemoryStream);
                    }
                    guiMgr.AddTextLine("New File should be loaded into ram");
                }
            }
            catch (Exception ex)
            {
                guiMgr.AddTextLine("Error: " + ex);
                throw;
            }
        }
    }
}
