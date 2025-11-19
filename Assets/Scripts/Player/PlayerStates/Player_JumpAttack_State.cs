using UnityEngine;

public class Player_JumpAttack_State : PlayerState
{
    private bool touchedGround;

    public Player_JumpAttack_State(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// Вход: придаёт импульс атаки в прыжке и триггерит фазу.
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        touchedGround = false;
        player.SetVelocity(player.jumpAttackVelocity.x * player.facingDir, player.jumpAttackVelocity.y);
        anim.SetTrigger("jumpAttackTrigger");
    }

    /// <summary>
    /// Логика приземления: повторный триггер и выход в idle по сигналу анимации.
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (player.groundDetected && touchedGround == false)
        {
            touchedGround = true;
            anim.SetTrigger("jumpAttackTrigger");
            player.SetVelocity(0, rb.linearVelocity.y);
        }

        if (player.groundDetected && triggerCalled)
            stateMachine.ChangeState(player.idleState);
    }
}
