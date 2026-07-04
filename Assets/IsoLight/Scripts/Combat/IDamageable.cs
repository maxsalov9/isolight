namespace IsoLight.Combat
{
    public interface IDamageable
    {
        int MaxHealth { get; }
        int CurrentHealth { get; }
        bool IsAlive { get; }

        void TakeDamage(int amount);
    }
}
