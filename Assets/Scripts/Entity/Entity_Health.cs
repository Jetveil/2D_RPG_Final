using System;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;
using UnityEngine.UI;
// using Random = System.Random;
using Random = UnityEngine.Random;

public class Entity_Health : MonoBehaviour, IDamageable
{
    private Entity entity;
    private Slider healthBar;
    private Entity_VFX enemyVFX;
    private Entity_Stats entityStats;

    [SerializeField] protected float currentHealth;
    [SerializeField] protected bool isDead;

    [Header("Health regen")]
    [SerializeField] private float healthRegenInterval = 1f;
    [SerializeField] private bool canRegenHealth = true;

    [Header("On Damage Knockback")]
    [SerializeField]
    private Vector2 knockbackForce = new Vector2(1.5f, 2.5f);
    [SerializeField]
    private Vector2 heavyKnockbackForce = new Vector2(7f, 7f);
    [SerializeField]
    private float knockbackDuration = 0.2f;
    [SerializeField]
    private float heavyKnockbackDuration = 0.5f;

    [Header("On Heavy Damage Knockback")]
    [SerializeField]
    private float heavyDamageThreshold = 0.3f; // Percentage of HP to lose to consider attack as heavy


    /// <summary>
    /// Инициализирует ссылки на компоненты, устанавливает текущее здоровье,
    /// обновляет индикатор здоровья и запускает периодическую регенерацию.
    /// </summary>
    private void Awake()
    {
        enemyVFX = GetComponent<Entity_VFX>();
        entity = GetComponent<Entity>();
        entityStats = GetComponent<Entity_Stats>();
        healthBar = GetComponentInChildren<Slider>();

        currentHealth = entityStats.GetMaxHealth();
        UpdateHealthBar();

        InvokeRepeating(nameof(RegenerateHealth), 0, healthRegenInterval);
    }

    /// <summary>
    /// Обновляет значение слайдера здоровья на основе текущего HP.
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.value = currentHealth / entityStats.GetMaxHealth();
    }

    /// <summary>
    /// Принимает входящий урон, учитывая броню/резисты; применяет нокбэк и VFX.
    /// </summary>
    public virtual bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (isDead)
            return false;

        if (AttackEvaded())
        {
            Debug.Log($"{gameObject.name} evaded the attack!");
            return false;
        }

        Entity_Stats attackerStats = GetComponent<Entity_Stats>();
        float armorReduction = attackerStats != null ? attackerStats.GetArmorReduction() : 0;

        float mitigation = entityStats.GetArmorMitigation(armorReduction);
        float physicalDamageTaken = damage * (1 - mitigation);

        float resistance = entityStats.GetElementalResistance(element);
        float elementalDamageTaken = elementalDamage * (1 - resistance);

        TakeKnockback(damageDealer, physicalDamageTaken);
        ReduceHealth(physicalDamageTaken + elementalDamageTaken);
        return true;
    }

    /// <summary>
    /// Проверяет шанс уклонения от атаки по стату evasion.
    /// </summary>
    private bool AttackEvaded() => Random.Range(0, 100) < entityStats.GetEvasion();

    /// <summary>
    /// Восстанавливает здоровье по тикеру регенерации, если это разрешено.
    /// Вызывается `InvokeRepeating` с интервалом <see cref="healthRegenInterval"/>.
    /// </summary>
    public void RegenerateHealth()
    {
        if (canRegenHealth == false)
            return;

        float regenAmount = entityStats.resources.healthRegen.GetValue();
        IncreaseHealth(regenAmount);
    }

    /// <summary>
    /// Увеличивает текущее здоровье на указанную величину, не превышая максимум,
    /// и обновляет индикатор здоровья.
    /// </summary>
    public void IncreaseHealth(float healAmount)
    {
        if (isDead)
            return;

        float newHealth = currentHealth + healAmount;
        float maxHealth = entityStats.GetMaxHealth();

        currentHealth = Mathf.Min(newHealth, maxHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Уменьшает здоровье на заданный урон, обновляет UI и проверяет смерть.
    /// </summary>
    public void ReduceHealth(float damage)
    {
        currentHealth -= damage;
        enemyVFX?.PlayOnDamageVFX();
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Помечает сущность мёртвой и вызывает реакцию смерти у Entity.
    /// </summary>
    private void Die()
    {
        isDead = true;
        entity.EntityDeath();
    }

    public float GetHealthPercent() => currentHealth / entityStats.GetMaxHealth();

    public void SetHealthToPercent(float percent)
    {
        currentHealth = entityStats.GetMaxHealth() * Mathf.Clamp01(percent);
        UpdateHealthBar();
    }

    /// <summary>
    /// Вычисляет силу и направление нокбэка исходя из урона и позиции атакующего.
    /// </summary>
    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;

        Vector2 knockback = IsHeavyDamage(damage) ? heavyKnockbackForce : knockbackForce;

        knockback.x *= direction;
        return knockback;
    }

    /// <summary>
    /// Считает и применяет нокбэк на основе полученного физического урона.
    /// </summary>
    private void TakeKnockback(Transform damageDealer, float physicalDamageTaken)
    {
        Vector2 knockback = CalculateKnockback(physicalDamageTaken, damageDealer);

        entity?.ReceiveKnockback(knockback, CalculateKnockbackDuration(physicalDamageTaken));
    }

    /// <summary>
    /// Проверяет, является ли урон «тяжёлым» (по доле от макс. HP).
    /// </summary>
    private bool IsHeavyDamage(float damage) => damage / entityStats.GetMaxHealth() >= heavyDamageThreshold;

    /// <summary>
    /// Возвращает длительность нокбэка в зависимости от тяжести урона.
    /// </summary>
    private float CalculateKnockbackDuration(float damage)
    {
        return IsHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration;
    }
}