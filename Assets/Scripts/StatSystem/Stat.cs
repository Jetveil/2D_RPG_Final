using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    [SerializeField] private List<StatModifier> modifiers = new List<StatModifier>();

    private bool needRecalculate = true;
    private float finalValue;

    /// <summary>
    /// Возвращает текущее базовое значение стата.
    /// </summary>
    public float GetValue()
    {
        if (needRecalculate)
        {
            finalValue = GetFinalValue();
            needRecalculate = false;
        }

        return finalValue;
    }

    public void AddModifier(float value, string source)
    {
        StatModifier modToAdd = new StatModifier(value, source);
        modifiers.Add(modToAdd);
        needRecalculate = true;
    }

    public void RemoveModifier(string source)
    {
        modifiers.RemoveAll(modifiers => modifiers.source == source);
        needRecalculate = true;
    }

    private float GetFinalValue()
    {
        float finalValue = baseValue;

        foreach (var modifier in modifiers)
        {
            finalValue = finalValue + modifier.value;
        }

        return finalValue;
    }

    public void SetBaseValue(float value) => baseValue = value;
}

[Serializable]
public class StatModifier
{
    public float value;
    public string source;

    public StatModifier(float value, string source)
    {
        this.value = value;
        this.source = source;
    }
}