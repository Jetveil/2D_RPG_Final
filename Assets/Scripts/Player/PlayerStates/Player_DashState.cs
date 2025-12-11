using UnityEngine;

public class Player_DashState : PlayerState
{
    public Player_DashState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    private float originalGravityScale;
    private int dashDir;

    /// <summary>
    /// Подготавливает рывок: направление, отключение гравитации и таймер.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        skillManager.dash.OnStartEffect();
        player.vfx.DoImageEchoEffect(player.dashDuration);

        stateTimer = player.dashDuration;

        dashDir = player.moveInput.x != 0 ? (int)player.moveInput.x : player.facingDir;

        originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0;
    }

    /// <summary>
    /// Движение рывком и выход по таймеру в idle/fall; отмена при касании стены.
    /// </summary>
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

    /// <summary>
    /// Завершение рывка: обнуляет скорость и возвращает гравитацию.
    /// </summary>
    public override void Exit()
    {
        base.Exit();

        skillManager.dash.OnEndEffect();

        player.SetVelocity(0, 0);
        rb.gravityScale = originalGravityScale;
    }

    /// <summary>
    /// Прерывает рывок при контакте со стеной.
    /// </summary>
    private void CancelDashIfNeeded()
    {
        if (player.wallDetected)
            stateMachine.ChangeState(player.wallSlideState);
        // else if (player.groundDetected)
        //     stateMachine.ChangeState(player.idleState);
    }
}