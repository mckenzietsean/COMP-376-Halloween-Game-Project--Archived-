using UnityEngine;

public interface IDamageable
{
    void Heal(int heal);
    void TakeDamage(int damage, GameObject attacker);
    void Death();
}
