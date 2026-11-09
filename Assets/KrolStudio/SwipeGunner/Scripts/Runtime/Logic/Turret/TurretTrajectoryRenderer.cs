using Zenject;

namespace KrolStudio
{
    public interface ITrajectoryRenderer
    {
        void SetActive(bool active);
        void RenderLaser();
    }

    public class TurretTrajectoryRenderer : ITrajectoryRenderer
    {
        IPlayerContext playerContext;
        float laserMaxDistance;

        [Inject]
        public TurretTrajectoryRenderer(IPlayerContext playerContext, TurretConfig config)
        {
            this.playerContext = playerContext;
            laserMaxDistance = config.laserMaxDistance;
        }

        public void SetActive(bool active)
        {
            playerContext.Turret.SetActive(active);
        }

        public void RenderLaser()
        {
            playerContext.Turret.RenderLaser(laserMaxDistance);
        }
    }
}