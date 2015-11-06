using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods {
    public class GUIWindowMutiSelection : MonoBehaviour, IEventListener
    {
        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private static readonly Timer UpdateTimer = new Timer(500);

        private Vector2 lmbDown;
        private Vector2 lmbUp;
        private bool lmbDrag;

        private Rect selectionBox;
        private List<APlayableEntity> selectedSettlers = new List<APlayableEntity>();

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start() {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();
            
            EventManager.getInstance().Register(this);
        }
        
        // Update is called once per frame
        public void Update() {
            if (Input.GetMouseButtonDown(0)) {
                lmbDown = Input.mousePosition;
                DeselectSettlers();
            }

            if (Input.GetMouseButtonUp(0)) {
                lmbUp = Input.mousePosition;
                lmbDrag = false;
                SelectMultipleObjects(lmbDown, lmbUp);
            }

            if (Input.GetMouseButton(0))
            {
                SelectionBox(Input.mousePosition);
            }
        }

        public void OnGUI() {
            if (lmbDrag) {
                float width = lmbUp.x - lmbDown.x;
                float height = (Screen.height - lmbUp.y) - (Screen.height - lmbDown.y);
                selectionBox = new Rect(lmbDown.x, Screen.height - lmbDown.y, width, height);
                GUI.Box(selectionBox, "");
            }
        }

        private void SelectMultipleObjects(Vector2 originalPos, Vector2 currentPos) {
            try
            {
                foreach (APlayableEntity aPlayableEntity in WorldManager.getInstance().PlayerFaction.units.OfType<APlayableEntity>().Where(x => x.isAlive())) //represents all the movable units
                {
                    //convert the current object position to screen coordinates
                    var screenCoordinates = Camera.main.WorldToScreenPoint(aPlayableEntity.transform.position);

                    //Find all the objects inside the box
                    if (selectionBox.Contains(screenCoordinates)) {
                        //do magic unit selection
                        aPlayableEntity.Select();
                        selectedSettlers.Add(aPlayableEntity);
                    }
                }
                GUIManager.getInstance().AddTextLine("There are " + selectedSettlers.Count + " selected settlers.");
            }
            catch (Exception ex)
            {
                GUIWindowModOptions.DisplayErrorMessage("" + ex.Message);
            }
        }

        private void DeselectSettlers() {
            foreach (APlayableEntity selectedSettler in selectedSettlers)
            {
                selectedSettler.Deselect();
            }
            selectedSettlers.Clear();
        }
        
        private void SelectionBox(Vector2 screenPosition) {
            if (screenPosition != lmbDown) {
                lmbDrag = true;
                lmbUp = screenPosition;
            }
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e) {
            
        }
    }
}
