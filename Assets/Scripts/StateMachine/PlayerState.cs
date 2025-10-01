using UnityEngine;

/// <summary>
/// Базовое состояние сущности: задаёт шаблон для всех конкретных состояний.
/// </summary>
public abstract class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSet input;


    public PlayerState(Player player, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.player = player;

        anim = player.anim;
        rb = player.rb;
        input = player.input;
    }

    public override void Update()
    {
        base.Update();


        if (input.Player.Dash.WasPerformedThisFrame() && CanDash())
            stateMachine.ChangeState(player.dashState);
    }

    public override void UpdateAnimationParams()
    {
        base.UpdateAnimationParams();
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private bool CanDash()
    {
        if (player.wallDetected || stateMachine.currentState == player.dashState)
            return false;
        return true;
    }
}