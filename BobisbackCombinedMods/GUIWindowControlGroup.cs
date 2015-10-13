using System.Linq;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class GUIWindowControlGroup : MonoBehaviour
    {

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private static readonly float[] DoubleTapDelayArray = { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
        private static readonly int[] NumberOfTapsArray = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private float prevRealTime;
        private float thisRealTime;

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted.
        public void Update()
        {
            prevRealTime = thisRealTime;
            thisRealTime = Time.realtimeSinceStartup;

            if (guiMgr.inGame && !guiMgr.gameOver && SettingsManager.BoolSettings[(int)Preferences.EnableControlGroups]) {
                KeyCode keyCode = KeyCode.Alpha0;
                for (int i = 0; i < 10; keyCode++, i++) {
                    if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(keyCode)) {
                        ProcessControlGroupDesignation(keyCode, i);
                    } else if (Input.GetKeyDown(keyCode)) {
                        ProcessControlGroupSelection(keyCode, i);
                    }
                    if (DoubleTapDelayArray[i] > 0) {
                        DoubleTapDelayArray[i] -= 1 * DeltaTime;
                    } else {
                        NumberOfTapsArray[i] = 0;
                    }
                }
            }
        }

        private void ProcessControlGroupDesignation(KeyCode keyCode, int i)
        {
            MonoBehaviour selectedObject = WorldManager.getInstance().PlayerFaction.selectedObject;
            if (selectedObject == null) {
                return;
            }

            ALivingEntity settler = WorldManager.getInstance().PlayerFaction.units.OfType<APlayableEntity>().Where(x => x.isAlive()).First(x => x.Equals(selectedObject));
            if (settler == null || !settler.isAlive()) {
                return;
            }

            SettingsManager.ControlGroupSettlers[i] = settler.unitName;
        }

        private void ProcessControlGroupSelection(KeyCode keyCode, int i)
        {
            string unitToSelect = SettingsManager.ControlGroupSettlers[i];
            ALivingEntity settler = WorldManager.getInstance().PlayerFaction.units.OfType<APlayableEntity>().Where(x => x.isAlive()).First(x => x.unitName == unitToSelect);
            if (settler == null || !settler.isAlive()) {
                SettingsManager.ControlGroupSettlers[i] = string.Empty;
                return;
            }

            if (WorldManager.getInstance().PlayerFaction.firstPersonUnit != null) {
                return;
            }

            MonoBehaviour selectedObject = WorldManager.getInstance().PlayerFaction.selectedObject;
            bool openSettlerWindow = selectedObject != null &&
                                     selectedObject.gameObject.tag == "ControllableUnit" &&
                                     AManager<GUIManager>.getInstance().settlerWindow.isOpen((APlayableEntity)selectedObject);
            
            WorldManager.getInstance().PlayerFaction.SelectObject(settler.transform, openSettlerWindow);

            if (DoubleTapDelayArray[i] > 0 && NumberOfTapsArray[i] == 1) {
                WorldManager.getInstance().PlayerFaction.MoveToPosition(settler.coordinate.world);
            } else {
                ResetDobuleTap(i);
            }
        }

        private void ResetDobuleTap(int i)
        {
            DoubleTapDelayArray[i] = 0.5f;
            NumberOfTapsArray[i] += 1;
        }

        private float DeltaTime
        { //this is to make our own delta time that does not relay on timescale
            get
            {
                if (Time.timeScale > 0f) return Time.deltaTime / Time.timeScale;
                return Time.realtimeSinceStartup - prevRealTime; // Checks realtimeSinceStartup again because it may have changed since Update was called
            }
        }
    }
}
