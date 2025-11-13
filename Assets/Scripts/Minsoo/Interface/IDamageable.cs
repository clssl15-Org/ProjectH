using Infrastructure;

public interface IDamageable
{
    void TakeDamage(int damage);
    void TakeDamage(int damage, Direction direction, float? knockbackForce = null);
}
