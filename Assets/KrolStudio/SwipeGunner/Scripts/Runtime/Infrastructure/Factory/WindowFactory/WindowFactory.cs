using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class WindowFactory : IWindowFactory
    {
        private readonly IInstantiator instantiator;
        private readonly IPersistentProgressService progressService;
        private readonly WindowAssetsConfig windowAssetsConfig;
        private readonly Transform uiRoot;

        private Dictionary<WindowType, GameObject> windowAssets = new Dictionary<WindowType, GameObject>();

        [Inject]
        public WindowFactory(IInstantiator instantiator,
                            WindowAssetsConfig windowAssetsConfig,
                            [Inject(Id = "UIRoot")] Transform uiRoot,
                            IPersistentProgressService progressService)
        {
            this.windowAssetsConfig = windowAssetsConfig;
            this.instantiator = instantiator;
            this.uiRoot = uiRoot;
            this.progressService = progressService;

            windowAssets = windowAssetsConfig.WindowAssets.ToDictionary(k => k.WindowType, v => v.WindowPrefab);
        }

        public T CreateWindow<T>(WindowType windowType) where T : WindowBase
        {
            T instantiateWindow = Instantiate<T>(windowAssets[windowType], uiRoot);
            return instantiateWindow;
        }

        private T Instantiate<T>(GameObject prefab, Transform parent = null) where T : Object
        {
            GameObject instantiatePrefab = instantiator.InstantiatePrefab(prefab, parent);
            return instantiatePrefab.GetComponent<T>();
        }
    }
}