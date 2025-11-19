using UnityEngine;

public class Player_DeadState : PlayerState
{
    public Player_DeadState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// Отключает ввод и физику при смерти игрока.
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        
        input.Disable();
        rb.simulated = false;
    }
}
