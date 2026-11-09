using Zenject;

namespace KrolStudio
{
    public class MovementTracker
    {
        float moveSpeed;
        float startTime;
        float travelDistance;
        float splineLength;

        IPlayerContext playerContext;

        [Inject]
        void Construct(IPlayerContext playerContext)
        {
            this.playerContext = playerContext;
            splineLength = playerContext.SplineMover.spline.CalculateLength();
        }

        public float Distance()
        {
            if (playerContext.SplineMover != null)
            {
                float normalizedTime = playerContext.SplineMover.NormalizedTime;
                var splineContainer = playerContext.SplineMover.spline;

                float traveledDistance = normalizedTime * splineLength;

                return traveledDistance;
            }

            return 0f;
        }

        public float Progress => playerContext.SplineMover != null ? playerContext.SplineMover.NormalizedTime : 0f;
    }
}