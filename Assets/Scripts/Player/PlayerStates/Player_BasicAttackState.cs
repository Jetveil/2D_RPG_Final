using UnityEngine;

public class Player_BasicAttackState : PlayerState
{
    public Player_BasicAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        if (comboLimit != player.attackVelocity.Length)
        {
            Debug.LogWarning("Combo limit have been adjusted to attack velocity array");
            comboLimit = player.attackVelocity.Length;
        }
    }

    private float attackVelocityTimer;
    private float lastTimeAttacked;

    private bool comboAttackQueued;
    private int attackDir;
    private const int FirstComboIndex = 1;
    private int comboLimit = 3;
    private int comboIndex = 1;


    /// <summary>
    /// Подготавливает удар: индекс комбо, направление и аним-параметры.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        ResetComboIndexIfNeeded();
        SyncAttackSpeed();

        attackDir = player.moveInput.x != 0 ? (int)player.moveInput.x : player.facingDir;


        anim.SetInteger("basicAttackIndex", comboIndex);
        ApplyAttackVelocity();
    }


    /// <summary>
    /// Ведёт таймер импульса, принимает очередь следующего удара и решает выход.
    /// </summary>
    public override void Update()
    {
        base.Update();
        HandleAttackVelocity();

        if (input.Player.Attack.WasPressedThisFrame())
            QueueNextAttack();

        if (triggerCalled)
        {
            HandleStateExit();
        }
    }

    /// <summary>
    /// Завершение: либо продолжает комбо, либо возвращается в idle.
    /// </summary>
    private void HandleStateExit()
    {
        if (comboAttackQueued)
        {
            comboAttackQueued = false;
            anim.SetBool(animBoolName, false);
            player.EnterAttackStateWithDelay();
        }
        else
            stateMachine.ChangeState(player.idleState);
    }

    /// <summary>
    /// При выходе увеличивает индекс комбо и фиксирует время удара.
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        comboIndex++;
        lastTimeAttacked = Time.time;
    }

    /// <summary>
    /// Ставит флажок для продолжения комбо, если лимит не достигнут.
    /// </summary>
    private void QueueNextAttack()
    {
        if (comboIndex < comboLimit)
            comboAttackQueued = true;
    }

    /// <summary>
    /// Ведёт таймер импульса и сбрасывает скорость по его окончании.
    /// </summary>
    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;

        if (attackVelocityTimer < 0)
            player.SetVelocity(0, rb.linearVelocity.y);
    }

    /// <summary>
    /// Применяет импульс текущего удара с учётом направления.
    /// </summary>
    private void ApplyAttackVelocity()
    {
        Vector2 attackVelocity = player.attackVelocity[comboIndex - 1];

        attackVelocityTimer = player.attackVelocityDuration;
        player.SetVelocity(attackVelocity.x * attackDir, attackVelocity.y);
    }

    /// <summary>
    /// Сбрасывает индекс комбо, если окно между ударами было пропущено.
    /// </summary>
    private void ResetComboIndexIfNeeded()
    {
        if (Time.time > lastTimeAttacked + player.comboResetTime)
            comboIndex = FirstComboIndex;

        if (comboIndex > comboLimit)
            comboIndex = FirstComboIndex;
    }
}
