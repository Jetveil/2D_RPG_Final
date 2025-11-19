using System;
using UnityEngine;

public class Entity_AnimationTriggers : MonoBehaviour
{
    public Entity entity;
    public Entity_Combat entityCombat;

    protected virtual void Awake()
    {
        entity = GetComponentInParent<Entity>();
        entityCombat = entity.GetComponentInParent<Entity_Combat>();
    }

    /// <summary>
    /// Сообщает активному состоянию FSM о срабатывании анимационного триггера.
    /// </summary>
    private void CurrentStateTrigger()
    {
        entity.CallAnimationTrigger();
    }

    /// <summary>
    /// Триггер атаки из анимации: инициирует логику удара у Entity_Combat.
    /// </summary>
    private void AttackTrigger()
    {
        entityCombat.PerformAttack();
    }
}
