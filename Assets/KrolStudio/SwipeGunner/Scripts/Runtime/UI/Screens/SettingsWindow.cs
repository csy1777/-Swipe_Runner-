using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Zenject;

namespace KrolStudio
{
    public class SettingsWindow : WindowBase, IProgressSaver, IProgressReader
    {
        [SerializeField] Button RestartButton;

        [SerializeField] AudioMixer audioMixer;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Toggle vibrate;

        ISceneLoader sceneLoader;
        PauseManager pauseManager;
        ISaveLoadService saveLoadService;
        IPersistentProgressService progressService;
        PlayerStateMachine playerStateMachine;

        [Inject]
        void Construct(PauseManager pauseManager, 
                       IPersistentProgressService progressService,
                       ISaveLoadService saveLoadService,
                       ISceneLoader sceneLoader,
                       PlayerStateMachine playerStateMachine)
        {
            this.sceneLoader = sceneLoader;
            this.pauseManager = pauseManager;
            this.saveLoadService = saveLoadService;
            this.progressService = progressService;
            this.playerStateMachine = playerStateMachine;

            saveLoadService.Register(SaveKey.SettingsWindow, this);
            LoadProgress(progressService.Progress);
        }

        public void SetVibrate(bool value)
        {
            progressService.Progress.Vibrate = value;
        }

        public void SetMusicVolume()
        {
            float volume = musicSlider.value;
            audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        }

        public void SetSFXVolume()
        {
            float volume = sfxSlider.value;
            audioMixer.SetFloat("Sfx", Mathf.Log10(volume) * 20);
        }

        void Restart()
        {
            SaveKey key = SaveKey.SettingsWindow;

            if (progressService.Progress?.IsTutorialShown == true &&
                playerStateMachine.IsCurrentState(typeof(PlayerIdleState)))
            {
                // If the tutorial happens, we'll save everything.
                // This is convenient right now. In other cases, an additional concrete construct may be used.
                key = SaveKey.All;
            }

            saveLoadService.SaveProgress(key);

            sceneLoader.Load(SceneName.Level.ToString());
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            RestartButton.interactable = true;
            RestartButton.onClick.AddListener(Restart);

            pauseManager.Pause();          
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            RestartButton.onClick.RemoveListener(Restart);
            pauseManager.Resume();
        }

        protected override void CloseWindow() // Resume Button
        {
            base.CloseWindow();
            RestartButton.interactable = false;

            if (progressService.Progress != null && progressService.Progress.IsTutorialShown)
            {
                saveLoadService.SaveProgress(SaveKey.All);
            }
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.Vibrate = vibrate.isOn;
            progress.MusicVolume = musicSlider.value;
            progress.SfxVolume = sfxSlider.value;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            musicSlider.value = progress.MusicVolume;
            SetMusicVolume();

            sfxSlider.value = progress.SfxVolume;
            SetSFXVolume();

            vibrate.isOn = progress.Vibrate;
            SetVibrate(progress.Vibrate);
        }

        protected override void Cleanup() { }

        protected override void Initialize() { }

        protected override void SubscribeUpdates() { }

    }
}
