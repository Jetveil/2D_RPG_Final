using UnityEngine;

/// <summary>
/// Базовое состояние сущности: задаёт шаблон для всех конкретных состояний.
/// </summary>
public abstract class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSet input;
    protected Player_SkillManager skillManager;


    public PlayerState(Player player, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.player = player;

        anim = player.anim;
        rb = player.rb;
        input = player.input;
        stats = player.stats;
        skillManager = player.skillManager;
    }

    /// <summary>
    /// Базовое обновление игрока-состояния c обработкой нажатия Dash.
    /// </summary>
    public override void Update()
    {
        base.Update();


        if (input.Player.Dash.WasPerformedThisFrame() && CanDash())
        {
            skillManager.dash.SetSkillOnCooldown();
            stateMachine.ChangeState(player.dashState);
        }
    }

    /// <summary>
    /// Устанавливает вертикальную скорость в параметры анимации.
    /// </summary>
    public override void UpdateAnimationParams()
    {
        base.UpdateAnimationParams();
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    /// <summary>
    /// Возвращает true, если не у стены и не в состоянии Dash.
    /// </summary>
    private bool CanDash()
    {
        if (skillManager.dash.CanUseSkill() == false)
            return false;

        if (player.wallDetected || stateMachine.currentState == player.dashState)
            return false;
        return true;
    }
}