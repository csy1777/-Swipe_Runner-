
namespace KrolStudio
{
    public interface IGameFactory
    {
        Projectile CreateProjectile();
        Enemy CreateEnemy();
        Tentacle CreateTentacle();
        DamagePopup CreateDamagePopup();
        EnemyKillRewardText CreateEnemyKillRewardText();
        UpgradePartController CreatePart();
    }
}