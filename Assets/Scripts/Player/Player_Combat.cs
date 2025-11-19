using UnityEngine;

public class Player_Combat : Entity_Combat
{
    [Header("Counter Attack Details")]
    [SerializeField] private float counterRecovery = .2f;

    /// <summary>
    /// Пытается выполнить контрудар по целям в зоне; возвращает успех.
    /// </summary>
    public bool CounterAttackPerformed()
    {
        bool hasPerformedCounter = false;

        foreach (var target in GetDetectedColliders())
        {
            ICounterable counterable = target.GetComponent<ICounterable>();

            if (counterable == null)
                continue;

            if (counterable.CanBeCountered)
            {
                counterable.HandleCounter();
                hasPerformedCounter = true;
            }
        }

        return hasPerformedCounter;
    }

    /// <summary>
    /// Возвращает длительность восстановления после контрудара.
    /// </summary>
    public float GetCounterRecoveryDuration() => counterRecovery;
}
