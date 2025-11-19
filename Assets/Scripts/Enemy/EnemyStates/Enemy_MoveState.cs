using UnityEngine;

public class Enemy_MoveState : Enemy_GroundedState
{
    public Enemy_MoveState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// Вход: разворачивается, если упирается в край или стену.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        if (enemy.groundDetected == false || enemy.wallDetected)
            enemy.Flip();
    }

    /// <summary>
    /// Патрульное движение и реакции на пропадание земли/стену.
    /// </summary>
    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, rb.linearVelocity.y);

        if (enemy.groundDetected == false)
            stateMachine.ChangeState(enemy.idleState);
        else if (enemy.wallDetected)
            enemy.Flip();
    }
}
