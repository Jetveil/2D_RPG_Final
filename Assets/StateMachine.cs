using UnityEngine;

/// <summary>
/// Управляет активным состоянием конечного автомата (FSM):
/// хранит текущее состояние, инициализирует стартовое и переключает между состояниями.
/// </summary>
public class StateMachine
{
    public EntityState currentState { get; private set; }

    /// <summary>
    /// Задаёт стартовое состояние и выполняет его Enter.
    /// </summary>
    public void Initialize(EntityState startState)
    {
        currentState = startState;
        currentState.Enter();
    }
    /// <summary>
    /// Вызывает Exit у текущего и Enter у нового состояния, обновляя текущее.
    /// </summary>
    public void ChangeState(EntityState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
    /// <summary>
    /// Делегирует обновление текущему состоянию.
    /// </summary>
    public void UpdateActiveState()
    {
        currentState.Update();
    }
}