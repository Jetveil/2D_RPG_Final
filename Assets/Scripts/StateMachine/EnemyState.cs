using UnityEngine;
using UnityEngine.PlayerLoop;

public class EnemyState : EntityState
{
    protected Enemy enemy;

    public EnemyState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.enemy = enemy;
        
        rb = enemy.rb;
        anim = enemy.anim;
        stats = enemy.stats;
    }


    /// <summary>
    /// Обновляет аним-параметры врага (множители и скорость).
    /// </summary>
    public override void UpdateAnimationParams()
    {
        base.UpdateAnimationParams();

        float battleAnimSpeedMultiplier = enemy.battleMoveSpeed / enemy.moveSpeed;

        anim.SetFloat("battleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        anim.SetFloat("moveAnimSpeedMultiplier", enemy.moveAnimSpeedMultiplier);
        anim.SetFloat("xVelocity", enemy.battleMoveSpeed);
    }
}
