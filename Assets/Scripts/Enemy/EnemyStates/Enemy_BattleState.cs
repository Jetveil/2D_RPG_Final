using UnityEngine;

public class Enemy_BattleState : EnemyState
{
    private Transform player;
    public float lastTimeWasInBattle;

    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// Вход в бой: обновляет таймер, находит игрока и при необходимости отступает.
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        
        UpdateBattleTimer();

        if (player == null)
            player = enemy.GetPlayerReference();

        if (ShouldRetreat())
        {
            rb.linearVelocity = new Vector2(enemy.retreatVelocity.x * -DirectionToPlayer(), enemy.retreatVelocity.y);
            enemy.HandleFlip(DirectionToPlayer());
        }
    }

    /// <summary>
    /// Логика боя: обновляет таймер, атакует в радиусе, иначе преследует.
    /// </summary>
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


    /// <summary>
    /// Обновляет время последнего контакта/обнаружения игрока.
    /// </summary>
    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;

    /// <summary>
    /// Проверяет, истёк ли таймер выхода из боя.
    /// </summary>
    private bool BattleTimeIsOver() => Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;

    /// <summary>
    /// Возвращает true, если игрок в радиусе атаки по X.
    /// </summary>
    private bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;


    /// <summary>
    /// Горизонтальная дистанция до игрока (по оси X).
    /// </summary>
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

    /// <summary>
    /// Требуется ли отступить, если игрок слишком близко.
    /// </summary>
    private bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;

    /// <summary>
    /// Возвращает направление к игроку: -1 влево, 1 вправо, 0 если в «мёртвой зоне».
    /// </summary>
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
