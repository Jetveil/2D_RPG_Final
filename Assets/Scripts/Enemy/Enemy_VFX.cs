using UnityEngine;

public class Enemy_VFX : Entity_VFX
{
    [Header("Attack Alert Window")]
    [SerializeField]
    private GameObject attackAlert;

    /// <summary>
    /// Включает/выключает индикатор окна атаки у врага.
    /// </summary>
    public void EnableAttackAlert(bool enable) => attackAlert.SetActive(enable);
}
