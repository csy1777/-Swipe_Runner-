using System;

namespace KrolStudio
{
    public class EnemyHealthBar : HealthBar
    {
        Health enemyHealth;
     
        private bool isShown;
        protected override IHealth Health => enemyHealth;

        public void Initialize(Health enemyHealth)
        {
            this.enemyHealth = enemyHealth;
        }

        public void Show()
        {
            isShown = true;
            holder.SetActive(true);
        }

        public void Hide()
        {
            isShown = false;
            holder.SetActive(false);
        }

        public override void OnHealthChanged(object sender, EventArgs e)
        {
            if (!isShown)
                Show();

            UpdateHealthSmooth();
        }

        public override void OnDied(object sender, EventArgs e)
        {
            Hide();
        }
    }
}