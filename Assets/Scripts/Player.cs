using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Игрок: хранит ссылки на состояния и компоненты,
/// и делегирует работу активному состоянию FSM.
/// </summary>
public class Player : Entity
{
    public PlayerInputSet input { get; private set; }

    public Player_IdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallJumpState wallJumpState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_DashState dashState { get; private set; }
    public Player_BasicAttackState basicAttackState { get; private set; }
    public Player_JumpAttack_State jumpAttackState { get; private set; }

    [Header("Attack Settings")]
    public Vector2[] attackVelocity;
    public Vector2 jumpAttackVelocity;
    public float attackVelocityDuration = .1f;
    public float comboResetTime;
    private Coroutine queuedAttackCo;

    [Header("Movement Settings")]
    public float moveSpeed;
    public float jumpForce = 5f;
    [Range(0, 1)]
    public float inAirMoveMultiplier = 0.7f;
    public float wallSlideSlowMultiplier = 0.7f;
    public Vector2 wallJumpForce;
    [Space]
    public float dashDuration = .25f;
    public float dashSpeed = 20;
    public Vector2 moveInput { get; private set; }


    protected override void Awake()
    {
        base.Awake();

        input = new PlayerInputSet();

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        jumpState = new Player_JumpState(this, stateMachine, "jumpFall");
        fallState = new Player_FallState(this, stateMachine, "jumpFall");
        wallSlideState = new Player_WallSlideState(this, stateMachine, "wallSlide");
        wallJumpState = new Player_WallJumpState(this, stateMachine, "jumpFall");
        dashState = new Player_DashState(this, stateMachine, "dash");
        basicAttackState = new Player_BasicAttackState(this, stateMachine, "basicAttack");
        jumpAttackState = new Player_JumpAttack_State(this, stateMachine, "jumpAttack");
    }

    override protected void Start()
    {
        stateMachine.Initialize(idleState);
    }

    public void EnterAttackStateWithDelay()
    {
        if (queuedAttackCo != null)
            StopCoroutine(queuedAttackCo);

        queuedAttackCo = StartCoroutine(EnterAttackStateWIthDelayCo());
    }

    private IEnumerator EnterAttackStateWIthDelayCo()
    {
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(basicAttackState);
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        input.Disable();
    }
}