using UnityEngine;
using UnityEditor;

namespace KrolStudio
{
    [CustomEditor(typeof(ExplosionEffect))]
    public class FragmentExplosion : Editor
    {
        private ExplosionEffect effect;

        public void OnEnable()
        {
            effect = target as ExplosionEffect;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("ShowTrajectory"))
            {
                effect.CreateTargets();
            }

            if (GUILayout.Button("ClearTrajectory"))
            {
                effect.ClearTargets();
            }

            GUILayout.EndHorizontal();

            if (GUI.changed)
                EditorUtility.SetDirty(effect);
        }

        public static class FragmentExplosionGizmoDrawer
        {
            [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
            public static void DrawCustomGizmo(ExplosionEffect effect, GizmoType gizmoType)
            {
                if (effect.origin == null || effect.config == null)
                    return;

                DrawHalfCircles(effect.origin,
                               new float[] { effect.config.firstRadius, effect.config.secondRadius },
                               effect.config.minSpawnAngle, effect.config.maxSpawnAngle);

                if (effect.OriginPos == null || effect.OriginPos.Count == 0 || effect.Targets == null || effect.Targets.Count == 0)
                    return;

                for (int i = 0; i < effect.OriginPos.Count; i++)
                    DrawTrajectory(effect.OriginPos[i], effect.Targets[i], effect.config);
            }

            static private void DrawTrajectory(Vector3 origin, Vector3 target, FragmentExplosionConfig config)
            {
                Vector3 fromTo = target - origin;
                Vector3 dirXZ = new Vector3(fromTo.x, 0, fromTo.z).normalized;

                float angleRad = config.angleInDegrees * Mathf.Deg2Rad;
                Vector3 shootDirection = Quaternion.AngleAxis(config.angleInDegrees, Vector3.Cross(dirXZ, Vector3.up)) * dirXZ;

                float speed = CalculateShotSpeed(origin, target, angleRad);
                Vector3 velocity = shootDirection * speed;

                Handles.color = Color.yellow;
                Vector3 previousPoint = origin;

                for (int i = 1; i <= config.trajectoryPoints; i++)
                {
                    float t = i * config.timeStep;
                    Vector3 currentPoint = origin + velocity * t + 0.5f * Physics.gravity * t * t;
                    Handles.DrawLine(previousPoint, currentPoint);
                    previousPoint = currentPoint;
                }
            }

            static private void DrawHalfCircles(Transform origin, float[] radii, float startAngleDeg, float endAngleDeg)
            {
                Vector3 center = origin.position;
                Vector3 normal = Vector3.up;
                Vector3 forward = origin.forward;

                foreach (float radius in radii)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawWireArc(
                        center,
                        normal,
                        Quaternion.Euler(0, startAngleDeg, 0) * forward,
                        endAngleDeg - startAngleDeg,
                        radius
                    );
                }
            }

            static private float CalculateShotSpeed(Vector3 from, Vector3 to, float angleRad)
            {
                Vector3 fromTo = to - from;
                float x = new Vector3(fromTo.x, 0, fromTo.z).magnitude;
                float y = fromTo.y;

                float denominator = 2 * (y - Mathf.Tan(angleRad) * x);
                if (Mathf.Approximately(denominator, 0f)) return 0f;

                float vSquared = (Physics.gravity.y * x * x) / (denominator * Mathf.Pow(Mathf.Cos(angleRad), 2));
                return Mathf.Sqrt(Mathf.Abs(vSquared));
            }
        }
    }
}