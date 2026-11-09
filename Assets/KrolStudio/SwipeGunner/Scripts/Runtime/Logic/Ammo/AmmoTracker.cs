using UnityEngine;
using Zenject;
using System;

namespace KrolStudio
{
    public class AmmoTracker : MonoBehaviour
    {
        private int currentBullets;
        private IPlayerContext playerContext;

        Action<int, int> OnAmmoUIUpdate;
        Action OutOfAmmo;

        [Inject]
        void Construct(IPlayerContext playerContext)
        {
            this.playerContext = playerContext;
        }

        public void Initialize(Action<int, int> onAmmoUIUpdate, Action OutOfAmmo)
        {
            this.OnAmmoUIUpdate = onAmmoUIUpdate;
            this.OutOfAmmo = OutOfAmmo;

            currentBullets = playerContext.PlayerStats.CurrentBullets;

            onAmmoUIUpdate?.Invoke(currentBullets, playerContext.PlayerStats.LowBulletsThreshold);
        }

        public bool IsAmmoEmpty => currentBullets <= 0;

        public void OnShoot(int ammoSpent)
        {
            if (IsAmmoEmpty) return;

            currentBullets -= ammoSpent;

            if (currentBullets <= 0)
            {
                currentBullets = 0;
                OutOfAmmo?.Invoke();
            }

            OnAmmoUIUpdate?.Invoke(currentBullets, playerContext.PlayerStats.LowBulletsThreshold);
        }

        public void OnReload(int newAmount)
        {
            currentBullets = newAmount;
            OnAmmoUIUpdate?.Invoke(currentBullets, playerContext.PlayerStats.LowBulletsThreshold);
        }
    }
}