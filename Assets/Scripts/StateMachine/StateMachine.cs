using UnityEngine;

/// <summary>
/// Управляет активным состоянием конечного автомата (FSM):
/// хранит текущее состояние, инициализирует стартовое и переключает между состояниями.
/// </summary>
public class StateMachine
{
    public EntityState currentState { get; private set; }
    public bool canChangeState;

    /// <summary>
    /// Задаёт стартовое состояние и выполняет его Enter.
    /// </summary>
    /// <summary>
    /// Инициализирует FSM стартовым состоянием и вызывает его Enter.
    /// </summary>
    public void Initialize(EntityState startState)
    {
        canChangeState = true;
        currentState = startState;
        currentState.Enter();
    }

    /// <summary>
    /// Вызывает Exit у текущего и Enter у нового состояния, обновляя текущее.
    /// </summary>
    /// <summary>
    /// Переключает текущее состояние: Exit старого и Enter нового.
    /// </summary>
    public void ChangeState(EntityState newState)
    {
        if (canChangeState == false)
            return;

        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    /// <summary>
    /// Делегирует обновление текущему состоянию.
    /// </summary>
    /// <summary>
    /// Делегирует кадр активному состоянию.
    /// </summary>
    public void UpdateActiveState()
    {
        currentState.Update();
    }

    /// <summary>
    /// Запрещает дальнейшие смены состояний.
    /// </summary>
    public void SwitchOffStateMachine() => canChangeState = false;
}
