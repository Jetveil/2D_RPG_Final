using UnityEngine;

public class Player_FallState : Player_AiredState
{
    public Player_FallState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// Подготовка к падению (настройки по необходимости).
    /// </summary>
    public override void Enter()
    {
        base.Enter();
    }

    /// <summary>
    /// Переходит в idle при приземлении или в wall-slide при касании стены.
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (player.groundDetected)
            stateMachine.ChangeState(player.idleState);

        if (player.wallDetected)
            stateMachine.ChangeState(player.wallSlideState);
    }
}
