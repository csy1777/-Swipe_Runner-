using System.Collections.Generic;
using UnityEngine;

namespace KrolStudio
{
    public class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] public FragmentExplosionConfig config;
        [SerializeField] public Transform origin;   // Point relative to which the hit area for fragments after the explosion is calculated


        List<Vector3> targets = new List<Vector3>();

        ExplosionEffectHandler[] explosionParts;

        public ExplosionEffectHandler[] ExplosionParts
        {
            get
            {
                if (explosionParts == null)
                {
                    explosionParts = GetComponentsInChildren<ExplosionEffectHandler>();
                }

                return explosionParts;
            }
        }

#if UNITY_EDITOR
        List<Vector3> originPos = new List<Vector3>();
        List<Quaternion> originRot = new List<Quaternion>();

        public List<Vector3> OriginPos => originPos;
        public List<Vector3> Targets => targets;

        bool initPos = false;
#endif

        public void Explode()
        {
            ProjectileShooter projectile = new ProjectileShooter(config.angleInDegrees);

            foreach (var parts in ExplosionParts)
            {
                var explodeRenderers = parts.ExplodeRenderer();
                if (explodeRenderers == null)
                    continue;

                foreach (var part in explodeRenderers)
                {
                    if (part == null)
                    {
                        Debug.LogWarning("Part is zero. Check the inspector.");
                        continue;
                    }

                    part.material = config.grayMaterial;
                }
            }

            foreach (var parts in ExplosionParts)
            {
                var explodeParts = parts.ExplodeParts();
                if (explodeParts == null)
                    continue;

                foreach (Rigidbody rb in explodeParts)
                {
                    if (rb == null)
                    {
                        Debug.LogWarning("Rigidbody is zero. Check the inspector.");
                        continue;
                    }

                    rb.isKinematic = false;
                    var target = GetRandomDirectionOffset(origin);
                    projectile.Shoot(rb, rb.position, rb.position + target, config.rotationCount);
                }
            }
        }

        Vector3 GetRandomDirectionOffset(Transform transform)
        {
            float angle = Random.Range(config.minSpawnAngle, config.maxSpawnAngle);

            // Uniform distribution across the area:
            float radius = Mathf.Sqrt(Random.Range(0f, 1f)) * (config.secondRadius - config.firstRadius) + config.firstRadius;

            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            return rotation * (transform.forward * radius);
        }


#if UNITY_EDITOR
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.J))
            {
                TestExplosion();
            }
            else if(Input.GetKeyDown(KeyCode.K))
            {
                RestorePosition();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                CreateTargets();
            }
        }

        private void InitOriginPos()
        {
            foreach (var parts in ExplosionParts)
            {
                var explodeParts = parts.ExplodeParts();
                if (explodeParts == null)
                    continue;

                foreach (Rigidbody rb in explodeParts)
                {
                    if (rb == null)
                    {
                        Debug.LogWarning("Rigidbody is zero. Check the inspector.");
                        continue;
                    }

                    originPos.Add(rb.position);
                    originRot.Add(rb.rotation);
                }
            }
        }

        public void TestExplosion()
        {
            if (targets.Count == 0) return;

            ProjectileShooter projectile = new ProjectileShooter(config.angleInDegrees);

            foreach (var parts in ExplosionParts)
            {
                var explodeParts = parts.ExplodeParts();
                if (explodeParts == null)
                    continue;

                for (int i = 0; explodeParts.Count > i; i++)
                {
                    explodeParts[i].isKinematic = false;
                    projectile.Shoot(explodeParts[i], explodeParts[i].position, explodeParts[i].position + targets[i], config.rotationCount);
                }
            }
        }

        public void RestorePosition()
        {
            foreach (var parts in ExplosionParts)
            {
                var explodeParts = parts.ExplodeParts();
                if (explodeParts == null)
                    continue;

                for (int i = 0; i < explodeParts.Count; i++)
                {
                    explodeParts[i].isKinematic = true;
                    explodeParts[i].transform.position = originPos[i];
                    explodeParts[i].transform.rotation = originRot[i];
                }
            }
        }

        public void ClearTargets()
        {
            targets.Clear();
        }

        public void CreateTargets()
        {
            if (!initPos)
            {
                initPos = true;
                InitOriginPos();
            }

            targets.Clear();

            foreach (var parts in ExplosionParts)
            {
                var explodeParts = parts.ExplodeParts();
                if (explodeParts == null)
                    continue;

                foreach (Rigidbody rb in explodeParts)
                {
                    targets.Add(GetRandomDirectionOffset(origin));
                }
            }
        }
#endif
    }
}