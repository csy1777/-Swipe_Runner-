using Cysharp.Threading.Tasks;
using System.Security.Cryptography;
using Zenject;

namespace KrolStudio
{
    public class GameplayBootstrapState : IState
    {
        readonly GameplayStateMachine gameplayStateMachine;

        readonly ILogService logService;
        readonly IInteractorPoolContainer interactorPoolContainer;
        readonly GameplayAssetsConfig gameplayAssetsConfig;
        readonly IGameFactory gameFactory;

        [Inject]
        public GameplayBootstrapState(GameplayStateMachine gameplayStateMachine, 
                                      ILogService logService, 
                                      IInteractorPoolContainer interactorPoolContainer,
                                      GameplayAssetsConfig gameplayAssetsConfig,
                                      IGameFactory gameFactory,
                                      AdsManager adsManager)
        {
            this.gameplayStateMachine = gameplayStateMachine;
            this.logService = logService;
            this.interactorPoolContainer = interactorPoolContainer;
            this.gameplayAssetsConfig = gameplayAssetsConfig;
            this.gameFactory = gameFactory;
        }

        public UniTask Enter()
        {
            logService.Log($"Entered {nameof(GameplayBootstrapState)}");

            CreatePoolContainers();

            gameplayStateMachine.Enter<InitGameplayState>().Forget();

            return default;
        }

        void CreatePoolContainers()
        {
            CreateDamagePopupPoolContainer();
            CreateEnemyKillRewardTextPoolContainer();
            CreateProjectilePoolContainer();
            CreateEnemyPoolContainer();
            CreateTentaclePoolContainer();
        }

        void CreateDamagePopupPoolContainer()
        {
            var factory = new ObjectFactory<DamagePopup>(gameFactory);
            interactorPoolContainer.CreatePool<DamagePopup, ObjectFactory<DamagePopup>>(factory, gameplayAssetsConfig.initialDamagePopupPoolCount, "DamagePopup");
        }

        void CreateEnemyKillRewardTextPoolContainer()
        {
            var factory = new ObjectFactory<EnemyKillRewardText>(gameFactory);
            interactorPoolContainer.CreatePool<EnemyKillRewardText, ObjectFactory<EnemyKillRewardText>>(factory, gameplayAssetsConfig.initialEnemyKillRewardTextPoolCount, "EnemyKillRewardText");
        }

        void CreateProjectilePoolContainer()
        {
            var factory = new ObjectFactory<Projectile>(gameFactory);
            interactorPoolContainer.CreatePool<Projectile, ObjectFactory<Projectile>>(factory, gameplayAssetsConfig.initialProjectilePoolCount, "Projectile");
        }

        void CreateEnemyPoolContainer()
        {
            var factory = new ObjectFactory<Enemy>(gameFactory);
            interactorPoolContainer.CreatePool<Enemy, ObjectFactory<Enemy>>(factory, gameplayAssetsConfig.initialEnemyPoolCount, "Enemy");
        }

        void CreateTentaclePoolContainer()
        {
            var factory = new ObjectFactory<Tentacle>(gameFactory);
            interactorPoolContainer.CreatePool<Tentacle, ObjectFactory<Tentacle>>(factory, gameplayAssetsConfig.initialTentaclePoolCount, "Tentacle");
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}