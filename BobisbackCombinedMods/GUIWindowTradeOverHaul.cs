using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timber_and_Stone;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using Timber_and_Stone.Profession.Human;
using Timber_and_Stone.Tasks;
using Timber_and_Stone.Utility;
using UnityEngine;

namespace Plugin.Bobisback.CombinedMods
{
    public class GUIWindowTradeOverHaul : MonoBehaviour, IEventListener
    {
        private const float ButtonHeight = 32;
        private const float LeftRightMargin = 15;
        private const float TopBottomMargin = 7.5f;
        private const float InbetweenMargin = 5f;
        private Rect windowRect = new Rect(30, 240, 350, 237);
        private const int TradeMenuSettingsWindowId = 510;
        private Rect openTradeWindowRect = new Rect(30, 80, 290, 47);
        private const int OpenTradeWindowId = 511;
        private Rect tradeWindowRect = new Rect(0f, 40f, 750f, 430f);
        private const int TradeWindowId = 512;

        private readonly GUIManager guiMgr = GUIManager.getInstance();
        private readonly String tradeMenuSettingsName = "Trade Menu Settings";
        private readonly String tradeWindowName = "Trade Menu";
        private readonly String openTradeWindowName = "Negotiate New Transaction";

        private static readonly Timer UpdateTimer = new Timer(500);
        private ALivingEntity merchant;

        private bool showTradeWindow;
        private bool tradeOnGoing;
        private bool merchantInHall;
        private int playerCoin;
        private int merchantCoin;
        private int totalTransactionCost;

        private readonly HashSet<Resource> resourcesBought = new HashSet<Resource>();
        private readonly HashSet<Resource> resourcesSold = new HashSet<Resource>();

        private readonly List<Transaction> TradeWindowSellList = new List<Transaction>();
        private readonly List<Transaction> TradeWindowBuyList = new List<Transaction>();
        private APlayableEntity trader;
        private bool needToCreateTransaction;
        
        //This function is called once when this window starts up. 
        //Do any one time setup/init things in this function.
        public void Start()
        {
            UpdateTimer.Elapsed += UpdateGameVariables;
            UpdateTimer.Start();

            windowRect.x = Screen.width - 30 - windowRect.width;
            tradeWindowRect.x = (Screen.width - tradeWindowRect.width) / 2;

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
                if (SettingsManager.BoolSettings[(int)Preferences.ToggleTradeSettingsMenu]) {
                    windowRect = GUI.Window(TradeMenuSettingsWindowId, windowRect, BuildTradeMenuSettingsMenu, string.Empty, guiMgr.windowBoxStyle);
                }

                if (!SettingsManager.BoolSettings[(int)Preferences.ToggleNewTradeMenu]) return;
                
                if (GUIManager.getInstance().merchantTrade != null) {
                    TimeManager.getInstance().play();
                    GUIManager.getInstance().merchantTrade = null;
                }
                if (tradeOnGoing) {
                    openTradeWindowRect = GUI.Window(OpenTradeWindowId, openTradeWindowRect, BuildOpenTradeWindow, string.Empty, guiMgr.windowBoxStyle);
                }
                if (showTradeWindow) {
                    tradeWindowRect = GUI.Window(TradeWindowId, tradeWindowRect, BuildTradeWindow, string.Empty, guiMgr.windowBoxStyle);
                }
            }

            tradeWindowRect.x = Mathf.Clamp(tradeWindowRect.x, 2f, Screen.width - tradeWindowRect.width - 2f);
            tradeWindowRect.y = Mathf.Clamp(tradeWindowRect.y, 40f, Screen.height - tradeWindowRect.height - 2f);
            windowRect.x = Mathf.Clamp(windowRect.x, 2f, Screen.width - windowRect.width - 2f);
            windowRect.y = Mathf.Clamp(windowRect.y, 40f, Screen.height - windowRect.height - 2f);
        }

        private void BuildTradeWindow(int id)
        {
            AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().DeSelect();
            AManager<TimeManager>.getInstance().pause();

            Rect backGroundWindow = new Rect(0f, 0f, tradeWindowRect.width, tradeWindowRect.height);
            guiMgr.DrawWindow(backGroundWindow, tradeWindowName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                showTradeWindow = false;
                AManager<TimeManager>.getInstance().play();
                return;
            }

            float buttonAboveHeight = TopBottomMargin;

            //Merchant Coins
            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight + (ButtonHeight + InbetweenMargin), (tradeWindowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "Merchant Coin: " + merchantCoin);

            //Player Coins
            buttonRect = new Rect((LeftRightMargin) + ((tradeWindowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), (tradeWindowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            guiMgr.DrawTextRightBlack(buttonRect, "Your Coin: " + playerCoin);

            //The merchant is willing to buy
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), tradeWindowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "The merchant is willing to buy ");

            //For loop building the list of items to buy
            foreach (Transaction transaction in TradeWindowBuyList)
            {
                BuildTransactionView(transaction, ref buttonAboveHeight);
            }

            //The merchant is willing to sell
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), tradeWindowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawTextLeftBlack(buttonRect, "The merchant is willing to sell ");

            //For loop building the list of items to sell
            foreach (Transaction transaction in TradeWindowSellList) {
                BuildTransactionView(transaction, ref buttonAboveHeight);
            }

            //Complete Transaction
            buttonRect = new Rect(LeftRightMargin, buttonAboveHeight + (ButtonHeight + InbetweenMargin), (tradeWindowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            var totalTransactions = TradeWindowBuyList.Count(x => x.completeTransaction) +
                                    TradeWindowSellList.Count(x => x.completeTransaction);
            if (guiMgr.DrawButton(buttonRect, "Complete " + totalTransactions + " Transaction(s)")) {
                if (totalTransactionCost > playerCoin)
                {
                    GUIWindowModOptions.DisplayMessage("Not Enough Coins", "We cannot afford to complete the transaction");
                } 
                else if (trader.faction.storage.getStorageAvailable(StorageType.Treasure) < 
                    totalTransactionCost*-1*Resource.FromID(55).mass)
                {
                    GUIWindowModOptions.DisplayMessage("To Many Coins", "Our coffers can't hold that much coin.");
                }
                else if (TradeWindowSellList.Any(x => x.storageNeeded != 0))
                {
                    GUIWindowModOptions.DisplayMessage("Not Enough Storage", "More storage is need");
                }
                else if ((totalTransactionCost*-1) > merchantCoin) 
                {
                    GUIWindowModOptions.DisplayMessage("Not Enough Coins", "Merchant cannot afford to complete the transaction");
                }
                else
                {
                    CompleteTransactions();
                }
            }

            //Dispaly total coin cost/total coin aqiured
            buttonRect = new Rect((LeftRightMargin) + ((tradeWindowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2)) + InbetweenMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), (tradeWindowRect.width - LeftRightMargin * 2) / 2 - (InbetweenMargin / 2), ButtonHeight);
            string temp = totalTransactionCost < 0 ? "Transaction Profit: " + (totalTransactionCost*-1) : "Transaction Cost: " + totalTransactionCost;
            guiMgr.DrawTextRightBlack(buttonRect, temp);

            tradeWindowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();

            if (GUI.tooltip != string.Empty) {
                ParseToolTip();
            }
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
                guiMgr.DrawTextLeftWhite(new Rect(rect.xMin + 6f, rect.yMin + 4f, rect.width, 28f), "Stockpiled: " + FormatNumberPostfix(guiMgr.storage[Resource.FromID(id)]));
            }

            if (a == "maxValue") {
                int maxValue = int.Parse(array[1]);
                Rect rect = new Rect(Event.current.mousePosition.x + 16f, Event.current.mousePosition.y + 4f, 200f, 28f);
                guiMgr.DrawTextLeftWhite(new Rect(rect.xMin + 6f, rect.yMin + 4f, rect.width, 28f), "Max: " + maxValue);
            }
        }

        private void CompleteTransactions()
        {
            IStorage storage = WorldManager.getInstance().PlayerFaction.storage;

            //Do all the buying
            foreach (Transaction transaction in TradeWindowBuyList)
            {
                if (!transaction.completeTransaction) continue;
                storage.addResource(Resource.FromID(55), transaction.totalValue);
                storage.removeResource(transaction.resource, transaction.amount);
                merchantCoin -= transaction.totalValue;
                transaction.maxAmount -= transaction.amount;
                transaction.amount = transaction.maxAmount/2;
                transaction.completeTransaction = false;
            }

            //Do all the selling
            foreach (Transaction transaction in TradeWindowSellList)
            {
                if (!transaction.completeTransaction) continue;
                storage.addResource(transaction.resource, transaction.amount);
                storage.removeResource(Resource.FromID(55), transaction.totalValue);
                merchantCoin += transaction.totalValue;
                transaction.maxAmount -= transaction.amount;
                transaction.amount = transaction.maxAmount / 2;
                transaction.completeTransaction = false;
            }

            UpdateGameVariables(null, null);

            AManager<TimeManager>.getInstance().play();
            showTradeWindow = false;
        }

        private void BuildTransactionView(Transaction transaction, ref float buttonAboveHeight)
        {
            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), 20, ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "", ref transaction.completeTransaction);
            buttonRect.x += 30;
            transaction.amount = BuildTextField(buttonRect, 0, transaction.maxAmount, transaction.amount);
            buttonRect.x += 130;
            buttonRect.width = 100;
            buttonRect.y -= InbetweenMargin;
            GUI.Label(buttonRect, new GUIContent(string.Empty, "trade tooltip/" + transaction.resource.index), guiMgr.boxStyle);
            buttonRect.width = 560;
            GUI.Label(buttonRect, new GUIContent(string.Empty), guiMgr.boxStyle);
            buttonRect.x += 5;
            buttonRect.width = 30;
            GUI.DrawTexture(buttonRect, transaction.resource.icon);

            string temp = transaction.storageNeeded == 0 ? "" : " (" + Mathf.CeilToInt(transaction.storageNeeded) + " Storage needed)";

            buttonRect.x += 37;
            buttonRect.y += InbetweenMargin;
            buttonRect.width = 530;
            guiMgr.DrawTextLeftWhite(buttonRect, transaction.resource.name + " for " + 
                transaction.totalValue + " coin. " + transaction.value + " coin per." + temp);

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
            if (value < minValue)
            {
                value = minValue;
            }
            if (value > maxValue) 
            {
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
            return (int) value;
        }

        private void BuildOpenTradeWindow(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, openTradeWindowRect.width, openTradeWindowRect.height);
            guiMgr.DrawWindow(backGroundWindow, openTradeWindowName, false);

            float buttonAboveHeight = 0;

            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), openTradeWindowRect.width - (LeftRightMargin * 2), ButtonHeight);
            if (guiMgr.DrawButton(buttonRect, "Start New Transaction")) {
                showTradeWindow = true;
            }

            openTradeWindowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;
        }

        private void BuildTradeMenuSettingsMenu(int id)
        {
            Rect backGroundWindow = new Rect(0f, 0f, windowRect.width, windowRect.height);
            guiMgr.DrawWindow(backGroundWindow, tradeMenuSettingsName, false);

            if (GUI.Button(new Rect(backGroundWindow.xMax - 24f, backGroundWindow.yMin + 4f, 20f, 20f), string.Empty, guiMgr.closeWindowButtonStyle)) {
                SettingsManager.BoolSettings[(int)Preferences.ToggleTradeSettingsMenu] = false;
                return;
            }

            float buttonAboveHeight = TopBottomMargin;
            
            Rect buttonRect = new Rect(LeftRightMargin, buttonAboveHeight += (ButtonHeight + InbetweenMargin), windowRect.width - (LeftRightMargin * 2), ButtonHeight);
            guiMgr.DrawCheckBox(buttonRect, "Activate New Trade Menu", ref SettingsManager.BoolSettings[(int)Preferences.ToggleNewTradeMenu]);
            
            //TODO set some things like amount of gold the merchant has

            windowRect.height = buttonAboveHeight + ButtonHeight + InbetweenMargin + TopBottomMargin;

            GUI.DragWindow();
        }

        private void UpdateGameVariables(object sender, ElapsedEventArgs e)
        {
            if (!SettingsManager.BoolSettings[(int)Preferences.ToggleNewTradeMenu]) return;

            totalTransactionCost = CalculateTransactionCost();

            CalculateStorageNeeded();

            playerCoin = WorldManager.getInstance().PlayerFaction.storage[Resource.FromID(55)];

            if (merchant != null) {
                if (merchant.taskStackContains(typeof(TaskExitMapViaRoads))) {
                    tradeOnGoing = false;
                    merchantInHall = false;
                    merchant = null;
                    trader = null;
                }
            }

            trader = GetTrader();
            if (merchant != null && trader != null && merchantInHall)
            {
                tradeOnGoing = true;
                if (needToCreateTransaction)
                {
                    merchantCoin = 0;
                    resourcesBought.Clear();
                    resourcesSold.Clear();
                    TradeWindowBuyList.Clear();
                    TradeWindowSellList.Clear();

                    CreateRandomTransaction();
                    needToCreateTransaction = false;
                }
            }
            else
            {
                tradeOnGoing = false;
            }
        }

        private void CalculateStorageNeeded()
        {
            float[] storageNeeded = new float[11];

            //get the storage amounts needed
            foreach (Transaction transaction in TradeWindowSellList.Where(x => x.completeTransaction)) {
                storageNeeded[transaction.resource.storageIndex] += transaction.storageAmountNeededForTransaction;
            }

            foreach (Transaction transaction in TradeWindowSellList) {
                if (storageNeeded[transaction.resource.storageIndex] > WorldManager.getInstance().PlayerFaction.storage.getStorageAvailable(transaction.resource.storageIndex) && transaction.completeTransaction)
                {
                    transaction.storageNeeded = storageNeeded[transaction.resource.storageIndex] - WorldManager.getInstance().PlayerFaction.storage.getStorageAvailable(transaction.resource.storageIndex);
                } 
                else 
                {
                    transaction.storageNeeded = 0;
                }
            }
        }

        private int CalculateTransactionCost()
        {
            int transactionCost = 0;

            foreach (Transaction transaction in TradeWindowBuyList.Where(x => x.completeTransaction))
            {
                transactionCost -= transaction.totalValue;
            }
            foreach (Transaction transaction in TradeWindowSellList.Where(x => x.completeTransaction)) 
            {
                transactionCost += transaction.totalValue;
            }
            return transactionCost;
        }

        [Timber_and_Stone.API.Event.EventHandler(Priority.Normal)]
        public void OnMerchantTrade(EventMerchantArrived evt)
        {
            showTradeWindow = false;
            merchantInHall = true;
            needToCreateTransaction = true;
            merchant = evt.getUnit();
        }

        private APlayableEntity GetTrader()
        {
            foreach (APlayableEntity current in AManager<WorldManager>.getInstance().controllerObj.GetComponent<ControlPlayer>().units.OfType<APlayableEntity>()) {
                if (current.isAlive() && current.getProfession() is Trader) {
                    Coordinate coordinate = current.coordinate;
                    if (AManager<ChunkManager>.getInstance().chunkArray[coordinate.chunk.x, coordinate.chunk.y - 1, coordinate.chunk.z].blocks[coordinate.block.x, coordinate.block.y, coordinate.block.z].isHall) {
                        return current;
                    }
                }
            }
            return null;
        }

        private void CreateRandomTransaction()
        {
            if (merchant == null || trader == null) return;

            //Amount of items to sell
            int amountToSell = UnityEngine.Random.Range(3, 8);
            //per item, need max amount trader has
            for (int i = 0; i < amountToSell; i++) {
                BuildMerchantSellTransaction();
            }

            //Amount of items to buy
            int amountToBuy = UnityEngine.Random.Range(3, 8);
            //per item, need max amount trader is willing to buy
            for (int i = 0; i < amountToBuy; i++)
            {
                BuildMerchantBuyTransaction();
            }
        }

        private void BuildMerchantSellTransaction()
        {
            if (AManager<ResourceManager>.getInstance().buyList.Count == 0) {
                trader.setBubble("I have nothing to buy.");
                return;
            }
            Resource resource = ResourceManager.getInstance().buyList.RandomElement<Resource>();
            if (resourcesSold.Contains(resource) || resource.index == 55)
            {
                return;
            }
            if (UnityEngine.Random.value < 0.2f)
            {
                //this.unit.setBubble("I'm afraid I have no use for " + resource.name + " at this time.");
                resourcesBought.Add(resource);
                return;
            }
            if (resourcesBought.Contains(resource))
            {
                return;
            }

            int traderLevel = trader.getProfession().getLevel();
            if (trader.preferences["trait.charismatic"])
            {
                traderLevel = Mathf.Min(traderLevel + 5, 20);
            }
            float traderInfluence = 2f - (float) (traderLevel/40);
            int resourceCount = 1;
            if (resource.name.Contains("Arrows") || resource.name.Contains("Bolts") ||
                (!(resource is Armor) && !(resource is Weapon) && !(resource is Tool)))
            {
                float resourceCountMax =
                    Mathf.Min(0.75f*trader.faction.storage.getStorageAvailable(resource.storageIndex)/resource.mass,
                        (float) playerCoin/(resource.value*traderInfluence));
                resourceCount = (int) (resourceCountMax*UnityEngine.Random.Range(0.5f, 1f));
                if (resourceCount < 0)
                {
                    resourceCount *= -1;
                }
            }
            int resourceValue = Mathf.CeilToInt(resource.value * traderInfluence);

            Transaction transaction = new Transaction(false, resource, resourceCount == 1 ? 1 : resourceCount / 2, resourceCount, resourceValue);

            TradeWindowSellList.Add(transaction);
            resourcesSold.Add(resource);
        }

        private void BuildMerchantBuyTransaction()
        {
            if (AManager<ResourceManager>.getInstance().sellList.Count == 0) {
                //trader.setBubble("I have nothing to sell.");
                return;
            }
            Resource resource = AManager<ResourceManager>.getInstance().sellList.RandomElement<Resource>();
            if (trader.faction.storage[resource] == 0 || resource.index == 55) {
                return;
            }
            if (resourcesBought.Contains(resource)) {
                return;
            }
            if (UnityEngine.Random.value < 0.3f) {
                //this.unit.setBubble("I'm afraid I have no use for " + resource.name + " at this time.");
                resourcesBought.Add(resource);
                return;
            }
            int resourceCountMin = Mathf.CeilToInt((float)trader.faction.storage[resource] * 0.5f);
            int resourceCountMax = Mathf.CeilToInt((float)trader.faction.storage[resource] * UnityEngine.Random.Range(0.5f, 0.9f));
            int resourceCount = UnityEngine.Random.Range(resourceCountMin, resourceCountMax + 1);
            int traderLevel = trader.getProfession().getLevel();
            if (trader.preferences["trait.charismatic"]) {
                traderLevel = Mathf.Min(traderLevel + 5, 20);
            }
            float traderInfluence = traderLevel * Mathf.Min(1f, (traderLevel / 20) + 0.2f);
            merchantCoin += Mathf.CeilToInt((resource.value*traderInfluence*resourceCount)/3);
            int resourceValue = Mathf.CeilToInt(resource.value * traderInfluence);

            Transaction transaction = new Transaction(true, resource, resourceCount == 1 ? 1 : resourceCount / 2, resourceCount, resourceValue);

            TradeWindowBuyList.Add(transaction);
            resourcesBought.Add(resource);
        }

        public static string FormatNumberPostfix(int number)
        {
            int num = (number != 0) ? ((int)Math.Log10((double)Math.Abs(number))) : 0;
            if (num < 4) {
                return number.ToString();
            }
            double num2 = (double)((int)((double)number / Math.Pow(10.0, (double)(num - 2))));
            return (num2 * Math.Pow(10.0, (double)(num % 3 - 2))).ToString() + PostFixArray[num / 3 - 1];
        }

        private static string[] PostFixArray = new string[]
        {
	        "k",
	        "M",
	        "B",
	        "T"
        };
    }

    public class Transaction : Trade
    {
        public int maxAmount;

        public bool completeTransaction;

        public float storageNeeded;

        public float storageAmountNeededForTransaction { get { return resource == null ? 0 : amount * resource.mass; } }

        public int totalValue
        {
            get { return amount * value; }
        }

        public Transaction(bool playerSelling, Resource resource, int amount, int maxAmount, int value) : base(playerSelling, resource, amount, value)
        {
            this.maxAmount = maxAmount;
        }
    }
}
