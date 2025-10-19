using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Chest : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb => GetComponent<Rigidbody2D>();
    private Animator anim => GetComponentInChildren<Animator>();
    private Entity_VFX entityVFX => GetComponent<Entity_VFX>();

    [Header("Chest Open Knockback")]
    [SerializeField]
    private Vector2 knockbackForce;

    public void TakeDamage(float damage, Transform damageDealer)
    {
        anim.SetBool("chestOpen", true);
        rb.linearVelocity = knockbackForce;
        rb.angularVelocity = Random.Range(-200f, 200f);

        entityVFX.PlayOnDamageVFX();
    }

}