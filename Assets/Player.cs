using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Игрок: хранит ссылки на состояния и компоненты,
/// и делегирует работу активному состоянию FSM.
/// </summary>
public class Player : MonoBehaviour
{
    public PlayerInputSet input { get; private set; }
    public Vector2 moveInput { get; private set; }

    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }

    [Header("Movement Settings")]
    public float moveSpeed;
    public float jumpForce = 5f;
    [Range(0, 1)]
    public float inAirMoveMultiplier = 0.7f;
    public float wallSlideSlowMultiplier = 0.7f;
    private bool isFacingRight = true;
    public int facingDir { get; private set; } = 1;
    public Vector2 wallJumpForce;
    [Space]
    public float dashDuration = .25f;
    public float dashSpeed = 20;

    [Header("Collision Detection")]
    [SerializeField]
    private float groundCheckDistance;
    [SerializeField]
    private float wallCheckDistance;
    [SerializeField]
    private LayerMask whatIsGround;
    public bool groundDetected { get; private set; }
    public bool wallDetected { get; private set; }

    public StateMachine stateMachine { get; private set; }
    public Player_IdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallJumpState wallJumpState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_DashState dashState { get; private set; }

    /// <summary>
    /// Инициализация: создаём состояния и запускаем FSM.
    /// </summary>
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        input = new PlayerInputSet();
        stateMachine = new StateMachine();

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        jumpState = new Player_JumpState(this, stateMachine, "jumpFall");
        fallState = new Player_FallState(this, stateMachine, "jumpFall");
        wallSlideState = new Player_WallSlideState(this, stateMachine, "wallSlide");
        wallJumpState = new Player_WallJumpState(this, stateMachine, "jumpFall");
        dashState = new Player_DashState(this, stateMachine, "dash");
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

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }


    private void Update()
    {
        HandleCollisionDetection();
        stateMachine.UpdateActiveState();
    }

    public void SetVelocity(float xVelocity, float yVelocity)
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }

    private void HandleFlip(float xVelocity)
    {
        if (xVelocity > 0 && !isFacingRight)
            Flip();
        else if (xVelocity < 0 && isFacingRight)
            Flip();
    }

    public void Flip()
    {
        rb.transform.Rotate(0, 180, 0);
        isFacingRight = !isFacingRight;
        facingDir = facingDir * -1;
    }

    private void HandleCollisionDetection()
    {
        groundDetected = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(wallCheckDistance * facingDir, 0));
    }
}