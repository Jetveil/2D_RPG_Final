using System;
using System.Collections;
using UnityEngine;

public class Entity_StatusHandler : MonoBehaviour
{
    private ElementType currentEffect = ElementType.None;

    private Entity entity;
    private Entity_VFX entityVFX;
    private Entity_Stats entityStats;
    private Entity_Health entityHealth;

    [Header("Electrify Effect Details")]
    [SerializeField] private GameObject lightningStrikeVfx;
    [SerializeField] private float currentCharge;
    [SerializeField] private float maximumCharge = 1;
    private Coroutine electrifyCo;

    /// <summary>
    /// Кэширует ссылки на компоненты <see cref="Entity_Stats"/>, <see cref="Entity"/>,
    /// <see cref="Entity_VFX"/> и <see cref="Entity_Health"/>.
    /// </summary>
    private void Awake()
    {
        entityStats = GetComponent<Entity_Stats>();
        entity = GetComponent<Entity>();
        entityVFX = GetComponent<Entity_VFX>();
        entityHealth = GetComponent<Entity_Health>();
    }

    /// <summary>
    /// Применяет/накапливает эффект Электричества: учитывает резист, накапливает заряд и
    /// при достижении порога вызывает удар молнии; иначе запускает корутину поддержания статуса.
    /// </summary>
    /// <param name="duration">Длительность статуса, сек.</param>
    /// <param name="damage">Урон удара молнии при срабатывании.</param>
    /// <param name="charge">Прирост заряда до учёта резиста [0..1].</param>
    public void ApplyElectrifyEffect(float duration, float damage, float charge)
    {
        float lightningResistance = entityStats.GetElementalResistance(ElementType.Lightning);
        float finalCharge = charge * (1 - lightningResistance);

        currentCharge = currentCharge + finalCharge;

        if (currentCharge >= maximumCharge)
        {
            DoLightningStrike(damage);
            StopElectrifyEffect();
            return;
        }

        if (electrifyCo != null)
            StopCoroutine(electrifyCo);

        electrifyCo = StartCoroutine(ElectrifyEffectCo(duration));
    }

    /// <summary>
    /// Сбрасывает электрический эффект: очищает статус, обнуляет заряд и останавливает VFX.
    /// </summary>
    private void StopElectrifyEffect()
    {
        currentEffect = ElementType.None;
        currentCharge = 0;
        entityVFX.StopAllVfx();
    }

    /// <summary>
    /// Вызывает удар молнии: создаёт VFX и наносит мгновенный урон по здоровью.
    /// </summary>
    /// <param name="damage">Величина наносимого урона.</param>
    private void DoLightningStrike(float damage)
    {
        Instantiate(lightningStrikeVfx, transform.position, Quaternion.identity);
        entityHealth.ReduceHealth(damage);
    }

    /// <summary>
    /// Короутина поддержки эффекта Электричества: помечает статус, проигрывает VFX
    /// и снимает эффект по истечении длительности.
    /// </summary>
    /// <param name="duration">Длительность статуса, сек.</param>
    private IEnumerator ElectrifyEffectCo(float duration)
    {
        currentEffect = ElementType.Lightning;
        entityVFX.PlayOnStatusVFX(duration, ElementType.Lightning);

        yield return new WaitForSeconds(duration);
        StopElectrifyEffect();
    }

    /// <summary>
    /// Применяет эффект Горения: учитывает огненный резист и запускает корутину дота.
    /// </summary>
    /// <param name="duration">Длительность статуса, сек.</param>
    /// <param name="fireDamage">Суммарный урон до учёта резиста.</param>
    public void ApplyBurnEffect(float duration, float fireDamage)
    {
        float fireResistance = entityStats.GetElementalResistance(ElementType.Fire);
        float finalDamage = fireDamage * (1 - fireResistance);

        StartCoroutine(BurnEffectCo(duration, finalDamage));
    }

    /// <summary>
    /// Короутина огненного дота-эффекта: помечает статус, запускает VFX и тикание урона.
    /// </summary>
    private IEnumerator BurnEffectCo(float duration, float totalDamage)
    {
        currentEffect = ElementType.Fire;
        entityVFX.PlayOnStatusVFX(duration, ElementType.Fire);

        int ticksPerSecond = 2;
        int tickCount = Mathf.RoundToInt(ticksPerSecond * duration);

        float damagePerTick = totalDamage / tickCount;
        float tickInterval = 1f / ticksPerSecond;

        for (int i = 0; i < tickCount; i++)
        {
            entityHealth.ReduceHealth(damagePerTick);
            yield return new WaitForSeconds(tickInterval);
        }

        currentEffect = ElementType.None;
    }

    /// <summary>
    /// Применяет эффект холода: рассчитывает длительность с учётом резиста и запускает корутину.
    /// </summary>
    public void ApplyChillEffect(float duration, float slowMultiplier)
    {
        float iceResistance = entityStats.GetElementalResistance(ElementType.Ice);
        float finalDuration = duration * (1 - iceResistance);
        StartCoroutine(ChillEffectCo(finalDuration, slowMultiplier));
    }

    /// <summary>
    /// Короутина холода: замедляет сущность, проигрывает VFX и снимает статус по окончании.
    /// </summary>
    private IEnumerator ChillEffectCo(float duration, float slowMultiplier)
    {
        entity.SlowDownEntity(duration, slowMultiplier);
        currentEffect = ElementType.Ice;
        entityVFX.PlayOnStatusVFX(duration, ElementType.Ice);

        yield return new WaitForSeconds(duration);
        currentEffect = ElementType.None;
    }

    /// <summary>
    /// Возвращает true, если нет активного статус-эффекта, и можно наложить новый.
    /// </summary>
    public bool CanBeApplied(ElementType element)
    {
        if (element == ElementType.Lightning && currentEffect == ElementType.Lightning)
            return true;

        return currentEffect == ElementType.None;
    }
}
