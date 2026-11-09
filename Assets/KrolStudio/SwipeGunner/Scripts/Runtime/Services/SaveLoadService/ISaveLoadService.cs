using System.Collections.Generic;

namespace KrolStudio
{
    public interface ISaveLoadService
    {
        void SaveProgress(SaveKey key);
        PlayerProgress LoadProgress();
        void Register(SaveKey key, IProgressSaver instance);
        IEnumerable<IProgressReader> ProgressReader();

        //void Cleanup();
    }
}