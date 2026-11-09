
namespace KrolStudio
{
    public interface IProgressSaver : IProgressReader
    {
        void UpdateProgress(PlayerProgress progress);
    }
}
