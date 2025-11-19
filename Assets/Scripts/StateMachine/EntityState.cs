using UnityEngine;

/// <summary>
/// Базовое состояние сущности: задаёт шаблон для всех конкретных состояний.
/// </summary>
public abstract class EntityState
{
    protected StateMachine stateMachine;
    protected string animBoolName;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Entity_Stats stats;


    [SerializeField] protected float stateTimer;
    protected bool triggerCalled;

    public EntityState(StateMachine stateMachine, string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    /// <summary>
    /// Вход в состояние: включает аниматорный флаг и сбрасывает триггеры.
    /// </summary>
    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    /// <summary>
    /// Обновление состояния: тикает таймер и обновляет аним-параметры.
    /// </summary>
    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParams();
    }

    /// <summary>
    /// Очистка при выходе из состояния (сброс триггеров, остановка эффектов).
    /// </summary>
    /// <summary>
    /// Выход из состояния: выключает аниматорный флаг.
    /// </summary>
    public virtual void Exit()
    {
        anim.SetBool(animBoolName, false);
    }

    /// <summary>
    /// Вызывается анимацией: помечает, что триггер состояния сработал.
    /// </summary>
    public void AnimationTrigger()
    {
        triggerCalled = true;
    }

    /// <summary>
    /// Хук для установки параметров Animator из состояния.
    /// </summary>
    public virtual void UpdateAnimationParams()
    {
    }

    public void SyncAttackSpeed()
    {
        float attackSpeed = stats.offense.attackSpeed.GetValue();
        anim.SetFloat("attackSpeedMultiplier", attackSpeed);
    }
}