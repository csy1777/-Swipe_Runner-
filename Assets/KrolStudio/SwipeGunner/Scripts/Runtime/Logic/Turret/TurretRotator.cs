using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public interface ITurretRotator
    {
        void Subscribe();
        void Unsubscribe();
    }

    public class TurretRotator : ITurretRotator
    {
        readonly IInputService inputService;
        
        TurretConfig config;
        Transform rotatingPart;
        float currentRotationY;
        Vector3 localEulerAngles;

        [Inject]
        public TurretRotator(IPlayerContext playerContext, 
                             IInputService inputService, 
                             TurretConfig config)
        {
            this.inputService = inputService;
            this.config = config;
            rotatingPart = playerContext.Turret.RotatingPart;

            if (rotatingPart != null)
            {
                localEulerAngles = rotatingPart.localEulerAngles;
                localEulerAngles.y = 0f;
            }

            currentRotationY = 0f;  //NormalizeAngle(turret.localEulerAngles.y);
        }

        public void Subscribe()
        {
            inputService.OnSwipe += RotateByInput;
        }

        public void Unsubscribe()
        {
            inputService.OnSwipe -= RotateByInput;
        }

        public void RotateByInput(float deltaX)
        {
            if (rotatingPart != null)
            {
                float deltaRotation = (config.inverseRotation ? 1 : -1) * deltaX * config.rotationSensitivity;
                currentRotationY -= deltaRotation;
                currentRotationY = Mathf.Clamp(currentRotationY, config.minRotationAngle, config.maxRotationAngle);
            
                rotatingPart.localEulerAngles = localEulerAngles + new Vector3(0f, currentRotationY, 0f);
            }
        }

        // Normalize angle in composition -180..180
        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}