using Cysharp.Threading.Tasks;

namespace KrolStudio
{
    public interface IExitableState
    {
        UniTask Exit();
    }
}
