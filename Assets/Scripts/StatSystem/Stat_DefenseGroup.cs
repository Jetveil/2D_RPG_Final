using System;
using UnityEngine;

[Serializable]
public class Stat_DefenseGroup
{
    // Physical defense
    public Stat evasion;
    public Stat armor;

    // Elemental resistance
    public Stat iceRes;
    public Stat fireRes;
    public Stat lightningRes;
}