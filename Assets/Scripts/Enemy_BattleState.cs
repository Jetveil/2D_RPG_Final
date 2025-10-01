using UnityEngine;

public class Enemy_BattleState : EnemyState
{
    private Transform player;
    public float lastTimeWasInBattle;

    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null)
            player = enemy.PlayerDetected().transform;

        if (ShouldRetreat())
        {
            rb.linearVelocity = new Vector2(enemy.retreatVelocity.x * -DirectionToPlayer(), enemy.retreatVelocity.y);
            enemy.HandleFlip(DirectionToPlayer());
        }
    }

    public override void Update()
    {
        base.Update();

        if (enemy.PlayerDetected())
            UpdateBattleTimer();

        if (BattleTimeIsOver())
            stateMachine.ChangeState(enemy.idleState);

        if (WithinAttackRange() && enemy.PlayerDetected())
            stateMachine.ChangeState(enemy.attackState);
        else
            enemy.SetVelocity(enemy.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }


    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;

    private bool BattleTimeIsOver() => Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;

    private bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;


    private float DistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        float dx = player.position.x - enemy.transform.position.x;

        return Mathf.Abs(dx); // именно горизонтальная дистанция до игрока
    }

// private bool IsPlayerInVerticalRange()
// {
//     if (player == null)
//         return false;
//
//     float maxVerticalDistance = 2f;
//
//     float verticalDistance = Mathf.Abs(player.position.y - enemy.transform.position.y);
//     return verticalDistance <= maxVerticalDistance;
// }

    private bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;

    private int DirectionToPlayer()
    {
        if (player == null) return 0;

        float dx = player.position.x - enemy.transform.position.x;
        const float deadZone = 0.15f;
        
        if (Mathf.Abs(dx) < deadZone)
            return 0; // «над головой» — не принимаем сторону

        return dx > 0f ? 1 : -1;
    }
}