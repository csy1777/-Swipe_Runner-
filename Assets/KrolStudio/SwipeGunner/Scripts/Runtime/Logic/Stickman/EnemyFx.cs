using UnityEngine;

namespace KrolStudio
{
    public class EnemyFx : MonoBehaviour
    {
        [SerializeField] ParticleSystem bloodEffect;

        public void PlayBloodEffect(Vector3 direction)
        {
            if (bloodEffect == null) return;

            bloodEffect.transform.rotation = Quaternion.LookRotation(direction);
            bloodEffect.Play();
        }
    }
}