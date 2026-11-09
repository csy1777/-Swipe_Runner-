using UnityEngine;

namespace KrolStudio
{
    public class PlayerFx : MonoBehaviour
    {
        [SerializeField] GameObject spikyFireBig;
        [SerializeField] GameObject smokeDarkTrail;
        [SerializeField] GameObject wheelTrail;

        [Space]
        [SerializeField] Transform nukeExplosionPrefab;
        [SerializeField] Transform coinsDirectionalPrefab;
        [SerializeField] Transform confettiDirectionalPrefab;

        [Space]
        [SerializeField] Transform directionalRightParent;
        [SerializeField] Transform directionalLeftParent;

        private void Awake()
        {
            spikyFireBig.SetActive(false);
            smokeDarkTrail.SetActive(false);
            wheelTrail.SetActive(false);
        }

        public void PlayCoinsDirectional()
        {
            var obj = Object.Instantiate(coinsDirectionalPrefab);
            obj.SetParent(directionalRightParent);
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;

            obj = Object.Instantiate(coinsDirectionalPrefab);
            obj.SetParent(directionalLeftParent);
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;
        }

        public void PlayConfetti()
        {
            var obj = Object.Instantiate(confettiDirectionalPrefab);
            obj.SetParent(directionalRightParent);
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;

            obj = Object.Instantiate(confettiDirectionalPrefab);
            obj.SetParent(directionalLeftParent);
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;
        }

        public void PlayNukeExplosion(bool value)
        {
            var obj = Object.Instantiate(nukeExplosionPrefab);
            obj.SetParent(transform);
            obj.localPosition = new Vector3(0f, 0.5f, 0f);
            obj.localRotation = Quaternion.identity;
        }

        public void PlaySpikyFireBig(bool value) { spikyFireBig.SetActive(value); }
        public void PlaySmokeDarkTrail(bool value) { smokeDarkTrail.SetActive(value); }
        public void PlayWheelTrail(bool value) { wheelTrail.SetActive(value); }
    }
}
