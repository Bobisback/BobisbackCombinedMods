using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Timber_and_Stone;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class GUIWindowResourcesMenu : MonoBehaviour, IEventListener
    {
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 10f;
        private static readonly float InbetweenMargin = 5f;
        private Rect windowRect = new Rect(30, 120, 800, 600);
        private Rect ScrollWindowRect = new Rect(0, 0, 780, 600-32);
        private static readonly int WindowId = 513;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String guiName = "Resource Menu";

        private static readonly Timer UpdateTimer = new Timer(500);
        private int[] storageIndexCounts;

        private ResourceManager resourceManager = ResourceManager.getInstance();
        private Vector2 resourcesScrollPosition;
        private int resetResourcesAmount;
        private bool resetResources;

        public IStorageController StorageController
        {
            get
            {
                return WorldManager.getInstance().PlayerFaction.storage as IStorageController;
            }
        }

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();

            storageIndexCounts = new int[11];
            foreach (Resource resource in resourceManager.resources.Where(resource => resource != null)) {
                storageIndexCounts[resource.storageIndex]++;
            }

            windowRect.x = Screen.width - 30 - windowRect.width;

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
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleResourceMenu]) {
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
                SettingsManager.BoolSettings[(int)Preferences.ToggleResourceMenu] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin - 32;

            resourcesScrollPosition = GUI.BeginScrollView(new Rect(0, 32, 800, 600-42), resourcesScrollPosition, ScrollWindowRect);

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight + (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Add 25 Resources")) {
                if (SettingsManager.BoolSettings[(int)Preferences.UnlimitedResources]) return;

                foreach (Resource resource in resourceManager.resources.Where(resource => resource != null)) {
                    StorageController.addResource(resource, 25);
                }
            }

            buttonRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Unlimited Resources", ref SettingsManager.BoolSettings[(int)Preferences.UnlimitedResources]);
            
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight + (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Reset All Resources To"))
            {
                resetResources = true;
            }

            buttonRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);

            resetResourcesAmount = BuildTextField(buttonRect, 0, 99999, resetResourcesAmount);
            
            Resource[] resourcesWithoutNulls = resourceManager.resources.Where(x => x != null).ToArray();
            
            for (int i = 0; i < resourcesWithoutNulls.Count(); i++)
            {
                bool isOdd = i % 2 != 0;
                BuildLabel(resourcesWithoutNulls[i], i % 2 != 0 ? buttonAboveHeight += (ButtonHeight + InbetweenMargin) : buttonAboveHeight + (ButtonHeight + InbetweenMargin), isOdd);
            }

            ScrollWindowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.EndScrollView();

            GUI.DragWindow();

            if (GUI.tooltip != string.Empty) {
                ParseToolTip();
            }
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            if (resetResources)
            {
                foreach (Resource resource in resourceManager.resources.Where(resource => resource != null)) {
                    StorageController.setResource(resource, resetResourcesAmount);
                }
                resetResources = false;
            }

            if (SettingsManager.BoolSettings[(int)Preferences.UnlimitedResources]) {
                if (StorageController == null) {
                    GUIManager.getInstance().AddTextLine("Storage failed");
                    return;
                }

                foreach (Resource resource in resourceManager.resources.Where(resource => resource != null)) {
                    StorageController.setStorageCap(resource.storageIndex, 1000);
                    var totalInStorageIndex = storageIndexCounts[resource.storageIndex];

                    if (totalInStorageIndex == 0 || resource.mass == 0) {
                        continue;
                    }

                    var totalMassAvailablePerStorageIndex = (799 / totalInStorageIndex);

                    var qty = (int)(totalMassAvailablePerStorageIndex / resource.mass);

                    StorageController.setResource(resource, qty);
                }
            }
        }

        /// <summary>
        /// This will build a button to display a idle settler. 
        /// It also has the logic in it to handle moving to and selecting a settler.
        /// </summary>
        /// <param name="settler">The settler that needs to be displayed</param>
        /// <param name="buttonAboveHeight">The y coord needed relative to all the views above it</param>
        /// <param name="isOdd">Is this a odd button.</param>
        private void BuildLabel(Resource resource, float buttonAboveHeight, bool isOdd)
        {
            Rect viewRect;
            if (isOdd) {
                viewRect = new Rect((LeftRightMargin) + ((windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight, (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            } else {
                viewRect = new Rect(LeftRightMargin, buttonAboveHeight, (windowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            }
            var maxWidth = viewRect.width;
            viewRect.width = 34 + 200;
            GUI.Label(viewRect, new GUIContent(string.Empty, "trade tooltip/" + resource.index), guiMgr.boxStyle);
            viewRect.width = 30;
            GUI.DrawTexture(viewRect, resource.icon);
            viewRect.x += 34;
            viewRect.width = 195;
            viewRect.y += 5;
            guiMgr.DrawTextLeftWhite(viewRect, resource.name);
            viewRect.x += 200;
            viewRect.width = maxWidth - 10;
            StorageController.setResource(resource, BuildTextField(viewRect, 0, 99999, StorageController.getResource(resource)));
        }

        private int BuildTextField(Rect buttonRect, float minValue, float maxValue, float value)
        {
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.minusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (value >= minValue + 10) {
                        value -= 10;
                    }
                } else {
                    if (value >= minValue) {
                        value--;
                    }
                }
            }
            buttonRect.x += 20;
            buttonRect.width = 84;
            buttonRect.height = 24;
            if (value < minValue) {
                value = minValue;
            }
            if (value > maxValue) {
                value = maxValue;
            }
            string shownValue = "" + value;
            GUI.Label(buttonRect, new GUIContent(string.Empty, "maxValue/" + maxValue));
            shownValue = guiMgr.DrawTextFieldCenteredWhite("textField", buttonRect, shownValue, 6);
            float.TryParse(shownValue, out value);
            buttonRect.x += 84;
            buttonRect.width = 20;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, string.Empty, guiMgr.plusButtonStyle)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (value <= maxValue + 10) {
                        value += 10;
                    }
                } else {
                    if (value <= maxValue) {
                        value++;
                    }
                }
            }
            return (int)value;
        }

        private void ParseToolTip()
        {
            string[] array = GUI.tooltip.Split(new char[]
	            {
		            "/"[0]
	            });
            string a = array[0];
            if (a == "trade tooltip") {
                int id = int.Parse(array[1]);
                Rect rect = new Rect(Event.current.mousePosition.x + 16f, Event.current.mousePosition.y + 4f, 200f, 28f);
                guiMgr.DrawTextLeftWhite(new Rect(rect.xMin + 6f, rect.yMin + 4f, rect.width, 28f), "Stockpiled: " + GUIWindowTradeOverHaul.FormatNumberPostfix(guiMgr.storage[Resource.FromID(id)]));
            }

            if (a == "maxValue") {
                int maxValue = int.Parse(array[1]);
                Rect rect = new Rect(Event.current.mousePosition.x + 16f, Event.current.mousePosition.y + 4f, 200f, 28f);
                guiMgr.DrawTextLeftWhite(new Rect(rect.xMin + 6f, rect.yMin + 4f, rect.width, 28f), "Max: " + maxValue);
            }
        }
    }
}
