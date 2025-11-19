using UnityEngine;

public class Player_JumpState : Player_AiredState
{
    public Player_JumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// Задаёт вертикальную скорость прыжка при входе.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        player.SetVelocity(rb.linearVelocity.x, player.jumpForce);
    }

    /// <summary>
    /// На спаде вертикальной скорости переключается в падение (если не jump-attack).
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (rb.linearVelocity.y < 0 && stateMachine.currentState is not Player_JumpAttack_State)
        {
            stateMachine.ChangeState(player.fallState);
        }
    }
}
