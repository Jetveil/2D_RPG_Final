using System;
using UnityEngine;

public class UI_MiniHealthBar : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    private void OnEnable() => entity.OnFlipped += HandleFlip;

    private void OnDisable() => entity.OnFlipped -= HandleFlip;

    /// <summary>
    /// Сбрасывает поворот, чтобы мини-HP бар не зеркалился при флипе сущности.
    /// </summary>
    private void HandleFlip() => transform.rotation = Quaternion.identity;
}
