using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Timber_and_Stone.API;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using EventHandler = Timber_and_Stone.API.Event.EventHandler;

namespace Plugin.Bobisback.CombinedMods {
    public class GUIWindowControlGroup : MonoBehaviour {

        private GUIManager guiMgr = GUIManager.getInstance();
        private static float[] doubleTapDelayArray = { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
        private static float[] numberOfTapsArray = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private float prevRealTime;
        private float thisRealTime;

        void Update() {
            prevRealTime = thisRealTime;
            thisRealTime = Time.realtimeSinceStartup;

            if (guiMgr.inGame && !guiMgr.gameOver && SettingsManager.boolSettings[(int)Preferences.enableControlGroups]) {

                KeyCode keyCode = KeyCode.Alpha0;
                for (int i = 0; i < 10; keyCode++, i++) {
                    if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(keyCode)) {
                        processControlGroupDesignation(keyCode, i);
                    } else if (Input.GetKeyDown(keyCode)) {
                        processControlGroupSelection(keyCode, i);
                    }
                    if (doubleTapDelayArray[i] > 0) {
                        doubleTapDelayArray[i] -= 1 * deltaTime;
                    } else {
                        numberOfTapsArray[i] = 0;
                    }
                }
            }
        }

        private void processControlGroupDesignation(KeyCode keyCode, int i) {
            MonoBehaviour selectedObject = UnitManager.getInstance().controllerObj.GetComponent<ControlPlayer>().selectedObject;
            if (selectedObject == null) {
                return;
            }

            ALivingEntity settler = UnitManager.getInstance().playerUnits.Find(x => x.Equals(selectedObject));
            if (settler == null || !settler.isAlive()) {
                return;
            }

            SettingsManager.controlGroupSettlers[i] = settler.unitName;
        }

        private void processControlGroupSelection(KeyCode keyCode, int i) {
            string unitToSelect = SettingsManager.controlGroupSettlers[i];
            ALivingEntity settler = UnitManager.getInstance().playerUnits.Find(x => x.unitName == unitToSelect);
            if (settler == null || !settler.isAlive()) {
                SettingsManager.controlGroupSettlers[i] = string.Empty;
                return;
            }

            MonoBehaviour selectedObject = UnitManager.getInstance().controllerObj.GetComponent<ControlPlayer>().selectedObject;

            bool openSettlerWindow = false;
            if (selectedObject != null && selectedObject.gameObject.tag == "ControllableUnit" && AManager<GUIManager>.getInstance().GetComponent<HumanSettlerWindow>().entity == selectedObject) {
                openSettlerWindow = true;
            }
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().SelectObject(settler.transform, openSettlerWindow);

            if (doubleTapDelayArray[i] > 0 && numberOfTapsArray[i] == 1) {
                AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().MoveToPosition(settler.coordinate.world);
            } else {
                resetDobuleTap(i);
            }
        }

        private void resetDobuleTap(int i) {
            doubleTapDelayArray[i] = 0.5f;
            numberOfTapsArray[i] += 1;
        }

        private float deltaTime { //this is to make our own delta time that does not relay on timescale
            get {
                if (Time.timeScale > 0f) return Time.deltaTime / Time.timeScale;
                return Time.realtimeSinceStartup - prevRealTime; // Checks realtimeSinceStartup again because it may have changed since Update was called
            }
        }
    }
}
