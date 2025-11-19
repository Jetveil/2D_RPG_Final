using UnityEngine;

/// <summary>
/// Сущность, способная получать урон.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Получение урона: физический и элементальный компонент, тип элемента и источник.
    /// </summary>
    public bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer);
}
