using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Cysharp.Threading.Tasks;

namespace KrolStudio
{
    public class FireModeWindow : WindowBase
    {
        [SerializeField] Button singleButton;
        [SerializeField] Button burstButton;
        [SerializeField] Button autoButton;
        [SerializeField] TMP_Text titleText;

        IPersistentProgressService progressService;
        GameplayStateMachine gameplayStateMachine;
        ISaveLoadService saveLoadService;

        [Inject]
        void Construct(
            IPersistentProgressService progressService,
            GameplayStateMachine gameplayStateMachine,
            ISaveLoadService saveLoadService)
        {
            this.progressService = progressService;
            this.gameplayStateMachine = gameplayStateMachine;
            this.saveLoadService = saveLoadService;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            singleButton.onClick.AddListener(SelectSingle);
            burstButton.onClick.AddListener(SelectBurst);
            autoButton.onClick.AddListener(SelectAuto);
        }

        protected override void OnDisable()
        {
            singleButton.onClick.RemoveListener(SelectSingle);
            burstButton.onClick.RemoveListener(SelectBurst);
            autoButton.onClick.RemoveListener(SelectAuto);

            base.OnDisable();
        }

        protected override void Initialize() { }

        protected override void SubscribeUpdates() { }

        protected override void Cleanup() { }

        void SelectSingle() => Select(FireMode.Single);
        void SelectBurst()  => Select(FireMode.Burst);
        void SelectAuto()   => Select(FireMode.Auto);

        void Select(FireMode mode)
        {
            progressService.Progress.SelectedFireMode = mode;
            saveLoadService.SaveProgress(SaveKey.All);
            CloseWindow();
        }

        protected override void CloseWindow()
        {
            base.CloseWindow();
            gameplayStateMachine.Enter<TutorialState>().Forget();
        }
    }
}