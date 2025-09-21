using UnityEngine;

public class Player_DashState : EntityState
{
    public Player_DashState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    private float originalGravityScale;
    private int dashDir;

    public override void Enter()
    {
        base.Enter();

        stateTimer = player.dashDuration;

        dashDir = player.moveInput.x != 0 ? (int)player.moveInput.x : player.facingDir;

        originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0;
    }

    public override void Update()
    {
        base.Update();
        CancelDashIfNeeded();
        player.SetVelocity(player.dashSpeed * dashDir, 0);

        if (stateTimer < 0)
        {
            if (player.groundDetected)
                stateMachine.ChangeState(player.idleState);
            else
                stateMachine.ChangeState(player.fallState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        player.SetVelocity(0, 0);
        rb.gravityScale = originalGravityScale;
    }

    private void CancelDashIfNeeded()
    {
        if (player.wallDetected)
            stateMachine.ChangeState(player.wallSlideState);
        // else if (player.groundDetected)
        //     stateMachine.ChangeState(player.idleState);
    }
}