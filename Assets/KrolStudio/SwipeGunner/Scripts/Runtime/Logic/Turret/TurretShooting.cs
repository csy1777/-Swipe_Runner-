using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace KrolStudio
{
    public interface IShooter
    {
        void ShotUpdate();
        void Subscribe();
        void Unsubscribe();
    }

    public class TurretShooting : IShooter
    {
        readonly IInputService inputService;
        readonly IPlayerContext playerContext;
        readonly IInteractorPoolContainer interactorPool;
        readonly ILogService logService;
        readonly FireModeConfig fireModeConfig;
        readonly IPersistentProgressService progressService;

        FireMode currentFireMode;
        FireMode selectedFireMode;

        bool isShooting;
        float timer;
        int nextPointIndex;

        // Burst：还剩下几发，以及三连发间隔计时
        int burstShotsRemaining;
        float burstTimer;

        private float currentHeat;
        private float maxHeat;
        public float HeatNormalized => 
            maxHeat <= 0f ? 0f : Mathf.Clamp01(currentHeat / maxHeat);

        public bool IsOverheated => 
            maxHeat > 0f && currentHeat >= maxHeat;

        public FireMode CurrentFireMode => currentFireMode;

        [Inject]
        public TurretShooting(
            IPlayerContext playerContext,
            IInputService inputService,
            IInteractorPoolContainer interactorPool,
            ILogService logService,
            FireModeConfig fireModeConfig,
            IPersistentProgressService progressService)
        {
            this.inputService = inputService;
            this.playerContext = playerContext;
            this.interactorPool = interactorPool;
            this.logService = logService;
            this.fireModeConfig = fireModeConfig;
            this.progressService = progressService;

            currentFireMode = progressService.Progress != null
                ? progressService.Progress.SelectedFireMode
                : fireModeConfig.defaultFireMode;
            
        }

        public void SetFireMode(FireMode mode)
        {
            if (progressService.Progress != null)
                progressService.Progress.SelectedFireMode = mode;

            ApplyFireMode(mode);
        }

        public void Subscribe()
        {
            FireMode mode = progressService.Progress != null
                ? progressService.Progress.SelectedFireMode
                : fireModeConfig.defaultFireMode;
            
            ApplyFireMode(mode);

            if (progressService.Progress != null)
                progressService.Progress.SelectedFireMode = mode;

            inputService.OnButtonDown += InputDown;
            inputService.OnButtonUp += InputUp;
        }

        public void Unsubscribe()
        {
            inputService.OnButtonDown -= InputDown;
            inputService.OnButtonUp -= InputUp;

            isShooting = false;
            burstShotsRemaining = 0;
            currentHeat = 0f;
        }

        private void ApplyFireMode(FireMode mode)
        {
            currentFireMode = mode;
            selectedFireMode = mode;

            isShooting = false;
            timer = 0f;

            burstShotsRemaining = 0;
            burstTimer = 0f;
            
            maxHeat = fireModeConfig.maxHeat;
            currentHeat = 0f;
        }
        

        private void InputDown()
        {
            if (EventSystem.current.IsPointerOverGameObject() ||
                EventSystem.current.IsPointerOverGameObject(0))
            {
                if (!RaycastUtility.RaycastFromMouse(
                        out var results,
                        LayerMask.GetMask("Player")))
                {
                    return;
                }
            }

            if (IsOverheated)
            {
                playerContext.AudioManager.PlaySound(
                    GameConstants.Sounds.TurretDryFire);

                return;
            }
            isShooting = true;

            if (currentFireMode == FireMode.Burst)
            {
                StartBurstVolley();
            }
            
        }
        private void StartBurstVolley()
        {
            burstShotsRemaining = fireModeConfig.burstCount;
            burstTimer = 0f;

            // 第一发立即发射
            Shot();
        }
        private void InputUp()
        {
            isShooting = false;

            // 松开后停止当前连发，并取消下一组
            burstShotsRemaining = 0;
            burstTimer = 0f;
        }

        public void ShotUpdate()
        {
            timer += Time.deltaTime;

            if (currentFireMode != FireMode.Overheat)
            {
                currentHeat = Mathf.Max(
                    0f,
                    currentHeat -
                    fireModeConfig.coolDownRate * Time.deltaTime);
            }
            
            switch (currentFireMode)
            {
                case FireMode.Single:
                    UpdateSingle();
                    break;

                case FireMode.Burst:
                    UpdateBurst();
                    break;

                case FireMode.Auto:
                    UpdateAuto();
                    break;
                case FireMode.Overheat:
                    UpdateOverHeat();
                    break;
            }
        }


        private void UpdateSingle()
        {
            // 恢复原版逻辑：按住鼠标，按照 FireInterval 连续射击。
            if (isShooting &&
                timer >= playerContext.PlayerStats.FireInterval)
            {
                Shot();
            }
        }

        private void UpdateAuto()
        {
            // 原射速的两倍：间隔乘以 0.5。
            float autoInterval =
                playerContext.PlayerStats.FireInterval * 0.5f;

            if (isShooting && timer >= autoInterval)
            {
                Shot();
            }
        }

        private void UpdateBurst()
        {
            if (!isShooting)
                return;

            burstTimer += Time.deltaTime;

            // 当前这一组还没打满三发
            if (burstShotsRemaining > 0)
            {
                if (burstTimer < fireModeConfig.burstInterval)
                    return;

                burstTimer = 0f;
                Shot();
                return;
            }

            // 三发已经打完，等待更长的组间隔
            if (burstTimer < fireModeConfig.burstInterval*6)
                return;

            // 鼠标仍按着，开始下一组三连发
            StartBurstVolley();
        }

        private void UpdateOverHeat()
        {
            if (timer >= fireModeConfig.overheatRecoveryTime)
            {
                currentFireMode = selectedFireMode;
                currentHeat = 0f;
                timer = 0f;
            }
        }
        
        private void Shot()
        {
            currentHeat = Mathf.Min(
                maxHeat,
                currentHeat + fireModeConfig.heatPerShot);
            if (currentHeat >= maxHeat)
            {
                currentFireMode=FireMode.Overheat;
                return;
            }
            
            if (playerContext.Turret.TurretLevel < 0)
                return;

            playerContext.AmmoTracker.OnShoot(
                playerContext.Turret.AmmoToSpend);

            if (playerContext.AmmoTracker.IsAmmoEmpty ||
                playerContext.Turret.TurretLevel < 0)
            {
                return;
            }

            timer = 0f;

            if (playerContext.Turret.FireComponents != null &&
                playerContext.Turret.FireComponents.Length > 0)
            {
                if (playerContext.Turret.FireSequentially)
                    FireNextPoint();
                else
                    FireAllPoints();
            }

            PlayShotSound();
            
            if (currentFireMode == FireMode.Burst)
            {
                burstTimer = 0f;

                if (burstShotsRemaining > 0)
                    burstShotsRemaining--;
            }
        }

        private void FireNextPoint()
        {
            nextPointIndex %=
                playerContext.Turret.FireComponents.Length;

            Projectile projectile =
                interactorPool.GetPool<Projectile>().Get();

            projectile.Initialize(
                playerContext.Turret
                    .FireComponents[nextPointIndex]
                    .firePoint);

            playerContext.Turret
                .FireComponents[nextPointIndex]
                .fireEffect?
                .Play(true);

            playerContext.Turret
                .FireComponents[nextPointIndex]
                .fireAnimator?
                .SetTrigger(
                    playerContext.Turret
                        .FireComponents[nextPointIndex]
                        .AnimationHash());

            nextPointIndex++;
        }

        private void FireAllPoints()
        {
            for (int i = 0;
                i < playerContext.Turret.FireComponents.Length;
                i++)
            {
                Projectile projectile =
                    interactorPool.GetPool<Projectile>().Get();

                projectile.Initialize(
                    playerContext.Turret
                        .FireComponents[i]
                        .firePoint);

                playerContext.Turret
                    .FireComponents[i]
                    .fireEffect?
                    .Play(true);

                playerContext.Turret
                    .FireComponents[i]
                    .fireAnimator?
                    .SetTrigger(
                        playerContext.Turret
                            .FireComponents[i]
                            .AnimationHash());
            }
        }

        private void PlayShotSound()
        {
            int level =
                playerContext.UpgradeLevel.GetLevel(PartType.Turret);

            int clampedLevel = Mathf.Clamp(
                level,
                0,
                GameConstants.Sounds.TurretSoundByLevel.Count - 1);

            if (level >=
                GameConstants.Sounds.TurretSoundByLevel.Count)
            {
                logService.LogWarning(
                    $"Turret level {level} exceeds available sound count " +
                    $"({GameConstants.Sounds.TurretSoundByLevel.Count}). " +
                    $"Using highest available sound.");
            }

            playerContext.AudioManager.PlaySound(
                GameConstants.Sounds
                    .TurretSoundByLevel[clampedLevel]);
        }

        public GameObject GetClickedUIObject()
        {
            PointerEventData pointerData =
                new PointerEventData(EventSystem.current);

            pointerData.position = Input.mousePosition;

            List<RaycastResult> results =
                new List<RaycastResult>();

            EventSystem.current.RaycastAll(
                pointerData,
                results);

            if (results.Count > 0)
                return results[0].gameObject;

            return null;
        }
    }
}