using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Zenject;

namespace KrolStudio
{
    public class LoadProgressState : IState
    {
        private readonly IPersistentProgressService progressService;
        private readonly ISaveLoadService saveLoadService;
        private readonly DefaultPlayerProgress defaultPlayerProgress;
        private readonly ISceneLoader sceneLoader;
        private readonly ILogService logService;

        [Inject]
        public LoadProgressState(IPersistentProgressService progressService,
                                 ISaveLoadService saveLoadService, 
                                 DefaultPlayerProgress defaultPlayerProgress,
                                 ISceneLoader sceneLoader,
                                 ILogService logService)
        {
            this.progressService = progressService;
            this.saveLoadService = saveLoadService;
            this.defaultPlayerProgress = defaultPlayerProgress;
            this.sceneLoader = sceneLoader;
            this.logService = logService;
        }

        public async UniTask Enter()
        {
            logService.Log($"Entered {nameof(LoadProgressState)}");

            LoadProgressOrInitNew();

            await sceneLoader.Load(SceneName.Level.ToString());
        }

        public UniTask Exit() => default;

        private PlayerProgress LoadProgressOrInitNew()
        {
            PlayerProgress progress = saveLoadService.LoadProgress();

            bool shouldRecreateProgress = progress == null || !progress.IsTutorialShown;

            progressService.Progress = shouldRecreateProgress ? NewProgress() : progress;

            if(shouldRecreateProgress) saveLoadService.SaveProgress(SaveKey.All);

            return progressService.Progress;
        }

        private PlayerProgress NewProgress()
        {
            logService.Log("Init new player progress.");

            PlayerProgress progress = new PlayerProgress();

            progress.CurrentLevelIndex = defaultPlayerProgress.currentLevelIndex;

            progress.BoardData = new BoardData(new Dictionary<int, PartSaveData>());
            progress.WalletsData = new WalletsData(GetWalletsData());
            progress.PlayerData = new PlayerData(GetPlayerData());

            progress.IncomeLevel = defaultPlayerProgress.incomeLevel;
            progress.AddPartlevel = defaultPlayerProgress.addPartlevel;

            progress.Vibrate = defaultPlayerProgress.vibrate;
            progress.MusicVolume = defaultPlayerProgress.musicVolume;
            progress.SfxVolume = defaultPlayerProgress.sfxVolume;

            progress.IsTutorialShown = false;

            progress.IsForcedAdEnabled = true;

            return progress;
        }

        Dictionary<PartType, int> GetPlayerData()
        {
            var partData = new Dictionary<PartType, int>();
            foreach (var item in defaultPlayerProgress.partSettings)
            {
                if(!partData.ContainsKey(item.type))
                {
                    partData.Add(item.type, item.level);
                }
            }
            return partData;
        }

        Dictionary<int, long> GetWalletsData()
        {
            var currency = new Dictionary<int, long>();
            foreach (var item in defaultPlayerProgress.currencySettings)
            {
                if (!currency.ContainsKey((int)item.type))
                {
                    currency.Add((int)item.type, item.count);
                }
            }
            return currency;
        }
    }
}