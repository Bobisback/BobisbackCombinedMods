using System.Linq;
using System.Timers;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class DefaultGUIWindow : MonoBehaviour, IEventListener
    {
        private static readonly float ButtonHeight = 32;
        private static readonly float LeftRightMargin = 15;
        private static readonly float TopBottomMargin = 7.5f;
        private static readonly float InbetweenMargin = 2.5f;
        private Rect windowRect = new Rect(0, 0, 300, 300);

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly string guiName = "No Name";

        private static readonly Timer UpdateTimer = new Timer(500);

        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();

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
            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }
        
        public virtual void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnGameLoad(EventGameLoad evt)
        {
            windowRect.x = 0;
            windowRect.y = 0;
        }
    }
}
