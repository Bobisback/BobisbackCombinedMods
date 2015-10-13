using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class GuiDisplaySettlerCount : MonoBehaviour
    {
        private GUIManager guiManager;
        private UnitManager unitManager;
        private int playerUnitCount;

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        void Start()
        {
            guiManager = AManager<GUIManager>.getInstance();
            unitManager = AManager<UnitManager>.getInstance();
        }

        //This is called alot less then ongui and can have some model data manipulation in it.
        //This is also were any hotkeys are intercepted. DO not display GUI stuff in this function
        void Update()
        {
            playerUnitCount = unitManager.playerUnits.Count;
        }

        //called anywhere from 60 times a sec to 1000 times a second. Only display GUI in this function. 
        //No model data should built/manipulated.
        void OnGui()
        {
            Rect labelRect = new Rect(10f, 120f, 300f, 55f);
            guiManager.DrawWindow(labelRect, "Display Settler Count", false);
            labelRect = new Rect(10f, 120f + 30f, 300f, 25f);
            guiManager.DrawTextCenteredBlack(labelRect, "Settler Count: " + playerUnitCount);
        }
    }
}
