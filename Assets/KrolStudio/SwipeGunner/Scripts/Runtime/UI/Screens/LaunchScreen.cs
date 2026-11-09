using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace KrolStudio
{
    public class LaunchScreen : WindowBase
    {
        [SerializeField] Button addPart;
        [SerializeField] Button addIncome;
        [Space]
        [SerializeField] GameObject addPartDisabled;
        [SerializeField] GameObject addIncomeDisabled;
        [Space]
        [SerializeField] TextMeshProUGUI incomeCost;
        [SerializeField] TextMeshProUGUI upgradeCost;
        [Space]
        [SerializeField] TutorialHand tutorialHand;

        private GameplayStateMachine gameplayStateMachine;
        private BoardManager boardManager;
        private PlayerFx playerFx;
        private UpgradeSystem upgradeSystem;
        private IWalletService walletService;
        private ILogService logService;
        private IHudView hudView;
        private DefaultPlayerProgress defaultPlayerProgress;

        // Tutorial
        private IPersistentProgressService progressService;
        private TutorialConfig tutorialConfig;
        private int currentPartIndex;

        [Inject]
        private void Construct(GameplayStateMachine gameplayStateMachine,
                              IPlayerContext playerContext,
                              UpgradeSystem upgradeSystem,
                              IWalletService walletService,
                              ILogService logService,
                              IHudView hudView,
                              IPersistentProgressService progressService,
                              TutorialConfig tutorialConfig,
                              DefaultPlayerProgress defaultPlayerProgress)
        {
            this.gameplayStateMachine = gameplayStateMachine;
            this.boardManager = playerContext.BoardManager;
            this.playerFx = playerContext.PlayerFx;
            this.upgradeSystem = upgradeSystem;
            this.walletService = walletService;
            this.logService = logService;
            this.hudView = hudView;
            this.progressService = progressService;
            this.tutorialConfig = tutorialConfig;
            this.defaultPlayerProgress = defaultPlayerProgress;
        }

        private void Start()
        {
            UpdateIncomeCost();
            UpdateUpgradePartCost();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            addPart.onClick.AddListener(boardManager.AddPart);
            addIncome.onClick.AddListener(AddIncome);

            upgradeSystem.IncomeLevelIncreased += UpdateIncomeCost;
            upgradeSystem.UpgradeLevelIncreased += UpdateUpgradePartCost;

            boardManager.OnSetPartInteractable += SetAddPartButtonInteractable;

            boardManager.EvaluatePartPurchaseAvailability();
            EvaluateIncomePurchaseAvailability();

            PlayTutorial();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            addPart.onClick.RemoveListener(boardManager.AddPart);
            addIncome.onClick.RemoveListener(AddIncome);

            upgradeSystem.IncomeLevelIncreased -= UpdateIncomeCost;
            upgradeSystem.UpgradeLevelIncreased -= UpdateUpgradePartCost;

            boardManager.OnSetPartInteractable -= SetAddPartButtonInteractable;
        }

        private void PlayTutorial()
        {
            if (!progressService.Progress.IsTutorialShown)
            {
                //reset
                walletService.SetAmount(CurrencyType.Coins, GetDefaultCurrencyAmount(CurrencyType.Coins), null);

                boardManager.OnMergeSuccess += NextPartTutorial;
                boardManager.SetGarbageVisibility(false);

                CloseButton.interactable = false;
                SetAddIncomeButtonInteractable(false);

                NextPartTutorial();
            }
        }

        private long GetDefaultCurrencyAmount(CurrencyType currency)
        {
            foreach (var item in defaultPlayerProgress.currencySettings)
                if (item.type == currency)
                    return item.count;
            return 0;
        }

        private void StopTutorial()
        {
            Debug.Log("StopTutorial");
            tutorialHand.StopTutorial();
            CloseButton.onClick.RemoveListener(StopTutorial);
        }

        private void NextPartTutorial()
        {
            PartType partType = tutorialConfig.GetPartType(currentPartIndex);

            if (Enum.Equals(PartType.None, partType))
            {
                tutorialHand.PlayStartClick();

                CloseButton.onClick.AddListener(StopTutorial);

                SetAddPartButtonInteractable(false); 
                SetAddIncomeButtonInteractable(false);

                boardManager.OnMergeSuccess -= NextPartTutorial;
                boardManager.SetGarbageVisibility(true);

                CloseButton.interactable = true;

                //EvaluateIncomePurchaseAvailability();
                //boardManager.EvaluatePartPurchaseAvailability();

                addPart.onClick.RemoveAllListeners();
                addPart.onClick.AddListener(boardManager.AddPart);

                return;
            }

            boardManager.EvaluatePartPurchaseAvailability();

            addPart.onClick.RemoveAllListeners();
            addPart.onClick.AddListener(TapStepTutorial);
            addPart.onClick.AddListener(() =>
            {
                boardManager.AddPart(0, partType);
                SetAddPartButtonInteractable(false);
            });

            tutorialHand.PlayClick();
        }

        void TapStepTutorial()
        {
            tutorialHand.PlayTap();
            currentPartIndex++;
        }

        public void SetAddPartButtonInteractable(bool value)
        {
            addPart.interactable = value;
            addPartDisabled.SetActive(!value);
        }

        public void SetAddIncomeButtonInteractable(bool value)
        {
            addIncome.interactable = value;
            addIncomeDisabled.SetActive(!value);
        }

        private void UpdateIncomeCost()
        {
            incomeCost.text = NumberFormatter.Format(upgradeSystem.GetCurrentIncomeCost());
        }

        private void UpdateUpgradePartCost()
        {
            upgradeCost.text = NumberFormatter.Format(upgradeSystem.GetNextUpgradePartCost());
        }

        private bool TryPurchaseIncomeUpgrade()
        {
            if (walletService.TryPurchase(CurrencyType.Coins, upgradeSystem.GetCurrentIncomeCost(), this))
            {
                return true;
            }

            logService.LogWarning("Not enough money");
            return false;
        }

        private void AddIncome()
        {
            if (TryPurchaseIncomeUpgrade())
            {
                hudView.ShowIncomeUpMessage();
                upgradeSystem.IncreaseIncomeLevel();
                playerFx.PlayCoinsDirectional();
            }

            EvaluateIncomePurchaseAvailability();
        }

        void EvaluateIncomePurchaseAvailability()
        {
            SetAddIncomeButtonInteractable(walletService.HasEnough(CurrencyType.Coins, upgradeSystem.GetCurrentIncomeCost()));
        }

        protected override void CloseWindow()
        {
            base.CloseWindow();
            progressService.Progress.IsTutorialShown = true;
            gameplayStateMachine.Enter<FireModeSelectState>().Forget();
        }

        protected override void Cleanup() { }

        protected override void Initialize() { }

        protected override void SubscribeUpdates() { }
    }
}
