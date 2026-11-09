using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace KrolStudio
{
    [Serializable]
    public struct SoundParameters
    {
        [Range(0, 1)]
        public float Volume;
        [Range(-3, 3)]
        public float Pitch;
        public bool Loop;
    }

    [Serializable]
    public class Sound
    {
        [SerializeField] string name = string.Empty;
        [SerializeField] SoundType soundType;
        [SerializeField] SoundPlayMode soundPlayMode;
        public SoundType SoundType => soundType;
        public SoundPlayMode SoundPlayMode => soundPlayMode;

        public string Name => name;

        [SerializeField] AudioClip clip = null;
        public AudioClip Clip => clip;
        
        public SoundParameters parameters = new SoundParameters();
        
        [HideInInspector]
        public AudioSource Source = null;

        private CtsHelper ctsHelper = new();

        public void PlayOneShot()
        {
            if (Source == null) return;

            Source.volume = parameters.Volume;
            Source.pitch = parameters.Pitch;
            Source.loop = parameters.Loop;

            Source.PlayOneShot(Clip);
        }

        public void Play()
        {
            if (Source == null) return;

            Source.clip = Clip;

            Source.volume = parameters.Volume;
            Source.pitch = parameters.Pitch;
            Source.loop = parameters.Loop;

            Source.Play();
        }

        public void Pause(bool pause)
        {
            if (Source == null) return;
            if (pause)
                Source.Pause();
            else
                Source.UnPause();
        }

        public void Stop()
        {
            if (Source == null) return;
            Source.Stop();
        }

        public void StartVolumeFade(float from, float to, float duration, Action action = null)
        {
            ctsHelper.Restart();
            FadeVolumeCoroutine(from, to, duration).Forget();
        }

        private async UniTask FadeVolumeCoroutine(float from, float to, float duration, Action action = null)
        {
            try
            {
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;

                    float t = Mathf.Clamp01(elapsed / duration);
                    parameters.Volume = Mathf.Lerp(from, to, t);

                    Source.volume = parameters.Volume;
                    Source.pitch = parameters.Pitch;
                    Source.loop = parameters.Loop;

                    await UniTask.NextFrame(ctsHelper.Token);
                }

                Source.volume = parameters.Volume;
                Source.pitch = parameters.Pitch;
                Source.loop = parameters.Loop;

                Dispose();

                action?.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"The object was destroyed during animation: {ex.Message}");
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"Missing link during animation: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex}");
            }
        }

        public void Dispose()
        {
            ctsHelper.Dispose();
        }
    }

    public class AudioManager : MonoBehaviour
    {
        [SerializeField] Sound[] sounds = null;
        [SerializeField] AudioSource musicSourcePrefab = null;
        [SerializeField] AudioSource sfxSourcePrefab = null;

        [SerializeField] bool isSoundActive;

        public void InitSounds()
        {
            foreach (var sound in sounds)
            {
                if (sound.Source == null)
                {
                    sound.Source = CreateAudioSourceObject(sound);
                }
            }
        }

        Sound CreateSound(string name)
        {
            foreach (var sound in sounds)
            {
                if(sound.Name == name)
                {
                    sound.Source = CreateAudioSourceObject(sound);
                    return sound;
                }
            }

            return null;
        }

        AudioSource CreateAudioSourceObject(Sound sound)
        {
            var sourcePrefab = GetAudioSource(sound.SoundType);
            AudioSource source = Instantiate<AudioSource>(sourcePrefab, gameObject.transform);
            source.name = sound.Name;
            return source;
        }

        AudioSource GetAudioSource(SoundType soundType)
        {
            return soundType switch
            {
                SoundType.Music => musicSourcePrefab,
                SoundType.SFX => sfxSourcePrefab,
                _ => default
            };
        }

        public void PlaySoundFading(string name, float from, float to, float duration)
        {
            if (!isSoundActive) return;

            var sound = GetSound(name);
            if (sound != null)
            {
                sound.StartVolumeFade(from, to, duration);
                sound.Play();
            }
            else
            {
                LogWarning();
            }
        }

        public void PlaySound(string name)
        {
            if (!isSoundActive) return;

            var sound = GetSound(name);

            if (sound != null)
            {
                if(sound.SoundPlayMode == SoundPlayMode.Play)
                {
                    sound.Play();
                }
                else if(sound.SoundPlayMode == SoundPlayMode.PlayOneShot)
                {
                    sound.PlayOneShot();
                }
            }
            else
            {
                LogWarning();
            }

        }

        public void PauseMusic(bool pause)
        {
            var sound = GetSound(name);
            if (sound != null)
            {
                sound.Pause(pause);
            }
            else
            {
                LogWarning();
            }
        }

        public void StopSoundFading(string name, float from, float to, float duration)
        {
            if (!isSoundActive) return;

            var sound = GetSound(name);
            if (sound != null)
            {
                sound.StartVolumeFade(from, to, duration, sound.Stop);
            }
            else
            {
                LogWarning();
            }
        }

        public void StopSound(string name)
        {
            var sound = GetSound(name);
            if (sound != null)
            {
                sound.Stop();
            }
            else
            {
                LogWarning();
            }
        }

        public void SetActiveSounds(bool value)
        {
            isSoundActive = value;
        }

        public Sound GetSound(string name)
        {
            foreach (var sound in sounds)
            {
                if (sound.Name == name)
                {
                    return sound;
                }
            }

            return CreateSound(name);
        }

        private void OnDestroy()
        {
            foreach (var sound in sounds)
            {
                sound.Dispose();
            }
        }

        private void LogWarning()
        {
#if UNITY_EDITOR
            Debug.LogWarning("Sound by the name " + name + " is not found! Issues occured at AudioManager.StopSound()");
#endif
        }
    }
}