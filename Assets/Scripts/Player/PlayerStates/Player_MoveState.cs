using UnityEngine;
using UnityEngine.InputSystem;

public class Player_MoveState : Player_GroundedState
{
    public Player_MoveState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine,
        animBoolName)
    {
    }

    /// <summary>
    /// Двигает по оси X и возвращается в idle при отсутствии ввода/упоре в стену.
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (player.moveInput.x == 0 || player.wallDetected)
            stateMachine.ChangeState(player.idleState);

        player.SetVelocity(player.moveSpeed * player.moveInput.x, rb.linearVelocity.y);
    }
}
