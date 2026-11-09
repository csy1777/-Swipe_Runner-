using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Zenject;

namespace KrolStudio
{
    public class SceneLoader : ISceneLoader
    {
        private readonly LoadingCurtainProvider loadingCurtain;
        private readonly ISaveLoadService saveLoadService;

        [Inject]
        public SceneLoader(LoadingCurtainProvider loadingCurtain, ISaveLoadService saveLoadService)
        {
            this.loadingCurtain = loadingCurtain;
            this.saveLoadService = saveLoadService;
        }

        public async UniTask Load(string name, System.Action onLoaded = null)
        {
            // Test code. Delete it
            //if (SceneManager.GetActiveScene().name == name)
            //    saveLoadService.Cleanup();

            loadingCurtain.Instance.Show();

            await SceneManager.LoadSceneAsync(name).ToUniTask();
            onLoaded?.Invoke();
        }
    }
}