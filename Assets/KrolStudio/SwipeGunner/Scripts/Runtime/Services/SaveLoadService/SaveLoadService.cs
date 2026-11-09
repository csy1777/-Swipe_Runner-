using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class SaveLoadService : ISaveLoadService
    {
        const string ProgressKey = "Progress";

        private readonly IPersistentProgressService persistentProgressService;
        private Dictionary<SaveKey, IProgressSaver> saverService = new();
        
        [Inject]
        public SaveLoadService(IPersistentProgressService persistentProgressService)
        {
            this.persistentProgressService = persistentProgressService;
        }

        public IEnumerable<IProgressReader> ProgressReader()
        {
            RemoveNullValues(saverService);
            return saverService.Values.OfType<IProgressReader>();
        }

        public void Register(SaveKey key, IProgressSaver instance)
        {
            if(!saverService.ContainsKey(key))
                saverService.Add(key, instance);
            else
                saverService[key] = instance;
        }

        //public void Cleanup()
        //{
        //    saverService.Clear();
        //}

        public PlayerProgress LoadProgress()
        {
            return PlayerPrefs.GetString(ProgressKey)?.ToDeserialized<PlayerProgress>();
        }

        public void SaveProgress(SaveKey key = SaveKey.All)
        {
            RemoveNullValues(saverService);

            foreach (var saver in saverService)
            {
                if ((key & saver.Key) != 0)
                    saver.Value.UpdateProgress(persistentProgressService.Progress);
            }    

            PlayerPrefs.SetString(ProgressKey, persistentProgressService.Progress.ToJson());
        }

        private void RemoveNullValues<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
            where TValue : class
        {
            List<TKey> keysToRemove = new();

            foreach (var pair in dictionary)
            {
                if (pair.Value == null || pair.Value.Equals(null)) // processing Unity's fake null
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                dictionary.Remove(key);
            }
        }
    }
}
