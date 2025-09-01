using UnityEngine;

public class Player_BasicAttackState : EntityState
{
    public Player_BasicAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    private float attackVelocityTimer;

    public override void Enter()
    {
        base.Enter();

        attackVelocityTimer = player.attackVelocityDuration;
    }

    public override void Update()
    {
        base.Update();
        HandleAttackVelocity();
        
        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);
    }

    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;

        if (attackVelocityTimer < 0)
            player.SetVelocity(0, rb.linearVelocity.y);
    }
}