using System;
using UnityEngine;

namespace KrolStudio
{
    public class Health : MonoBehaviour, IHealth
    {
        int current;
        int max;

        public event EventHandler<EventArgs> OnHealthChanged;
        public event EventHandler<EventArgs> OnDied;

        public bool IsAlive => current > 0;

        public int Current
        {
            get => current;
            set => current = value;
        }

        public int Max
        {
            get => max;
        }

        public float Ratio => max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        public void SetMaxHealth(int value)
        {
            max = value;
        }

        public virtual void TakeDamage(int damage, object sender, EventArgs e)
        {
            if (current > damage)
            {
                current -= damage;
                OnHealthChanged?.Invoke(sender, e);
            }
            else if (current > 0)
            {
                current = 0;
                OnHealthChanged?.Invoke(sender, e);
                OnDied?.Invoke(sender, e);
            }
        }

        public void ResetHealth()
        {
            current = max;
        }

        public virtual void Initialize() { }
    }
}