
namespace KrolStudio
{
    public interface IWindowService
    {
        WindowBase GetWindow(WindowType windowType);
        void Open(WindowType windowType, bool overlayPage = false);
        void Close();
    }
}