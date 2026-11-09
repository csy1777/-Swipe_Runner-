using System;

namespace KrolStudio
{
    public interface IDamageable
    {
        void TakeDamage(int damage, object sender, EventArgs eventArgs);
    }

    public interface IHealth : IDamageable
    {
        event EventHandler<EventArgs> OnHealthChanged;
        event EventHandler<EventArgs> OnDied;

        int Current { get; set; }
        int Max { get; }
        float Ratio { get; }
        bool IsAlive { get; }

        void Initialize();
        void SetMaxHealth(int value);
        void ResetHealth();
    }
}