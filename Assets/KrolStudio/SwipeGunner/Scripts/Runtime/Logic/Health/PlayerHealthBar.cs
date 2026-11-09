using System;
using TMPro;
using UnityEngine;

namespace KrolStudio
{
    public interface IPlayerHealthBar
    {
        void Show();
        void Hide();
    }

    public class PlayerHealthBar : HealthBar, IPlayerHealthBar
    {
        [SerializeField] TMP_Text ammoAmountText;
        [SerializeField] Animator animator;
        [SerializeField] GameObject warningImage;
        
        PlayerHealth playerHealth;

        readonly int ScaleBullets = Animator.StringToHash(GameConstants.Animations.ScaleBullets);
        readonly int ScaleRedBullets = Animator.StringToHash(GameConstants.Animations.ScaleRedBullets);

        protected override IHealth Health => playerHealth;

        public void Initialize(PlayerHealth playerHealth)
        {
            this.playerHealth = playerHealth;
        }

        public void UpdateAmmoUI(int currentBullets, int lowBulletsThreshold)
        {
            warningImage.SetActive(currentBullets > 0 && currentBullets <= lowBulletsThreshold);
            
            ammoAmountText.text = currentBullets.ToString();
            
            animator.SetTrigger(currentBullets > 0 ? ScaleBullets : ScaleRedBullets);
        }

        public void Show()
        {
            holder.SetActive(true);
        }

        public void Hide()
        {
            holder.SetActive(false);
        }

        public override void OnHealthChanged(object sender, EventArgs e)
        {
            UpdateHealthSmooth();
        }

        public override void OnDied(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
