using UnityEngine;

public class Enemy_VFX : Entity_VFX
{
    [Header("Attack Alert Window")]
    [SerializeField]
    private GameObject attackAlert;

    public void EnableAttackAlert(bool enable) => attackAlert.SetActive(enable);
}