using UnityEngine;

public class Enemy_GroundedState : EnemyState
{
    public Enemy_GroundedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// На земле: при обнаружении игрока переходит в бой.
    /// </summary>
    public override void Update()
    {
        base.Update();
        
        if(enemy.PlayerDetected() == true)
            stateMachine.ChangeState(enemy.battleState);
    }
}
