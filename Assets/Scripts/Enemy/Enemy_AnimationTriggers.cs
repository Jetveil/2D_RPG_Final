using UnityEngine;

public class Enemy_AnimationTriggers : Entity_AnimationTriggers
{
    private Enemy enemy;
    private Enemy_VFX enemyVFX;

    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponentInParent<Enemy>();
        enemyVFX = GetComponentInParent<Enemy_VFX>();
    }

    /// <summary>
    /// Включает окно уязвимости к контрудару и визуальный индикатор.
    /// </summary>
    private void EnableCounterWindow()
    {
        enemy.EnableCounterWindow(true);
        enemyVFX.EnableAttackAlert(true);
    }

    /// <summary>
    /// Выключает окно контратаки и визуальный индикатор.
    /// </summary>
    private void DisableCounterWindow()
    {
        enemy.EnableCounterWindow(false);
        enemyVFX.EnableAttackAlert(false);
    }
}
