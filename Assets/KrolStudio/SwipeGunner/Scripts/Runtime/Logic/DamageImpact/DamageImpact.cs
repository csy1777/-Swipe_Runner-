using System;
using UnityEngine;

namespace KrolStudio
{
    public class DamageEventArgs : EventArgs
    {
        public int damage { get; }

        public DamageEventArgs(int damage)
        {
            this.damage = damage;
        }
    }

    public class DamageDirectionEventArgs : EventArgs
    {
        public int damage { get; }
        public Vector3 direction { get; }

        public DamageDirectionEventArgs(int damage, Vector3 direction)
        {
            this.damage = damage;
            this.direction = direction;
        }
    }

    public abstract class DamageImpact : MonoBehaviour
    {      
        protected abstract IHealth Health { get; }

        public virtual void Awake()
        {
            Health.OnHealthChanged += OnHealthChange;
            Health.OnDied += OnDied;
        }

        protected virtual void OnHealthChange(object sender, EventArgs e) { }

        protected virtual void OnDied(object sender, EventArgs e) { }

        private void OnDestroy()
        {
            if (Health != null)
            {
                Health.OnHealthChanged -= OnHealthChange;
                Health.OnDied -= OnDied;
            }
        }
    }
}