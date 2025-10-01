using System;
using UnityEngine;

public class Entity_AnimationTriggers : MonoBehaviour
{
    public Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    private void CurrentStateTrigger()
    {
        entity.CallAnimationTrigger();
    }
}