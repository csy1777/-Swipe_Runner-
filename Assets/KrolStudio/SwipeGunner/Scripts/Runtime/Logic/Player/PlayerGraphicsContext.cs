using UnityEngine;

namespace KrolStudio
{
    public interface IPlayerGraphicsContext
    {
        ExplosionEffect ExplosionEffect { get; }
        UpgradeLevelController UpgradeLevel { get; }
        DamageFlashEffect DamageFlashEffect { get; }
        Turret Turret { get; }
        RocketLauncher RocketLauncher { get; }
        Collider PlayerGraphicsCollider { get; }

    }

    [RequireComponent(typeof(ExplosionEffect))]
    [RequireComponent(typeof(UpgradeLevelController))]
    [RequireComponent(typeof(DamageFlashEffect))]
    public class PlayerGraphicsContext : MonoBehaviour, IPlayerGraphicsContext
    {
        [SerializeField] Turret turret;
        [SerializeField] RocketLauncher rocketLauncher;
        [SerializeField] Collider playerGraphicsCollider;

        ExplosionEffect explosionEffect;
        UpgradeLevelController upgradeLevel;
        DamageFlashEffect damageFlashEffect;

        public Turret Turret => turret;
        public RocketLauncher RocketLauncher => rocketLauncher;
        public ExplosionEffect ExplosionEffect => explosionEffect;
        public UpgradeLevelController UpgradeLevel => upgradeLevel;
        public DamageFlashEffect DamageFlashEffect => damageFlashEffect;
        public Collider PlayerGraphicsCollider => playerGraphicsCollider;

        void Awake()
        {
            explosionEffect = GetComponent<ExplosionEffect>();
            upgradeLevel = GetComponent<UpgradeLevelController>();
            damageFlashEffect = GetComponent<DamageFlashEffect>();
        }
    }
}