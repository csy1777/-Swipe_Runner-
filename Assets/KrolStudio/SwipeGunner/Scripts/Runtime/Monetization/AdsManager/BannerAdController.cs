
namespace KrolStudio
{
    public class BannerAdController
    {
        private AdsConfig config;
        private AdProviderRegistry registry;
        private IPersistentProgressService progressService;
        private bool isBannerActive = true;

        public void Initialize(AdsConfig config, AdProviderRegistry registry, IPersistentProgressService progressService)
        {
            this.config = config;
            this.registry = registry;
            this.progressService = progressService;
        }

        public void Show()
        {
            if (!isBannerActive)
                return;

            var handler = registry.GetHandler(config.BannerType);

            if (handler == null ||
                !progressService.Progress.IsForcedAdEnabled ||
                !handler.IsInitialised())
                return;

            handler.ShowBanner();
        }

        public void Hide()
        {
            var handler = registry.GetHandler(config.BannerType);

            if (handler == null || !handler.IsInitialised())
                return;

            handler.HideBanner();
        }

        public void Destroy()
        {
            var handler = registry.GetHandler(config.BannerType);

            if (handler == null || !handler.IsInitialised())
                return;

            handler.DestroyBanner();
        }

        public void Enable()
        {
            isBannerActive = true;
            Show();
        }

        public void Disable()
        {
            isBannerActive = false;
            Hide();
        }
    }
}