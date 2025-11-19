using UnityEngine;

public class Enemy_DeadState : EnemyState
{
    public Enemy_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// Вход в смерть: отключает анимацию, включает падение и блокирует FSM.
    /// </summary>
    public override void Enter()
    {
        anim.enabled = false;
        rb.gravityScale = 12;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15);

        enemy.GetComponent<Collider2D>().enabled = false;
        
        stateMachine.SwitchOffStateMachine();
    }
}
