using System;
using UnityEngine;

public class Entity_AnimationTriggers : MonoBehaviour
{
    public Entity entity;
    public Entity_Combat entityCombat;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
        entityCombat = entity.GetComponentInParent<Entity_Combat>();
    }

    private void CurrentStateTrigger()
    {
        entity.CallAnimationTrigger();
    }

    private void AttackTrigger()
    {
        entityCombat.PerformAttack();
    }
}