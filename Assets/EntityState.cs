using UnityEngine;

/// <summary>
/// Базовое состояние сущности: задаёт шаблон для всех конкретных состояний.
/// </summary>
public abstract class EntityState
{
    protected Player player;
    protected StateMachine stateMachine;
    protected string animBoolName;
    
    protected Animator anim;
    protected Rigidbody2D rb;
    protected PlayerInputSet input;


    public EntityState(Player player, StateMachine stateMachine, string animBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
        
        anim = player.anim;
        rb = player.rb;
        input = player.input;
    }

    /// <summary>
    /// Инициализация при входе в состояние (анимации, сброс таймеров).
    /// </summary>
    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);
    }


    public virtual void Update()
    {
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    /// <summary>
    /// Очистка при выходе из состояния (сброс триггеров, остановка эффектов).
    /// </summary>
    public virtual void Exit()
    {
        anim.SetBool(animBoolName, false);
    }
}