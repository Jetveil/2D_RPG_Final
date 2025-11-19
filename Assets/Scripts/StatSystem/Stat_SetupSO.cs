using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Default Stat Setup", fileName = "Default Stat Setup")]
public class Stat_SetupSO : ScriptableObject
{
    [Header("Resources")]
    public float maxHealth = 100f;
    public float healthRegen;

    [Header("Major Stats")]
    public float strength;
    public float agility;
    public float intelligence;
    public float vitality;

    [Header("Offense - Physical Damage")]
    public float damage = 10f;
    public float attackSpeed = 1f;
    public float critChance;
    public float critPower = 150f;
    public float armorReduction;

    [Header("Offence - Elemental Damage")]
    public float fireDamage;
    public float iceDamage;
    public float lightningDamage;

    [Header("Defence - Physical Damage")]
    public float evasion;
    public float armor;

    [Header("Defence - Elemental Damage")]
    public float fireResistance;
    public float iceResistance;
    public float lightningResistance;
}