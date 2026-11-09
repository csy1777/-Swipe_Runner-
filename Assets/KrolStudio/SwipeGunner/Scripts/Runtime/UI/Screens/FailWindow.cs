using Zenject;

namespace KrolStudio
{
    public class FailWindow : WindowBase
    {
        ISceneLoader sceneLoader;
        ISaveLoadService saveLoadService;
        AdsManager adsManager;

        bool adWasShown;

        [Inject]
        void Construct(GameplayStateMachine gameplayStateMachine, 
                        ISceneLoader sceneLoader, 
                        ISaveLoadService saveLoadService,
                        AdsManager adsManager)
        {
            this.sceneLoader = sceneLoader;
            this.saveLoadService = saveLoadService;
            this.adsManager = adsManager;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            saveLoadService.SaveProgress(SaveKey.All);
        }

        protected override void CloseWindow()
        {
            if (!adWasShown && adsManager.Settings.InterstitialType != AdProvider.Disable)
            {
                adWasShown = true;
                adsManager.ShowInterstitial(null, true);
            }
            else
            {
                Restart();
            }
        }

        void Restart()
        {
            sceneLoader.Load(SceneName.Level.ToString());
        }

        protected override void Cleanup() { }

        protected override void Initialize() { }

        protected override void SubscribeUpdates() { }
    }
}
