using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Базовый класс для всех сущностей в игре. Содержит общие компоненты и функциональность, такие как аниматор, физика, машина состояний и управление направлением взгляда.
/// </summary>
public class Entity : MonoBehaviour
{
    public event Action OnFlipped;
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Entity_Stats stats { get; private set; }
    protected StateMachine stateMachine;
    private bool isFacingRight = true;
    public int facingDir { get; private set; } = 1;


    [Header("Collision Detection")]
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform secondaryWallCheck;
    public bool groundDetected { get; private set; }
    public bool wallDetected { get; private set; }

    // Condition variables
    private bool isKnocked;
    private Coroutine knockbackCoroutine;
    private Coroutine slowDownCo;


    /// <summary>
    /// Инициализация: создаём состояния и запускаем FSM.
    /// </summary>
    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();
        stats = GetComponent<Entity_Stats>();
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        HandleCollisionDetection();
        stateMachine.UpdateActiveState();
    }


    public void CallAnimationTrigger()
    {
        stateMachine.currentState.AnimationTrigger();
    }

    /// <summary>
    /// Хук на смерть сущности: переопределяется наследниками для реакции на 0 HP.
    /// </summary>
    public virtual void EntityDeath()
    {
    }

    /// <summary>
    /// Применяет временное замедление параметров сущности на заданную длительность.
    /// </summary>
    /// <param name="duration">Длительность эффекта в секундах.</param>
    /// <param name="slowMultiplier">Доля замедления [0..1].</param>
    public virtual void SlowDownEntity(float duration, float slowMultiplier)
    {
        if (slowDownCo != null)
            StopCoroutine(slowDownCo);

        slowDownCo = StartCoroutine(SlowDownEntityCo(duration, slowMultiplier));
    }

    /// <summary>
    /// Короутина замедления: меняет параметры на время и затем восстанавливает.
    /// </summary>
    protected virtual IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
        yield return null;
    }

    /// <summary>
    /// Применяет нокбэк с силой и длительностью, используя Rigidbody2D.
    /// </summary>
    public void ReceiveKnockback(Vector2 knockback, float duration)
    {
        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);
        knockbackCoroutine = StartCoroutine(KnockbackCo(knockback, duration));
    }

    /// <summary>
    /// Короутина нокбэка: задаёт скорость на время и сбрасывает её.
    /// </summary>
    private IEnumerator KnockbackCo(Vector2 knockback, float duration)
    {
        isKnocked = true;
        rb.linearVelocity = knockback;
        yield return new WaitForSeconds(duration);
        rb.linearVelocity = Vector2.zero;
        isKnocked = false;
    }

    /// <summary>
    /// Устанавливает скорость по осям и выполняет авто-поворот по знаку X.
    /// </summary>
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isKnocked)
            return;

        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }

    /// <summary>
    /// Проверяет, требуется ли поворот по направлению скорости по X, и флипает при необходимости.
    /// </summary>
    public void HandleFlip(float xVelocity)
    {
        if (xVelocity > 0 && !isFacingRight)
            Flip();
        else if (xVelocity < 0 && isFacingRight)
            Flip();
    }

    /// <summary>
    /// Поворот спрайта на 180 по Y, инверсия facingDir и событие OnFlipped.
    /// </summary>
    public void Flip()
    {
        rb.transform.Rotate(0, 180, 0);
        isFacingRight = !isFacingRight;
        facingDir = facingDir * -1;

        OnFlipped?.Invoke();
    }

    /// <summary>
    /// Обновляет флаги касания земли и стены лучами относительно точек чеков.
    /// </summary>
    private void HandleCollisionDetection()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);

        if (secondaryWallCheck != null)
        {
            wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround)
                           && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        }
        else
            wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));

        if (secondaryWallCheck != null)
            Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
    }
}