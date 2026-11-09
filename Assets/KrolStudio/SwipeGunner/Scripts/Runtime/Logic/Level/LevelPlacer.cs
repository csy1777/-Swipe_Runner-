using UnityEngine;
using UnityEngine.Splines;
using Zenject;

namespace KrolStudio
{
    public interface ILevelPlacer
    {
        Level CurrentLevel { get; }
        void LoadLevel();
        SplineContainer GetPathSpline { get; }
    }

    public class LevelPlacer : ILevelPlacer, IProgressSaver, IProgressReader
    {
        readonly LevelList levelList;
        readonly IHudView hudView;

        int currentLevelIndex;
        public int CurrentLevelIndex 
        { 
            get
            {
                return currentLevelIndex;
            }
            private set
            {
                currentLevelIndex = value;
            }
        }

        public Level CurrentLevel { get; private set; }

        public SplineContainer GetPathSpline => CurrentLevel.playerPathSpline;

        [Inject]
        public LevelPlacer(LevelList levelList, IHudView hudView, ISaveLoadService saveLoadService)
        {
            this.levelList = levelList;
            this.hudView = hudView;

            saveLoadService.Register(SaveKey.LevelPlacer, this);
        }

        public void IncrementLevelIndex() => CurrentLevelIndex++;

        public void LoadLevel()
        {
            hudView.UpdateLevelIndicator(CurrentLevelIndex);

            RemovePrevious();
            Spawn();
        }

        private void RemovePrevious()
        {
            if (CurrentLevel != null)
            {
                Object.Destroy(CurrentLevel.gameObject);
            }
        }

        private void Spawn()
        {
            CurrentLevel = Object.Instantiate(levelList.GetLevel((CurrentLevelIndex - 1) % levelList.Count));
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.CurrentLevelIndex = CurrentLevelIndex;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            CurrentLevelIndex = progress.CurrentLevelIndex;
        }
    }
}