using System;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Entity_Combat : MonoBehaviour
{
    public float damage = 10f;
    private Entity_VFX vfx;

    [Header("Target Detection")]
    [SerializeField]
    private Transform targetCheck;
    [SerializeField]
    private float targetCheckRadius = 1;
    [SerializeField]
    private LayerMask whatIsTarget;

    private void Awake()
    {
        vfx = gameObject.GetComponent<Entity_VFX>();
    }

    public void PerformAttack()
    {
        foreach (var target in GetDetectedColliders())
        {
            IDamageable damageable = target.GetComponent<IDamageable>();

            if (damageable == null)
                continue;

            damageable.TakeDamage(damage, transform);
            vfx.CreateOnHitVFX(target.transform);
        }
    }

    protected Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatIsTarget);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }
}