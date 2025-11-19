using UnityEngine;

public class Player_CounterAttackState : PlayerState
{
    private Player_Combat combat;
    private bool counteredSomebody;

    public Player_CounterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat = player.GetComponent<Player_Combat>();
    }

    /// <summary>
    /// Запускает окно контрудара, фиксирует успех и настраивает анимацию.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        stateTimer = combat.GetCounterRecoveryDuration();
        counteredSomebody = combat.CounterAttackPerformed();

        anim.SetBool("counterAttackPerformed", counteredSomebody);
    }

    /// <summary>
    /// Держит игрока на месте; выход по триггеру анимации или по таймауту.
    /// </summary>
    public override void Update()
    {
        base.Update();

        player.SetVelocity(0, rb.linearVelocity.y);

        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);

        if (stateTimer < 0 && counteredSomebody == false)
            stateMachine.ChangeState(player.idleState);
    }
}
