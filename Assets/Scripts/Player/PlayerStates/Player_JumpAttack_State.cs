using UnityEngine;

public class Player_JumpAttack_State : PlayerState
{
    private bool touchedGround;

    public Player_JumpAttack_State(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        touchedGround = false;
        player.SetVelocity(player.jumpAttackVelocity.x * player.facingDir, player.jumpAttackVelocity.y);
        anim.SetTrigger("jumpAttackTrigger");
    }

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