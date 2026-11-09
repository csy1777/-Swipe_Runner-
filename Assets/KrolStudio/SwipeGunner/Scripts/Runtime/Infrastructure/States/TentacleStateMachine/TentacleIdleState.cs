using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class TentacleIdleState : IState, IUpdatableState
    {
        readonly TentacleTriggerChecker triggerChecker;
        readonly ITentacleContext tentacleContext;
        readonly TentacleStateMachine stateMachine;
        readonly IPlayerContext playerContext;

        public float rotationSpeed;

        [Inject]
        public TentacleIdleState(TentacleTriggerChecker triggerChecker,
                                 ITentacleContext tentacleContext,
                                 TentacleStateMachine stateMachine,
                                 IPlayerContext playerContext,
                                 TentacleConfig config)
        {
            this.triggerChecker = triggerChecker;
            this.tentacleContext = tentacleContext;
            this.stateMachine = stateMachine;
            this.playerContext = playerContext;

            rotationSpeed = config.rotationSpeed;
        }

        public UniTask Enter()
        {
            return default;
        }

        public UniTask Exit()
        {
            return default;
        }

        void LookAtPlayerY()
        {
            Vector3 direction = playerContext.Transform.position - tentacleContext.Transform.position;
            direction.y = 0f; // Zero out Y so that rotation occurs only on the horizontal plane

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                tentacleContext.Transform.rotation = Quaternion.Slerp(tentacleContext.Transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }

        public void Update()
        {
            LookAtPlayerY();

            if (triggerChecker.CanTriggered())
            {
                stateMachine.Enter<TentacleAttackState>().Forget();
                return;
            }
        }
    }
}
