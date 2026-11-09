
namespace KrolStudio
{
    public interface IWindowFactory
    {
        T CreateWindow<T>(WindowType windowType) where T : WindowBase;
    }
}