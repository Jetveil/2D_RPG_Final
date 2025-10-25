using UnityEngine;

public class Entity_Stats : MonoBehaviour
{
    public Stat maxHP;
    public Stat vitality; // increases maxHP by 5 per unit

    public float GetMaxHealth()
    {
        float baseHP = maxHP.GetValue();
        float bonusHP = vitality.GetValue() * 5;

        return baseHP + bonusHP;
    }
}