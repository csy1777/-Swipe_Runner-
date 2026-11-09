using UnityEngine;

namespace KrolStudio
{
    public class TurretPart : MonoBehaviour
    {
        public Transform rotatingPart;
        public Transform trajectoryPoint;
   
        [Space]
        public int ammoToSpend;
        public bool fireSequentially;
        [Space]
        public FireComponents[] fireComponents;
    }

    [System.Serializable]
    public class FireComponents
    {
        public Transform firePoint;
        public ParticleSystem fireEffect;
        public Animator fireAnimator;

        [SerializeField] string fireAnimationKey = "Fire";
        
        int animationHash;

        public int AnimationHash()
        {
            if(animationHash == 0)
                animationHash = Animator.StringToHash(fireAnimationKey);
            return animationHash;
        }
    }
}
