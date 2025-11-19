using UnityEngine;

/// <summary>
/// Объект, который можно контратаковать (с возможностью оглушения).
/// </summary>
public interface ICounterable
{
    /// <summary>
    /// Признак активного окна уязвимости к контрудару.
    /// </summary>
    public bool CanBeCountered { get; }
    /// <summary>
    /// Обработка успешного контрудара.
    /// </summary>
    public void HandleCounter();
}
