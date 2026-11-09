using UnityEngine;

namespace KrolStudio
{
    public class ProjectileShooter
    {
        private readonly float angleInDegrees;

        public ProjectileShooter(float angleInDegrees)
        {
            this.angleInDegrees = angleInDegrees;
        }

        private float CalculateFlightTime(float speed, float angleRad, float heightDifference)
        {
            float vy = speed * Mathf.Sin(angleRad);
            float g = -Physics.gravity.y;

            // Quadratic equation: h = vy * t - 0.5 * g * t^2
            // Solve for the time to reach height h (there may be 2 roots — take the greater one)
            float discriminant = vy * vy + 2 * g * heightDifference;
            if (discriminant < 0f) return 0f;

            float t1 = (vy + Mathf.Sqrt(discriminant)) / g;
            float t2 = (vy - Mathf.Sqrt(discriminant)) / g;

            return Mathf.Max(t1, t2);
        }

        private Vector3 ApplyRandomConeDeviation(Vector3 axis, float maxAngleDeg)
        {
            float maxAngleRad = maxAngleDeg * Mathf.Deg2Rad;

            // Generate a random direction within a cone around the original axis
            float z = Mathf.Cos(Random.Range(0f, maxAngleRad));
            float theta = Random.Range(0f, 2f * Mathf.PI);
            float r = Mathf.Sqrt(1f - z * z);

            Vector3 localDir = new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), z);
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, axis.normalized);

            return rot * localDir;
        }

        public void Shoot(Rigidbody projectile, Vector3 origin, Vector3 target, int rotationCount = 1, bool enableRotation = true)
        {
            Vector3 fromTo = target - origin;
            Vector3 dirXZ = new Vector3(fromTo.x, 0, fromTo.z).normalized;

            float angleRad = angleInDegrees * Mathf.Deg2Rad;

            Vector3 shootDirection = Quaternion.AngleAxis(angleInDegrees, Vector3.Cross(dirXZ, Vector3.up)) * dirXZ;

            float speed = CalculateShotSpeed(origin, target, angleRad);
            projectile.linearVelocity = shootDirection * speed;

            // Calculate flight time
            float flightTime = CalculateFlightTime(speed, angleRad, fromTo.y);

            if (enableRotation && flightTime > 0f)
            {
                // Rotation axis: take any axis orthogonal to the direction of movement
                Vector3 rotationAxis = Vector3.Cross(shootDirection, Vector3.up).normalized;

                // If shootDirection is almost vertical, use a different vector for the cross product
                if (rotationAxis == Vector3.zero)
                    rotationAxis = Vector3.Cross(shootDirection, Vector3.right).normalized;

                // Angular speed: 2PI * N rotations over the flight time
                float angularSpeed = (2f * Mathf.PI * rotationCount) / flightTime;

                projectile.angularVelocity = rotationAxis * angularSpeed;
            }

            projectile.isKinematic = false;
        }

        public void Shoot(
             Rigidbody projectile,
             Transform origin,
             Transform target,
             float rotationCount = 1f,
             bool enableRotation = true,
             Vector3? customRotationAxis = null,
             float rotationAxisDeviationAngle = 0f)
        {
            Vector3 fromTo = target.position - origin.position;
            Vector3 dirXZ = new Vector3(fromTo.x, 0, fromTo.z).normalized;

            float angleRad = angleInDegrees * Mathf.Deg2Rad;
            Vector3 shootDirection = Quaternion.AngleAxis(angleInDegrees, Vector3.Cross(dirXZ, Vector3.up)) * dirXZ;

            float speed = CalculateShotSpeed(origin.position, target.position, angleRad);
            projectile.linearVelocity = shootDirection * speed;

            float flightTime = CalculateFlightTime(speed, angleRad, fromTo.y);

            if (enableRotation && flightTime > 0f)
            {
                Vector3 rotationAxis;

                if (customRotationAxis.HasValue)
                {
                    rotationAxis = customRotationAxis.Value.normalized;
                }
                else
                {
                    rotationAxis = Vector3.Cross(shootDirection, Vector3.up).normalized;
                    if (rotationAxis == Vector3.zero)
                        rotationAxis = Vector3.Cross(shootDirection, Vector3.right).normalized;
                }

                // Random deviation of the rotation axis (if specified)
                if (rotationAxisDeviationAngle > 0f)
                {
                    rotationAxis = ApplyRandomConeDeviation(rotationAxis, rotationAxisDeviationAngle);
                }

                float angularSpeed = (2f * Mathf.PI * rotationCount) / flightTime;
                projectile.angularVelocity = rotationAxis * angularSpeed;
            }

            projectile.isKinematic = false;
        }


        private float CalculateShotSpeed(Vector3 from, Vector3 to, float angleRad)
        {
            Vector3 fromTo = to - from;
            float x = new Vector3(fromTo.x, 0f, fromTo.z).magnitude;
            float y = fromTo.y;

            float denominator = 2f * (y - Mathf.Tan(angleRad) * x);
            if (Mathf.Approximately(denominator, 0f)) return 0f;

            float vSquared = (Physics.gravity.y * x * x) / (denominator * Mathf.Pow(Mathf.Cos(angleRad), 2f));
            return Mathf.Sqrt(Mathf.Abs(vSquared));
        }
    }
}