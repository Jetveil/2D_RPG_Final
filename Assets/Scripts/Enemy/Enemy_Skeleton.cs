using UnityEngine;

public class Enemy_Skeleton : Enemy, ICounterable
{
    /// <summary>
    /// Показывает, может ли враг быть оглушён контрударом в текущий момент.
    /// </summary>
    public bool CanBeCountered
    {
        get => canBeStunned;
    }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        battleState = new Enemy_BattleState(this, stateMachine, "battle");
        deadState = new Enemy_DeadState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
    }


    [ContextMenu("Stun Enemy")]
    /// <summary>
    /// Обрабатывает успешный контрудар: переводит врага в оглушение.
    /// </summary>
    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }
}
