using Cysharp.Threading.Tasks;

namespace KrolStudio
{
    public interface ISceneLoader
    {
        UniTask Load(string nextScene, System.Action onLoaded = null);
    }
}