using UnityEngine;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KrolStudio
{
    public class Rocket : MonoBehaviour
    {
        [SerializeField] ParticleSystem[] launchParticles;
        [SerializeField] ParticleSystem explosionPrefab;
        [SerializeField] AudioManager audioManager;

        [SerializeField] RocketConfig config;
        private Transform parent;

        //private Collider[] results;

        IPlayerContext playerContext;

        public void Initialize(RocketConfig config, IPlayerContext playerContext)
        {
            this.config = config;
            //results = new Collider[config.maxExplodeTargets];

            this.playerContext = playerContext;

            audioManager.InitSounds();
        }

        public void Launch(Transform target)
        {
            audioManager.PlaySound(GameConstants.Sounds.RocketLaunch);

            parent = transform.parent;
            transform.SetParent(null);

            Vector3 startPos = transform.localPosition;

            // Point after ascending upwards
            Vector3 upPos = startPos + Vector3.up * config.upDistance;

            // Intermediate point for the arc
            Vector3 midPoint = (upPos + target.position) / 2f;
            midPoint.y += config.arcHeight;

            // Full path: up -> arc -> target
            Vector3[] path = new Vector3[]
            {
                upPos,
                midPoint,
                target.position
            };

            transform.DOPath(path, config.upTime + config.flyTime, PathType.CatmullRom, PathMode.Full3D)
                 .SetEase(Ease.InOutSine)
                 .SetLookAt(0.01f)
                 .OnStart(() => {
                     LaunchParticles(true);
                     transform.SetParent(null);
                 })
                 .OnComplete(Explode);
        }

        void LaunchParticles(bool value)
        {
            foreach (var item in launchParticles)
            {
                if(value)
                    item.Play(true);
                else
                    item.Stop();
            }
        }

        void Explode()
        {
            LaunchParticles(false);
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            audioManager.PlaySound(GameConstants.Sounds.RocketExplosion);
            ApplyDamage();

            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void ApplyDamage()
        {
            // Get all colliders inside the sphere
            Collider[] colliders = Physics.OverlapSphere(
                 transform.position,
                 config.baseRadius,
                 config.detectableTargets,              // LayerMask
                 QueryTriggerInteraction.Collide        
             );

            foreach (Collider hit in colliders)
            {
                if (hit.gameObject == gameObject) continue; // Skip self (ignore the collider of the current object)

                HandleAttack(hit);
            }
        }

        private void HandleAttack(Collider hit)
        {
            if (hit.TryGetComponent<IHealth>(out var healthHandler))
            {
                Attack(healthHandler);
            }
        }

        private void Attack(IHealth entity)
        {
            entity.TakeDamage(playerContext.PlayerStats.RocketDamage, this, new DamageDirectionEventArgs(playerContext.PlayerStats.RocketDamage, Vector3.up));
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            // Draw only if this specific object is selected
            if (Selection.activeGameObject != gameObject)
                return;
#endif
            if (config == null)
            {
                Debug.LogWarning($"{name}: Config not set!", this);
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, config.baseRadius);
        }
#endif
    }
}