using System;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;
using UnityEngine.UI;
// using Random = System.Random;
using Random = UnityEngine.Random;

public class Entity_Health : MonoBehaviour, IDamageable
{
    private Slider healthBar;
    private Entity_VFX enemyVFX;
    private Entity entity;
    private Entity_Stats stats;

    [SerializeField] protected float currentHealth;
    [SerializeField] protected bool isDead;

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

    private void Awake()
    {
        enemyVFX = GetComponent<Entity_VFX>();
        entity = GetComponent<Entity>();
        stats = GetComponent<Entity_Stats>();
        healthBar = GetComponentInChildren<Slider>();

        currentHealth = stats.GetMaxHealth();
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.value = currentHealth / stats.GetMaxHealth();
    }


    public virtual bool TakeDamage(float damage, Transform damageDealer)
    {
        if (isDead)
            return false;

        if (AttackEvaded())
        {
            Debug.Log($"{gameObject.name} evaded the attack!");
            return false;
        }

        Vector2 knockback = CalculateKnockback(damage, damageDealer);

        entity?.ReceiveKnockback(knockback, CalculateKnockbackDuration(damage));
        enemyVFX?.PlayOnDamageVFX();
        ReduceHealth(damage);

        return true;
    }

    private bool AttackEvaded() => Random.Range(0, 100) < stats.GetEvasion();

    protected void ReduceHealth(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        entity.EntityDeath();
    }

    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;

        Vector2 knockback = IsHeavyDamage(damage) ? heavyKnockbackForce : knockbackForce;

        knockback.x *= direction;
        return knockback;
    }


    private bool IsHeavyDamage(float damage) => damage / stats.GetMaxHealth() >= heavyDamageThreshold;

    private float CalculateKnockbackDuration(float damage)
    {
        return IsHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration;
    }
}