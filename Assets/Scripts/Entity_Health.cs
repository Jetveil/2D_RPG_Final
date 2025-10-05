using UnityEngine;

public class Entity_Health : MonoBehaviour
{
    [SerializeField]
    protected float maxHealth = 100;
    [SerializeField]
    protected bool isDead;

    public virtual void TakeDamage(float damage)
    {
        if (isDead)
            return;

        ReduceHealth(damage);
    }

    protected void ReduceHealth(float damage)
    {
        maxHealth -= damage;

        if (maxHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} is dead");
    }
}