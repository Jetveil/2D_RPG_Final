using UnityEngine;
using UnityEngine.InputSystem;

public class Player_IdleState : Player_GroundedState
{
    public Player_IdleState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// Ставит скорость в ноль при входе в ожидание.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        player.SetVelocity(0, 0);
    }

    /// <summary>
    /// Переходит в движение при наличии ввода, игнорируя упор в стену.
    /// </summary>
    public override void Update()
    {
        base.Update();
        
        if (player.moveInput.x == player.facingDir && player.wallDetected)
            return;

        if (player.moveInput.x != 0)
            stateMachine.ChangeState(player.moveState);
    }
}
