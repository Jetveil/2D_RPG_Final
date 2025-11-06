## Почему maxHP 170, а HP 135? — ${ts}

**Текст заданного вопроса**
- Assets/Scripts/Entity_Stats.cs Почему maxHP становится 170, а currentHealth отрабатывает  
  корректно и равно 135?

**Доступное и подробное объяснение**
- В `Assets/Scripts/Entity_Stats.cs` метод `GetMaxHealth()` делает `maxHP += vitality * 5;`. Из-за этого КАЖДЫЙ вызов увеличивает поле `maxHP` на `vitality * 5`.
- Последовательность: первый вызов (например, в `Entity_Health.Awake()`) даёт `100 + 7*5 = 135` и присваивается в `currentHealth`. При следующих вызовах (например, обновление `Slider` в UI) `GetMaxHealth()` снова меняет `maxHP`: 135 → 170 → 205 и т.д.
- Почему `currentHealth` выглядит правильным: его задали один раз (135) и дальше не меняли. Растёт только `maxHP`, потому что метод мутирует состояние.
- Аналогия: вместо «посчитать», вы «прибавляете к счётчику» — число растёт само, даже если параметры не изменились.

**Исправление**
```csharp
// Assets/Scripts/Entity_Stats.cs
using UnityEngine;

public class Entity_Stats : MonoBehaviour
{
    public float maxHP = 100;   // Базовое здоровье
    public float vitality = 7;  // Каждая единица даёт +5 к максимуму

    public float GetMaxHealth()
    {
        // Минимальный фикс: вернуть чистый расчёт без побочных эффектов
        // Было: maxHP += vitality * 5;
        return maxHP + vitality * 5f;
    }
}
```
- Почему сработает: метод станет «чистым» — перестанет менять поле `maxHP`. Все места будут получать одно и то же корректное значение `135` при заданных статах.

**Улучшения**
- По запросу: кэшировать максимум при старте (`cachedMax = stats.GetMaxHealth();`) и использовать в UI (`healthBar.maxValue = cachedMax;`), чтобы избегать повторных вычислений.
---
## Почему доступен тип Stat в коде? — 2025-10-25 15:27:23

**Текст заданного вопроса**
- Assets/Scripts/StatSystem/Stat_MajorGroup.cs почему я могу получить доступ к Stat? Потому что класс public?

**Доступное и подробное объяснение**
- В проекте (без `.asmdef`) все скрипты собираются в один Assembly `Assembly-CSharp`. В C# верхнеуровневые классы без модификатора по умолчанию `internal`. Такие типы доступны в пределах того же assembly. Поэтому даже если бы `Stat` НЕ был `public`, но оставался `internal` — его всё равно можно было бы использовать из других ваших скриптов.
- У вас `Stat` объявлен как `public` и помечен `[Serializable]` (`Assets/Scripts/StatSystem/Stat.cs`). Это даёт два эффекта:
  - Доступ из любого другого assembly (если появятся `.asmdef` или внешние плагины).
  - Сериализацию в инспекторе Unity, когда он встречается как поле в другом сериализуемом типе.
- Почему видно в инспекторе поля `strength/agility/...`? Потому что:
  - `Stat_MajorGroup` помечен `[Serializable]` и (как класс) доступен сериализации.
  - Поля `public Stat strength;` и т.п. — публичные, значит Unity сериализует их по умолчанию (либо можно было сделать `private` + `[SerializeField]`).

Итого: вы можете «достучаться» до `Stat` не только потому что класс `public`, а потому что скрипты в одном сборочном контексте (`Assembly-CSharp`). `public` станет важно, если вынесете часть кода в отдельную сборку через `.asmdef` — тогда `internal` перестанет быть виден снаружи.

**Исправление**
```csharp
// Ничего править не обязательно. Но если хотите ограничить доступ внутри сборки:
// Assets/Scripts/StatSystem/Stat.cs
using System;

[Serializable]
internal class Stat
{
    // ... оставьте как есть, Unity продолжит сериализовать, 
    // т.к. сериализация не зависит от public/internal у самого типа,
    // важна аннотация [Serializable] и доступность поля-холдера.
}
```

Если в будущем добавите `.asmdef` и разобьёте код на несколько сборок — верните `public` для типов, которыми делитесь между сборками.
---
## Что такое asmdef в Unity? — 2025-10-25 15:30:36

**Текст заданного вопроса**
- что такое asmdef?

**Доступное и подробное объяснение**
- Определение:  — это Assembly Definition (описание сборки). Такой файлик говорит Unity: «все скрипты в этой папке (и ниже, пока не встретится другой ) — это отдельная C#‑сборка».
- Зачем нужно:
  - Ускоряет компиляцию — меняется файл в одной сборке → пересобирается только она, а не весь проект.
  - Инкапсуляция — вы чётко делите код на модули и управляете, кто на кого ссылается.
  - Контроль зависимостей — в  вы явно указываете ссылки (Assembly Definition References). Нет ссылки — нет доступа.
  - Платформы/режимы — можно помечать сборку как Editor‑only, ограничивать платформы и задавать Define Constraints/Version Defines.
  - Тесты — для тестов создают отдельные test‑assemblies (галка Test Assemblies), чтобы Unity Test Framework их видел.
- Как работает в проекте:
  - Без  весь код идёт в одну сборку  (и ). Потому и «видно всё отовсюду», даже .
  - С  классы становятся изолированными по сборкам. Чтобы один модуль видел типы другого, добавьте явную ссылку в разделе References. И вот тут модификатор  уже обязателен (иначе внешний модуль не увидит тип).
- Ключевые поля :
  - Name — имя сборки.
  - References — список сборок, на которые можно ссылаться.
  - Auto Referenced — если включено, другие сборки автоматически получают ссылку (обычно лучше выключить для библиотек, чтобы зависимости были явными).
  - Include/Exclude Platforms — где эта сборка компилируется.
  - Define Constraints, Version Defines — условия компиляции/включения кода.
  - Root Namespace — опционально, корневое пространство имён для новых скриптов.
- Связанные файлы:
  -  (Assembly Definition Reference) — «указатель» на уже существующую сборку/плагин, чтобы сослаться на него из другой папки.
  -  — в настройках  есть список «Friend assemblies», чтобы разрешить доступ к  типам из дружественной сборки (редкий, но полезный кейс для тестов).

Практический совет: делите код минимум на  и  сборки; UI/Gameplay/Tools — по мере роста. При первой разбивке будьте готовы добавить явные References и сделать публичными типы, которыми делитесь между модулями.
---
## Как работает Stat.GetValue в GetMaxHealth — ${ts}

**Текст заданного вопроса**
- Assets/Scripts/StatSystem/Stat.cs как работает метод getvalue? Например, в GetMaxHealth

**Доступное и подробное объяснение**
- В `Assets/Scripts/StatSystem/Stat.cs` метод `GetValue()` просто возвращает сериализованное поле `baseValue`:
```csharp
// Assets/Scripts/StatSystem/Stat.cs
[Serializable]
public class Stat
{
    [SerializeField] private float baseValue;

    public float GetValue()
    {
        return baseValue; // значение, заданное в инспекторе у этого Stat
    }
}
```
- В `Assets/Scripts/Entity/Entity_Stats.cs` метод `GetMaxHealth()` берёт два значения через `GetValue()` и считает формулу без побочных эффектов:
```csharp
// Assets/Scripts/Entity/Entity_Stats.cs (фрагмент)
public float GetMaxHealth()
{
    float baseHP = maxHealth.GetValue();     // напр., 100 из инспектора
    float bonusHP = major.vitality.GetValue() * 5; // напр., 7 * 5 = 35
    return baseHP + bonusHP; // 135
}
```
- Откуда берётся число: `baseValue` хранится в сцене/префабе (Unity сериализует класс `Stat`, т.к. он `[Serializable]`). В инспекторе вы задаёте `baseValue` для каждого поля типа `Stat` — например, `maxHealth` и `major.vitality`.
- Важно: сейчас `Stat` не имеет модификаторов/баффов — это «голое» значение. Поэтому `GetValue()` не суммирует/не умножает ничего сверху; оно всегда возвращает базу, которую вы выставили.
- Подводные камни: убедитесь, что ссылки `maxHealth` и `major`/`vitality` назначены в инспекторе; иначе возможен `NullReferenceException` при вызове `GetValue()`.
---
## Serializable vs SerializeField в Unity — 2025-10-26 11:22:10

**Текст заданного вопроса**
- Что значит Serializable класс в контексте юнити? Чем этот аттрибут отличается от serializedfield?

**Доступное и подробное объяснение**
- [Serializable] — атрибут для ТИПА (class/struct). Делает тип «подходящим для сериализации» Unity. Это нужно, чтобы Unity могла сохранять и показывать в инспекторе ПОЛЯ этого типа, когда он встречается внутри ,  или другого сериализуемого типа.
- [SerializeField] — атрибут для ПОЛЯ. Говорит Unity сериализовать это поле даже если оно  (и показать его в инспекторе). Публичные поля сериализуются по умолчанию, если не помечены .

Ключевые отличия:
- Область действия:
  -  применяется к типу и разрешает Unity «залезать внутрь» экземпляров этого типа.
  -  применяется к конкретному полю, включив/исключив его из сериализации независимо от модификатора доступа.
- Зависимость:  не переезжает ограничение типа. Если тип НЕ сериализуемый (нет  и это не ), то даже поле с  не будет сериализовано и не отобразится.
- Инспектор:
  -  позволяет инспектору разворачивать вложенные поля вашего кастомного типа.
  -  делает приватные поля видимыми/сохраняемыми так же, как публичные.
- Что НЕ делает : не превращает класс в компонент/ресурс. Для отдельного ассета используйте , для компонента — наследуйте .

Мини-примеры:


Полезные дополнения:
- Исключить из сериализации:  — не сохранится и не покажется.
- Полиморфизм: для ссылочных полей со сменой подтипов используйте  (и собственный UI/PropertyDrawer).
- Ограничения сериализации Unity: сериализуются только поля, а не свойства; нет словарей (без костылей/новых версий), generics и абстрактные типы — с ограничениями; конструкторы не вызываются при десериализации.
---
## Почему defense.evasion видит GetValue()? — 2025-10-26 13:43:50

**Текст заданного вопроса**
- Assets/Scripts/Entity/Entity_Stats.cs почему поле evasion имеет доступ к GetValue()?

**Доступное и подробное объяснение**
- Поле `evasion` объявлено как тип `Stat` внутри `Stat_DefenseGroup` (см. `Assets/Scripts/StatSystem/Stat_DefenseGroup.cs`). А в `Stat` есть публичный метод `GetValue()` (см. `Assets/Scripts/StatSystem/Stat.cs`). В C# это значит: переменная типа `Stat` имеет доступ ко всем его доступным (public/internal) членам — в том числе к `GetValue()`.
- Доступность тут обеспечивает именно модификатор доступа и сборка, а не «инспектор» или `[Serializable]`. В вашем коде `GetValue()` объявлен как `public`, поэтому вызов возможен откуда угодно внутри проекта (и из других сборок, если будут ссылки).
- Почему поле вообще существует и видно в инспекторе: `Stat_DefenseGroup` помечен `[Serializable]`, а поля `evasion`, `armor`, и т.п. — публичные, значит Unity их сериализует. Но сериализация влияет на отображение/сохранение данных, а не на возможность вызывать методы.

Небольшая иллюстрация:
```csharp
// Assets/Scripts/StatSystem/Stat.cs
[System.Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    public float GetValue() => baseValue; // метод публичный → доступен как stat.GetValue()
}

// Assets/Scripts/StatSystem/Stat_DefenseGroup.cs
[System.Serializable]
public class Stat_DefenseGroup
{
    public Stat evasion; // поле типа Stat → есть метод GetValue()
}

// Assets/Scripts/Entity/Entity_Stats.cs (пример использования)
public class Entity_Stats : MonoBehaviour
{
    public Stat_DefenseGroup defense;

    public float GetEvasion()
    {
        return defense.evasion.GetValue(); // законный вызов: evasion — это Stat
    }
}
```

Если бы `GetValue()` был `internal`, вы всё равно могли бы вызывать его из этого проекта, потому что все скрипты без `.asmdef` собираются в одну сборку `Assembly-CSharp`. Потребуется `public`, когда начнёте делить код на отдельные сборки через `.asmdef`.
---
## Почему недоступны Range и stats в Entity_Health — 2025-10-26 14:42:40

**Текст заданного вопроса**
- Assets/Scripts/Entity/Entity_Health.cs почему недоступен метод Range и поле stats, хотя оно определено

**Доступное и подробное объяснение**
- Метод  принадлежит  (статический), а у  такого метода нет. В файле задан алиас  и закомментирован . Поэтому при обращении к  компилятор не находит метод.
- В строке  есть ещё две проблемы синтаксиса:
  - В лямбда‑выражении‑стрелке не пишется слово .
  - Вызов  невалиден: нужен метод , например .
- По полю : само поле объявлено верно () и инициализируется в . Но дальше вызывается , а такого метода в  нет (в файле  его действительно нет). Отсюда ощущение «недоступно/не найдено» — на самом деле не существует метода.
- Как правильно получить уклонение: либо добавить метод‑прокси в  (), либо обращаться к вложенному полю напрямую: .

**Исправление**


Альтернатива (если хочется ):


Готов применить правку в  (и при желании — добавить  в ). Подтвердите, пожалуйста.
---
## Почему недоступны Range и stats в Entity_Health — 2025-10-26 14:46:19

**Текст заданного вопроса**
- Assets/Scripts/Entity/Entity_Health.cs почему недоступен метод Range и поле stats, хотя оно определено

**Доступное и подробное объяснение**
- Метод `Range` — это статический метод класса `UnityEngine.Random`. В файле задан алиас `using Random = System.Random;` и закомментирован `using Random = UnityEngine.Random;`. У `System.Random` нет метода `Range`, поэтому вызов не находится. Используйте `UnityEngine.Random.Range` или верните алиас на `UnityEngine.Random`.
- В строке `private bool AttackEvaded() => return Random.(0, 100) < stats.GetEvasion();` есть синтаксические ошибки:
  - В expression-bodied методе не пишут слово `return`.
  - Вызов `Random.(0, 100)` некорректен — нужен `Random.Range(0f, 100f)`.
- По полю `stats`: поле объявлено корректно и инициализируется в `Awake()`. Но вызывается `stats.GetEvasion()`, а такого метода в `Entity_Stats` сейчас нет. Потому компилятор «ругается» не на доступ к полю, а на отсутствие метода. Правильнее либо обратиться к вложенному полю (`stats.defense.evasion.GetValue()`), либо добавить метод‑обёртку `GetEvasion()` в `Entity_Stats`.

**Исправление**
```csharp
// Assets/Scripts/Entity/Entity_Health.cs (фрагмент)
// 1) Вернуть алиас Unity Random
using Random = UnityEngine.Random;

// 2) Починить синтаксис и обращение к значению уклонения
private bool AttackEvaded()
{
    float roll = Random.Range(0f, 100f);                     // бросок 0..100
    float evasionPercent = stats.defense.evasion.GetValue(); // читаем шанс уклонения из Stat
    return roll < evasionPercent;
}
```

Альтернатива через метод в статистиках:
```csharp
// Assets/Scripts/Entity/Entity_Stats.cs
public float GetEvasion()
{
    return defense.evasion.GetValue();
}

// Assets/Scripts/Entity/Entity_Health.cs
private bool AttackEvaded() => UnityEngine.Random.Range(0f, 100f) < stats.GetEvasion();
```

Могу применить правки в `Entity_Health.cs` и при желании добавить `GetEvasion()` в `Entity_Stats.cs`. Подтвердите, пожалуйста.

---
## Как считается critPower на примере — 2025-10-27 15:54:16

**Текст заданного вопроса**
- Приведи на примере с числами как рассчитывается criPower Assets/Scripts/Entity/Entity_Stats.cs

**Доступное и подробное объяснение**
- В текущем проекте `critPower` нигде не рассчитывается внутри `Entity_Stats.cs`. В этом файле есть лишь ссылка на группу оффенсивных статов (`offense`), но формула крита отсутствует: см. `Assets/Scripts/Entity/Entity_Stats.cs:5`.
- Само поле `critPower` — это просто `Stat` с базовым значением, без модификаторов: см. `Assets/Scripts/StatSystem/Stat_OffenseGroup.cs:9` и реализацию `Stat.GetValue()` — возвращает `baseValue` как есть (`Assets/Scripts/StatSystem/Stat.cs:9`).
- Поэтому «как считается» сейчас зависит от того, КАК вы захотите применять `critPower` при расчёте урона. Ниже — типовые варианты с числами. Рекомендую Вариант A как самый понятный.

- Вариант A (рекомендованный): `critPower` — это итоговый множитель в процентах
  - Формула: `damage = baseDamage * (isCrit ? critPower/100f : 1f)`.
  - Пример: `baseDamage = 80`, `critChance = 25%`, `critPower = 150%`.
    - Крит сработал → урон = `80 * 1.5 = 120`.
    - Крит не сработал → урон = `80`.
  - Если поднять `critPower` до `180%` предметом: крит-урон = `80 * 1.8 = 144`.

- Вариант B: `critPower` — это «бонус к урону крита» в процентах
  - Формула: `damage = baseDamage * (isCrit ? (1f + critPower/100f) : 1f)`.
  - Пример: `baseDamage = 80`, `critPower = 50` (то есть +50% бонус) → при крите `80 * 1.5 = 120`.
  - Если `critPower = 80` → при крите `80 * 1.8 = 144`.

- Вариант C: `critPower` — это «плоский множитель X»
  - Формула: `damage = baseDamage * (isCrit ? critPower : 1f)`.
  - Пример: `critPower = 2.0` → при крите урон удваивается: `80 * 2.0 = 160`.

Подводные камни и советы
- Зафиксируйте единую трактовку в комментарии к полю `critPower` (проценты или множитель) — это убережёт от «1.5» vs «150».
- Клампите шанс крита: `0..100`, и защитите множитель минимумом `100%` (или `1.0x`) во избежание негативных/некорректных значений.
- Не используйте `System.Random` в геймплейной логике Unity — берите `UnityEngine.Random` или детерминированный источник (для тестов).

**Исправление**
```csharp
// Пример применения Варианта A без изменений файлов проекта (демонстрационный код)
using UnityEngine;

public static class CritCalculator
{
    // Возвращает урон с учётом шанса крита и мощности крита в процентах
    public static float ComputeDamageWithCrit(float baseDamage, Stat critChanceStat, Stat critPowerStat)
    {
        float chance = Mathf.Clamp(critChanceStat.GetValue(), 0f, 100f);   // 0..100 %
        float powerPct = Mathf.Max(critPowerStat.GetValue(), 100f);        // минимум 100%
        bool isCrit = UnityEngine.Random.Range(0f, 100f) < chance;         // бросок
        float critMul = powerPct / 100f;                                   // %, в множитель
        return baseDamage * (isCrit ? critMul : 1f);
    }

    // Детёрмин. пример «сколько будет при крите» (без случайности)
    public static float CritOnlyDamage(float baseDamage, float critPowerPercent)
    {
        float critMul = Mathf.Max(critPowerPercent, 100f) / 100f;
        return baseDamage * critMul;
    }
}

// Примеры чисел:
// baseDamage = 80, critChance = 25, critPower = 150
//  - При крите: CritOnlyDamage(80, 150) = 120
//  - В среднем по множеству ударов ожидаемый урон ≈ 80 * (0.75 * 1 + 0.25 * 1.5) = 90
```

**Улучшения**
- По запросу: добавлю метод `GetCritMultiplier()` в `Entity_Stats` и единые юнит‑тесты (`Assets/Tests/EditMode`) для формулы; вынесу RNG в интерфейс для тестируемости.
---
## Почему VFX позади Player? — 2025-10-29 14:53:37

**Текст заданного вопроса**
- «Почему префаб VFX_OnCritHit появляется на слое поверх Enemy_Skeleton (как надо), а при ударе по Player проигрывается позади Player?»

**Доступное и подробное объяснение**
- Коротко: визуальный эффект рендерится «сзади» Player, потому что его `Sorting Layer`/`Order in Layer` ниже, чем у спрайта Player. Для Enemy слой/порядок врага ниже, поэтому там VFX кажется «сверху». Иными словами, VFX сейчас попадает между слоями Enemy и Player.
- В 2D-рендере порядок рисования задаётся так: `Sorting Layer` (первично) → `Order in Layer` (внутри слоя) → иногда Z-позиция (если включён соответствующий режим Transparent Sort Mode). Для `ParticleSystemRenderer` действуют те же правила, что и для `SpriteRenderer`.
- Дополнительный частый фактор: `SortingGroup` на объекте Player. Если VFX становится ребёнком под `SortingGroup`, он не может «вырваться» выше других групп; его порядок ограничен группой и порядком внутри неё. Поэтому эффект может оказываться под спрайтом Player, если у эффекта меньший `Order in Layer` внутри этой группы.
- Почему «над Enemy», но «под Player»: вероятнее всего, слой VFX (или `Default`) выше слоя Enemy, но ниже слоя Player. Либо VFX заспаунен как ребёнок Player с меньшим порядком внутри `SortingGroup`.

Что проверить (в редакторе):
- Откройте префаб `VFX_OnCritHit` → у корневого объекта с `ParticleSystemRenderer` проверьте `Sorting Layer` и `Order in Layer`.
- Посмотрите на `SpriteRenderer`/`SortingGroup` у Player и Enemy: какие `Sorting Layer`/`Order in Layer` используются.
- Если в проекте URP 2D: в `Renderer Data` проверьте `Transparency Sort Mode` (обычно Orthographic) и, если используется Custom Axis/Z-слой, чтобы VFX не имел Z «за камерой».

Мини-вывод: сейчас VFX находится ниже рендера Player. Нужно поднять `Sorting Layer` (или внутри слоя поднять `Order in Layer`), либо при спавне подстраивать порядок под цель с небольшим оффсетом.

**Исправление**
```csharp
// Вариант A (быстрый, без кода):
// 1) Создайте Sorting Layer "VFX" и поместите его выше слоёв Player/Enemy.
// 2) В префабе VFX_OnCritHit на ParticleSystemRenderer:
//    - Sorting Layer = "VFX"
//    - Order in Layer = 100 (или другой гарантированно высокий)
// Это обеспечит, что эффект будет поверх и Player, и Enemy.
```

```csharp
// Вариант B (универсальный, с кодом): подстройка под цель на лету.
// Подходит, если вы хотите, чтобы VFX всегда был чуть выше любого объекта попадания.
// Один публичный класс на файл; PascalCase для типа/метода, camelCase для полей.

using UnityEngine;
using UnityEngine.Rendering;

public class MatchSortingToTarget : MonoBehaviour
{
    [SerializeField] private int orderOffset = 5; // На сколько быть выше цели
    [SerializeField] private Renderer targetRenderer; // Задаётся при спавне

    private void Awake()
    {
        if (targetRenderer == null)
        {
            // Попытка найти рендерер цели у родителя, если не передан извне
            targetRenderer = GetComponentInParent<Renderer>();
        }

        if (targetRenderer == null)
        {
            return; // Нечего выравнивать
        }

        // Читаем слой/порядок цели
        int layerId = targetRenderer.sortingLayerID;
        int order = targetRenderer.sortingOrder + orderOffset;

        // Применяем ко всем рендерам VFX (Sprite/Particle/Trail и т.д.)
        var renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingLayerID = layerId;
            renderers[i].sortingOrder = order;
        }
    }
}
```

```csharp
// Пример спавна (фрагмент): передаём Renderer цели, чтобы VFX оказался выше её слоя.
// Предпочтительно НЕ вкладывать VFX внутрь Player, если у Player есть SortingGroup,
// или поднимайте порядок внутри группы достаточно высоко.

public class VfxSpawner : MonoBehaviour
{
    [SerializeField] private GameObject vfxOnCritHitPrefab;

    public void SpawnCritVfx(Vector3 position, Renderer targetRenderer)
    {
        var vfx = Instantiate(vfxOnCritHitPrefab, position, Quaternion.identity);
        var matcher = vfx.GetComponent<MatchSortingToTarget>();
        if (matcher != null)
        {
            // Передаём рендер цели (SpriteRenderer, ParticleSystemRenderer тоже наследует Renderer)
            matcher.GetType().GetField("targetRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                   ?.SetValue(matcher, targetRenderer);
        }
    }
}
```

Подводные камни и почему фикс сработает:
- Если у Player есть `SortingGroup` и вы делаете VFX ребёнком Player, VFX не сможет «перепрыгнуть» в другой слой/группу. Тогда либо:
  - повышайте `sortingOrder` VFX внутри группы выше, чем у спрайта Player; либо
  - спавните VFX вне группы (на корне сцены) и задавайте слой вручную; либо
  - дайте самому VFX свой `SortingGroup` и выставьте его выше, чем у Player.
- Вариант A делает слой VFX глобально выше — самый простой и быстрый путь.
- Вариант B гарантирует, что эффект окажется поверх конкретной цели (даже если её порядок меняется на лету).

**Улучшения**
- По запросу: настроить единый слой `VFX_Overlay` и конвенции по `Order in Layer` (например, тела = 0, оружие = 5, VFX = 50),
  добавить тонкую логику: если удар «под ногами» — сортировать ниже, если «в воздухе» — выше, и т.д.
---
## Почему доступен elementalDamage в логе — 2025-10-31 16:54:15

**Текст заданного вопроса**
- Assets/Scripts/Entity/Entity_Health.cs Почему я имею доступ к `elementalDamage` в `Debug.Log($"Elemental damage: {elementalDamage}");`

**Доступное и подробное объяснение**
- Коротко: потому что `elementalDamage` — это параметр метода `TakeDamage` и он находится в области видимости тела этого метода. Любой код внутри `TakeDamage(...) { ... }` (включая `Debug.Log`) может обратиться к параметрам по имени.
- В проекте интерфейс `IDamageable` задаёт сигнатуру: `bool TakeDamage(float damage, float elementalDamage, Transform damageDealer);` Поэтому и базовый класс `Entity_Health`, и наследники (например, `Enemy_Health`) получают этот параметр и могут им пользоваться внутри переопределённого метода.
- Аналогия: метод — это «кухня», а параметры — «продукты», которые занесли в эту кухню. Пока вы не вышли из метода, они перед глазами.
- Подводные камни:
  - Параметр доступен только внутри метода. В другом методе класса его не видно — нужно либо передать его параметром, либо сохранить во внутреннем поле.
  - Если в классе есть поле с таким же именем, параметр «затенит» поле (shadowing). Тогда к полю обращаются как `this.elementalDamage`.
- Замечание по коду: в `Assets/Scripts/Entity/Entity_Combat.cs` есть вызов `damageable.TakeDamage(damage, elementalDamage, transform);`, но локальная переменная `elementalDamage` там не объявлена. Источником для неё, судя по коду, должен быть `stats.GetElementalDamage()`.

**Исправление**
```csharp
// Вставьте лог прямо внутри метода, где параметр виден
// Assets/Scripts/Entity/Entity_Health.cs
public virtual bool TakeDamage(float damage, float elementalDamage, Transform damageDealer)
{
    if (isDead)
        return false;

    Debug.Log($"Elemental damage: {elementalDamage}"); // параметр доступен внутри метода

    // ... остальная логика
    return true;
}
```

```csharp
// Минимальный рабочий фикс источника elementalDamage
// Assets/Scripts/Entity/Entity_Combat.cs
public void PerformAttack() 
{
    foreach (var target in GetDetectedColliders())
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
            continue;

        float damage = stats.GetPhysicalDamage(out bool isCrit);
        float elementalDamage = stats.GetElementalDamage(); // <-- объявляем и считаем

        bool targetGotHit = damageable.TakeDamage(damage, elementalDamage, transform);
        if (targetGotHit)
            vfx.CreateOnHitVFX(target.transform, isCrit);
    }
}
```

```csharp
// Если нужно использовать elementalDamage в других методах Entity_Health,
// сохраните его в поле при получении (вариант по необходимости)
// Assets/Scripts/Entity/Entity_Health.cs
public class Entity_Health : MonoBehaviour, IDamageable
{
    private float lastElementalDamage; // хранение последнего значения

    public virtual bool TakeDamage(float damage, float elementalDamage, Transform damageDealer)
    {
        lastElementalDamage = elementalDamage; // теперь доступно вне этого метода
        // ...
        return true;
    }

    private void SomeOtherMethod()
    {
        Debug.Log($"Last elemental damage: {lastElementalDamage}");
    }
}
```

Почему это сработает:
- Параметр по определению виден в теле метода — лог корректен. Добавление объявления `elementalDamage` в `Entity_Combat` убирает потенциальную ошибку «имя не существует в текущем контексте» и передаёт корректное значение от атакующего.

**Улучшения**
- По запросу: можем учесть резисты цели (например, через `Entity_Stats` защитной группы) и посчитать вклад `elementalDamage` в `finalDamage`. Также можно перейти на `Physics2D.OverlapCircleNonAlloc` для снижения аллокаций в бою.
---
## Равные elemental бонусы: учёт 0.5 у слабых — 2025-10-31 17:36:48

**Текст заданного вопроса**
- Assets/Scripts/Entity/Entity_Stats.cs необходимо добавить проверку, чтобы при одинаковых занчениях bonusFire, bonusIce and bonusLightning одно из них учитывалось как highestDamage, а два других как .5 в weakerElementsDamage. Сейчас если значения равны, то бонус не считается совсем

**Доступное и подробное объяснение**
- Сейчас `GetElementalDamage()` берёт строго наибольший элемент у трёх (`fire/ice/lightning`) и игнорирует остальные. При равенстве используется первый по порядку, а «бонус» от других стихий не добавляется.
- Что нужно: при равенстве максимумов выбрать одну стихию как `highestDamage` (детерминированно, например Fire > Ice > Lightning), а остальные равные максимумы учесть как «ослабленные» с коэффициентом `0.5` и сложить в `weakerElementsDamage`. Итог: `final = highest + weaker*0.5 + intelligence`.
- Нюанс с float: использовать `Mathf.Approximately` (или небольшой `epsilon`), чтобы корректно ловить «равенство» для вещественных чисел.

**Исправление**
```csharp
// Assets/Scripts/Entity/Entity_Stats.cs
public float GetElementalDamage()
{
    float fireDamage = offense.fireDamage.GetValue();
    float iceDamage = offense.iceDamage.GetValue();
    float lightningDamage = offense.lightningDamage.GetValue();
    float bonusElementalDamage = major.intelligence.GetValue();

    float maxDamage = Mathf.Max(fireDamage, Mathf.Max(iceDamage, lightningDamage));
    if (maxDamage <= 0)
        return 0;

    // Флаги равенства максимуму (с допуском для float)
    bool fireIsMax = Mathf.Approximately(fireDamage, maxDamage);
    bool iceIsMax = Mathf.Approximately(iceDamage, maxDamage);
    bool lightningIsMax = Mathf.Approximately(lightningDamage, maxDamage);

    // Выбираем детерминированно один «главный» максимум: Fire > Ice > Lightning
    float highestDamage;
    const float weakerFactor = 0.5f;
    float weakerElementsDamage = 0f;

    if (fireIsMax)
    {
        highestDamage = fireDamage;
        fireIsMax = false; // «потребили» его как главный
    }
    else if (iceIsMax)
    {
        highestDamage = iceDamage;
        iceIsMax = false;
    }
    else
    {
        highestDamage = lightningDamage;
        lightningIsMax = false;
    }

    // Все оставшиеся равные максимумы считаем как «слабые» с коэффициентом 0.5
    if (fireIsMax)      weakerElementsDamage += fireDamage * weakerFactor;
    if (iceIsMax)       weakerElementsDamage += iceDamage * weakerFactor;
    if (lightningIsMax) weakerElementsDamage += lightningDamage * weakerFactor;

    float finalDamage = highestDamage + weakerElementsDamage + bonusElementalDamage;
    return finalDamage;
}
```

Почему сработает
- При равенстве максимумов мы детерминированно выбираем один «главный» и корректно учитываем два других по `0.5`. При отсутствии равенства логика осталась прежней — берётся только один максимум.
- `Mathf.Approximately` защищает от проблем сравнения float из разных источников (баффы, расчёты, округления).

Нужно применить правку в файле? Подтвердите — и я пропатчу `Assets/Scripts/Entity/Entity_Stats.cs` автоматически.
---
## Минимальный фикс равных стихий (0.5) — 2025-10-31 17:43:29

**Текст заданного вопроса**
- Сейчас ты ввел свои переменные и переписал половину метода. Предложи простое и локаничное решение (можно без детерминации)

**Доступное и подробное объяснение**
- Сохраняем текущую идею: берём максимум из трёх стихий. Если есть ещё одна/две стихии, РОВНО равные максимуму, добавляем их по коэффициенту `0.5`. Не выбираем «главную» детерминированно — просто считаем количество равных максимумов.
- Это локально: 2–3 строки логики поверх текущего кода, без перестройки структуры и доп. коллекций.

**Исправление**
```csharp
// Assets/Scripts/Entity/Entity_Stats.cs
public float GetElementalDamage()
{
    float fireDamage = offense.fireDamage.GetValue();
    float iceDamage = offense.iceDamage.GetValue();
    float lightningDamage = offense.lightningDamage.GetValue();
    float bonusElementalDamage = major.intelligence.GetValue();

    float maxDamage = Mathf.Max(fireDamage, Mathf.Max(iceDamage, lightningDamage));
    if (maxDamage <= 0)
        return 0;

    // Подсчитываем, сколько стихий равны максимуму (с допуском для float)
    int equals = 0;
    if (Mathf.Approximately(fireDamage, maxDamage)) equals++;
    if (Mathf.Approximately(iceDamage, maxDamage)) equals++;
    if (Mathf.Approximately(lightningDamage, maxDamage)) equals++;

    // Если есть дополнительные равные максимумы, учитываем их как 0.5 каждый
    float weakerSum = (equals > 1) ? (equals - 1) * maxDamage * 0.5f : 0f;

    return maxDamage + weakerSum + bonusElementalDamage;
}
```

Почему это сработает
- При 1 максимуме `equals == 1` и формула даёт базовый максимум без изменений.
- При 2 или 3 равных максимумах «лишние» добавятся по `0.5` каждый, как вы и хотели.
- Минимум кода, без ветвлений по «главной» стихии.

Готов применить правку в файле `Assets/Scripts/Entity/Entity_Stats.cs`. Подтвердите — и я обновлю код.
---
## Как работает base.TakeDamage и wasHit — 2025-10-31 17:43:29

**Текст заданного вопроса**
- Assets/Scripts/Enemy/Enemy_Health.cs как работает  `bool wasHit = base.TakeDamage?`

**Доступное и подробное объяснение**
- Ключевое: `base.TakeDamage(...)` вызывает реализацию метода `TakeDamage` у базового класса (`Entity_Health`). Поскольку метод помечен `virtual` в базе и `override` в наследнике, вызов через `base` обеспечивает выполнение «общей» логики получения урона: проверка смерти/уклонения, расчёт брони, отбрасывание, VFX, уменьшение `currentHealth`.
- Возвращаемое значение `bool` говорит, был ли урон фактически применён:
  - `false` — цель уже мертва (`isDead == true`) или атака уклонена (`AttackEvaded()`), значит ничего дальше делать не нужно;
  - `true` — урон прошёл: здоровье уменьшено, воспроизведены эффекты/отбрасывание.
- В `Enemy_Health` это выглядит так:

```csharp
public override bool TakeDamage(float damage, float elementalDamage, Transform damageDealer)
{
    bool wasHit = base.TakeDamage(damage, elementalDamage, damageDealer);

    if (wasHit == false)
        return false; // база сказала: урон не применён → выходим

    // Дополнительная логика врага после успешного попадания
    if (damageDealer.GetComponent<Player>() != null)
        enemy.TryEnterBattleState(damageDealer);

    return true; // урон прошёл, расширенная логика выполнена
}
```

- Аналогия: `base.TakeDamage` — «базовая касса» урона (списывает HP, эффекты). Если кассир отказал (уклонение/мертв), касса не пробивает чек — вы выходите. Если чек пробит, наследник добавляет «послепродажные» шаги (перевод врага в бой).
- Подводные камни, на которые стоит обратить внимание (без правки кода сейчас):
  - В `Entity_Health.TakeDamage` параметр `elementalDamage` пока не используется — позже его можно учесть в формуле финального урона с резистами.
  - Внутри `Entity_Health.TakeDamage` берётся `attackerStats` через `GetComponent<Entity_Stats>()` у получателя урона. Логичнее получить статы атакующего от `damageDealer` (иначе редукция брони читается со стороны цели).

**Исправление**
- Вносить изменения не требуется: строка `bool wasHit = base.TakeDamage(...)` корректно получает результат из базового метода и управляет последующим потоком кода через ранний выход при `false`.
---
## Почему не срабатывает ApplyChilledEffect? — 2025-11-03 15:39:39

**Текст заданного вопроса**
- Почему не срабатывает ApplyChilledEffect? Assets/Scripts/Entity_StatusHandler.cs

**Доступное и подробное объяснение**
- Суть: вызов «ледяного» эффекта до цели либо не доходит, либо доходит, но визуально ничего не меняет. В проекте есть две основные причины:
  - Поиск компонента на коллайдере. В `Assets/Scripts/Entity/Entity_Combat.cs:54` используется `target.GetComponent<Entity_StatusHandler>()`. Если коллайдер цели находится на дочернем объекте, а сам `Entity_StatusHandler` висит на родителе, `GetComponent` вернёт `null` и код выйдет раньше, чем вызовет `ApplyChilledEffect`.
  - Пустая базовая реализация замедления. `Entity_StatusHandler.ApplyChilledEffect` вызывает `entity.SlowDownEntity(duration, slowMultiplier)`, но у базового `Entity` корутина `SlowDownEntityCo` пустая. То есть без переопределения в потомке замедление «ничего не делает». У `Enemy` есть override (замедляет `moveSpeed`, `battleMoveSpeed` и `anim.speed`), а у `Player` такого override нет — поэтому на игроке эффект «не срабатывает».
- Дополнительно: в `Assets/Scripts/Entity_StatusHandler.cs` поле `currentEffect` нигде не устанавливается и не сбрасывается. Это не мешает самому вызову, но не даёт контролировать наложение/снятие статуса и потенциально вводит в заблуждение.
- Как быстро проверить:
  - Временно залогировать вызов: добавить `Debug.Log("Chill applied")` в `ApplyChilledEffect` и ударить цель — если лог не появляется, значит не нашли `StatusHandler` на цели.
  - Проверить иерархию объекта: где `Collider2D` и где `Entity_StatusHandler`. Если хэндлер на родителе, нужен `GetComponentInParent`.
  - Сравнить поведение на врагах и на игроке: враг замедляется (есть override), игрок — нет (override отсутствует).

**Исправление**
```csharp
// 1) Гарантировать, что найдём StatusHandler на цели
// Assets/Scripts/Entity/Entity_Combat.cs
public void ApplyStatusEffect(Transform target, ElementType element)
{
    // Было:
    // Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();

    // Стало: ищем на родителе (и себе), т.к. коллайдер часто на дочернем объекте
    Entity_StatusHandler statusHandler = target.GetComponentInParent<Entity_StatusHandler>();

    if (statusHandler == null)
        return;

    if (element == ElementType.Ice && statusHandler.CanBeApplied(ElementType.Ice))
    {
        statusHandler.ApplyChilledEffect(defaultDuration, chillSlowMultiplier);
    }
}
```

```csharp
// 2) Реализовать фактическое замедление для Player
// Assets/Scripts/Player/Player.cs
using System.Collections;

protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
{
    float originalMoveSpeed = moveSpeed;
    float originalAnimSpeed = anim.speed;

    float speedMultiplier = 1f - slowMultiplier; // при 0.2f получаем 80% скорости

    moveSpeed *= speedMultiplier;
    anim.speed *= speedMultiplier;

    yield return new WaitForSeconds(duration);

    moveSpeed = originalMoveSpeed;
    anim.speed = originalAnimSpeed;
}
```

```csharp
// 3) (Опционально) Отметить и снимать статус внутри StatusHandler
// Assets/Scripts/Entity_StatusHandler.cs
using System.Collections;

public void ApplyChilledEffect(float duration, float slowMultiplier)
{
    if (currentEffect != ElementType.None)
        return; // уже под эффектом — не стакаем

    currentEffect = ElementType.Ice;
    entity.SlowDownEntity(duration, slowMultiplier);
    StartCoroutine(ClearEffectAfter(duration));
}

private IEnumerator ClearEffectAfter(float duration)
{
    yield return new WaitForSeconds(duration);
    currentEffect = ElementType.None;
}
```

- Почему это сработает:
  - `GetComponentInParent` устраняет проблему поиска хэндлера на родителе, когда удар приходится по дочернему коллайдеру.
  - Переопределение `SlowDownEntityCo` в `Player` делает замедление реальным, как уже реализовано в `Enemy`.
  - Управление `currentEffect` даёт предсказуемость: не стакаем повторно и корректно сбрасываем эффект по таймеру.

Запрос на подтверждение
- Следуя guardrails, править файлы в `Assets/` без согласия не буду. Подтвердите, пожалуйста, применить правки к трём местам выше? Могу внести точечные патчи и прогнать сцену.
---
## Как работает PlayStatusVfxCo пошагово — 2025-11-04 16:00:12

**Текст заданного вопроса**
- Assets/Scripts/Entity/Entity_VFX.cs как работает пошагово PlayStatusVfxCo?

**Доступное и подробное объяснение**
- Где вызывается: публичный метод `PlayOnStatusVFX(float duration, ElementType element)` решает, какой цвет эффекта применить, и запускает корутину `PlayStatusVfxCo(duration, chillVfx)` для `ElementType.Ice`.
- Что делает корутина `PlayStatusVfxCo` пошагово:
  1) Задаёт локальные параметры:
     - `tickInterval = 0.25f` — период мигания.
     - `timeHasPassed = 0` — накопленное время.
  2) Готовит два оттенка выбранного цвета:
     - `lightColor = effectColor * 1.2f` — чуть ярче.
     - `darkColor = effectColor * 0.8f` — чуть темнее.
  3) Вводит флаг `toggle = false` для переключения между светлым/тёмным.
  4) Цикл пока `timeHasPassed < duration`:
     - Устанавливает `sr.color = toggle ? lightColor : darkColor` — применяет один из оттенков к `SpriteRenderer`.
     - Инвертирует `toggle = !toggle` — на следующем шаге будет другой оттенок.
     - `yield return new WaitForSeconds(tickInterval)` — «усыпляет» корутину на четверть секунды (игровое время, зависит от `Time.timeScale`).
     - Увеличивает накопленное время: `timeHasPassed += tickInterval`.
  5) По завершении длительности сбрасывает цвет спрайта: `sr.color = Color.white`.

- Аналогия: «мигалка». Каждые 0.25 сек спрайт перекрашивается попеременно в светлый/тёмный оттенок «льда», пока не истечёт таймер.

- Важные детали и подводные камни:
  - Сброс цвета жёстко в `Color.white`. Если исходный `SpriteRenderer.color` не белый (например, был тонирован), вы потеряете исходный оттенок. Надёжнее сохранить оригинальный цвет в `Awake` и восстанавливать его.
  - Аллокации: `new WaitForSeconds(tickInterval)` создаётся в каждом тике — это лишние выделения памяти. Лучше кэшировать `WaitForSeconds` внутри корутины один раз на интервал.
  - Параллельные вызовы: если `PlayOnStatusVFX` вызвать повторно до завершения прошлого эффекта, запустится вторая корутина и они начнут конфликтовать (цвет будет «перетягиваться»). Стоит хранить ссылку на текущую корутину и останавливать её перед запуском новой.
  - Зависимость от TimeScale: `WaitForSeconds` учитывает `Time.timeScale`. Если игра на паузе (`timeScale = 0`), мигание остановится. Если нужно мигание «поверх паузы», используйте `WaitForSecondsRealtime`.

**Исправление**
```csharp
// Без изменений проекта: показать безопасные улучшения логики
// Assets/Scripts/Entity/Entity_VFX.cs (концепт)
using System.Collections;
using UnityEngine;

public class Entity_VFX : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalSpriteColor;
    private Coroutine statusVfxCo;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalSpriteColor = sr != null ? sr.color : Color.white;
    }

    public void PlayOnStatusVFX(float duration, ElementType element)
    {
        if (statusVfxCo != null)
            StopCoroutine(statusVfxCo); // не допускаем конкурирующих корутин

        if (element == ElementType.Ice)
            statusVfxCo = StartCoroutine(PlayStatusVfxCo(duration, Color.cyan));
    }

    private IEnumerator PlayStatusVfxCo(float duration, Color effectColor)
    {
        const float tickInterval = 0.25f;
        float timeHasPassed = 0f;

        // Кэшируем инструкцию ожидания, чтобы не аллоцировать каждый тик
        WaitForSeconds waitTick = new WaitForSeconds(tickInterval);

        Color lightColor = effectColor * 1.2f;
        Color darkColor = effectColor * 0.8f;
        bool toggle = false;

        while (timeHasPassed < duration)
        {
            if (sr != null)
                sr.color = toggle ? lightColor : darkColor;
            toggle = !toggle;

            yield return waitTick;
            timeHasPassed += tickInterval;
        }

        if (sr != null)
            sr.color = originalSpriteColor; // корректный возврат к исходному цвету

        statusVfxCo = null;
    }
}
```

- Почему это надёжнее:
  - Восстанавливаем исходный цвет спрайта вместо принудительного белого.
  - Кэшируем `WaitForSeconds` (меньше аллокаций каждой четверти секунды).
  - Исключаем одновременные корутины одного эффекта (нет «борьбы за цвет»).
  - По желанию можно заменить `WaitForSeconds` на `WaitForSecondsRealtime`, если эффект должен идти при паузе.
---
## Приветствие и формат работы UnityMentor 2D — 2025-11-06 09:29:10

**Текст заданного вопроса**
- «привет»

**Доступное и подробное объяснение**
- Привет! Я UnityMentor: помогу с Unity 6.1 (6000.1.14f1) по 2D-проекту. Расскажите, с чем помочь: настройка сцены, физика, анимации, скрипты, баг-репроды, оптимизация, архитектура состояний и т.д.
- Можем начать с описания проблемы или показать, как запустить/проверить нужную сцену в `Assets/Scenes/`.

**Исправление**
```csharp
// Сейчас исправления не требуются — это приветствие.
```

**Улучшения**
- По запросу: предложу архитектуру/рефакторинг/тесты.
---
## Описать классы и методы в scripts — 2025-11-06 11:17:55

**Текст заданного вопроса**
- «Задай описание всем классам и методам (не в коде) в папке scripts (кроме базовых методов языка C# и Unity). Выведи в формате "Название класса \ метода - Описание"»

**Доступное и подробное объяснение**
- В корне проекта папка `scripts` не содержит C#-файлов; в Unity-проекте игровые скрипты лежат в `Assets/Scripts/`. Я прошёлся по `Assets/Scripts/` и описал все классы и их пользовательские методы.
- Исключены базовые методы C#/Unity: `Awake`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnEnable`, `OnDisable`, `OnDestroy`, `OnDrawGizmos` и т.п. (включая их переопределения). Остальные методы, относящиеся к игровой логике/состояниям/боёвке — включены.
- Формат: «Имя класса или метода — краткое назначение». Для методов указан владелец: `Класс.Метод`.

**Исправление**
```csharp
// Это документация/описания — изменений в код не вносилось.
```

**Описание (классы и методы)**

ElementType — Перечисление элементов урона/эффектов (None/Fire/Ice/Lightning).

Stat — Обёртка над числовым значением стата.
Stat.GetValue — Возвращает текущее базовое значение стата.

Stat_MajorGroup — Группа «основных» статов: сила/ловкость/интеллект/жизнь.

Stat_OffenseGroup — Группа атакующих статов: физ.урон, крит.шанс/сила, редукция брони, элем.уроны.

Stat_DefenseGroup — Группа защитных статов: уклонение, броня, элем.сопротивления.

Entity — Базовый класс сущности: аниматор, Rigidbody2D, FSM, детект окружения, разворот.
Entity.CallAnimationTrigger — Пробрасывает триггер из анимации в текущее состояние FSM.
Entity.EntityDeath — Виртуальный хук реакции на смерть (переопределяется наследниками).
Entity.SlowDownEntity — Запускает корутину временного замедления сущности.
Entity.SlowDownEntityCo — Короутина: применяет и снимает замедление параметров.
Entity.ReceiveKnockback — Применяет нокбэк на время (через корутину).
Entity.SetVelocity — Устанавливает скорость и авто‑флип по знаку X.
Entity.HandleFlip — Проверяет необходимость флипа по заданной X‑скорости.
Entity.Flip — Поворачивает трансформ, меняет `facingDir`, вызывает `OnFlipped`.
Entity.HandleCollisionDetection — Обновляет `groundDetected`/`wallDetected` рейкастами.

Entity_Health — Управление здоровьем: приём урона, уклонение, нокбэк, смерть, UI‑полоса HP.
Entity_Health.TakeDamage — Считает физ./элем.урон с бронёй/резистами, применяет нокбэк/VFX.
Entity_Health.ReduceHealth — Уменьшает HP, обновляет UI, запускает VFX и проверяет смерть.
Entity_Health.Die — Отмечает смерть и вызывает `Entity.EntityDeath`.
Entity_Health.AttackEvaded — Возвращает, сработало ли уклонение по шансам.
Entity_Health.TakeKnockback — Вычисляет и применяет нокбэк через Entity.
Entity_Health.CalculateKnockback — Определяет силу/направление нокбэка по «тяжести» удара.
Entity_Health.IsHeavyDamage — Проверяет, превышает ли урон порог «тяжёлого».
Entity_Health.CalculateKnockbackDuration — Длительность нокбэка в зависимости от тяжести.
Entity_Health.UpdateHealthBar — Обновляет значение слайдера HP.

Entity_VFX — Визуальные эффекты сущности: «хит»-эффект, цвет по элементу, статус‑мигание.
Entity_VFX.PlayOnStatusVFX — Запускает мигание спрайта цветом статуса на длительность.
Entity_VFX.PlayStatusVfxCo — Короутина периодического изменения цвета спрайта.
Entity_VFX.UpdateOnHitColor — Меняет цвет VFX попадания согласно элементу/сброс по None.
Entity_VFX.CreateOnHitVFX — Спавнит VFX попадания/крита, задаёт цвет/флип по направлению.
Entity_VFX.PlayOnDamageVFX — Включает «материал урона» на короткое время.
Entity_VFX.OnDamageVFXCoroutine — Короутина временной замены материала.

Entity_AnimationTriggers — Связка событий анимации с логикой Entity/Combat.
Entity_AnimationTriggers.CurrentStateTrigger — Сообщает активному состоянию о триггере анимации.
Entity_AnimationTriggers.AttackTrigger — Вызывает `Entity_Combat.PerformAttack`.

Enemy_AnimationTriggers — Спецтриггеры врага: окно контратаки + визуальное предупреждение.
Enemy_AnimationTriggers.EnableCounterWindow — Включает окно парирования и индикатор атаки.
Enemy_AnimationTriggers.DisableCounterWindow — Выключает окно парирования и индикатор.

Entity_Combat — Ближний бой: поиск целей, нанесение урона, статусы, VFX попадания.
Entity_Combat.PerformAttack — Наносит урон всем целям в зоне; применяет статусы/VFX.
Entity_Combat.ApplyStatusEffect — Применяет статус цели по элементу (лед — замедление).
Entity_Combat.GetDetectedColliders — Возвращает цели в радиусе поражения.

Entity_Stats — Хранит статы и вычисляет производные (урон/резисты/крит/уклонение/броня).
Entity_Stats.GetElementalDamage — Выбирает доминирующий элемент и суммарный элем.урон.
Entity_Stats.GetElementalResistance — Считает сопротивление к элементу с учётом капа.
Entity_Stats.GetMaxHealth — Возвращает максимум HP с бонусами от `vitality`.
Entity_Stats.GetArmorReduction — Возвращает множитель пробития брони (0..1).
Entity_Stats.GetArmorMitigation — Долю поглощения урона (эффективная броня с капом).
Entity_Stats.GetPhysicalDamage — Физ.урон с шансом/силой крита и бонусами от статов.
Entity_Stats.GetEvasion — Шанс уклонения с капом.

StateMachine — Менеджер конечного автомата: хранит/меняет текущее состояние.
StateMachine.Initialize — Задаёт стартовое состояние и вызывает `Enter`.
StateMachine.ChangeState — Делает `Exit` старого и `Enter` нового состояния.
StateMachine.UpdateActiveState — Делегирует `Update` активному состоянию.
StateMachine.SwitchOffStateMachine — Запрещает дальнейшие смены состояний.

EntityState — Абстрактная база состояния сущности.
EntityState.Enter — Включает аниматорный bool состояния, сбрасывает триггер.
EntityState.Update — Тикает таймер состояния и обновляет аним‑параметры.
EntityState.Exit — Выключает аниматорный bool состояния.
EntityState.AnimationTrigger — Помечает, что пришёл триггер из анимации.
EntityState.UpdateAnimationParams — Хук для установки параметров Animator.

EnemyState — База состояний врага; связывает `rb/anim`, настраивает аним‑параметры.
EnemyState.UpdateAnimationParams — Ставит множители скорости анимации/движения.

Enemy — Класс врага: параметры ИИ/боя, набор состояний и взаимодействие с игроком.
Enemy.SlowDownEntityCo — Короутина замедления (скорость бега/боя и скорость анимации).
Enemy.EnableCounterWindow — Разрешает/запрещает «стан» от контрудара.
Enemy.EntityDeath — Переводит врага в состояние смерти.
Enemy.HandlePlayerDeath — Возвращает врага в idle при смерти игрока.
Enemy.TryEnterBattleState — Переходит в бой, если не в бою/атаке; сохраняет ссылку на игрока.
Enemy.GetPlayerReference — Лениво получает/кеширует `Transform` игрока.
Enemy.PlayerDetected — Проверяет игрока лучом по направлению взгляда.

Enemy_Skeleton — Конкретный тип врага; заполняет состояния; поддерживает контрудар.
Enemy_Skeleton.HandleCounter — Переключает в оглушение, если окно контратаки активно.

Enemy_VFX — Доп. VFX врага.
Enemy_VFX.EnableAttackAlert — Вкл/выкл визуальный индикатор окна атаки.

Enemy_Health — Наследник `Entity_Health` с реакцией на удар от игрока.
Enemy_Health.TakeDamage — После удара игроком переводит врага в состояние боя.

Player — Игрок: ввод, параметры движения/атаки, набор состояний.
Player.SlowDownEntityCo — Короутина замедления: снижает move/jump/анимацию/импульсы атак.
Player.EntityDeath — Посылает событие смерти и переводит в `DeadState`.
Player.EnterAttackStateWithDelay — Ставит переход в базовую атаку на конец кадра.
Player.EnterAttackStateWIthDelayCo — Короутина задержки входа в атаку на 1 кадр.

Player_Combat — Боевая логика игрока для контрудара.
Player_Combat.CounterAttackPerformed — Проверяет цель в зоне и вызывает `HandleCounter`.
Player_Combat.GetCounterRecoveryDuration — Длительность «восстановления» после контрудара.

Player_GroundedState — База наземных состояний игрока.
Player_GroundedState.Update — Обрабатывает прыжок/атаку/контрудар и переход в падение.

Player_IdleState — Ожидание на земле.
Player_IdleState.Enter — Обнуляет скорость по входу в состояние.
Player_IdleState.Update — Переходит в ходьбу при вводе, игнорирует, если упирается в стену.

Player_MoveState — Движение по земле.
Player_MoveState.Update — Двигает по X, переходит в idle при отпускании/касании стены.

Player_JumpState — Фаза начала прыжка.
Player_JumpState.Enter — Задаёт вертикальную скорость прыжка.
Player_JumpState.Update — На спаде Y переходит в падение (если не jump‑attack).

Player_AiredState — Общая логика «в воздухе».
Player_AiredState.Update — Воздушный контроль по X; переход в jump‑attack по нажатию атаки.

Player_FallState — Падение.
Player_FallState.Update — Переходы: на землю (idle) или на стену (wall‑slide).

Player_WallSlideState — Скольжение по стене.
Player_WallSlideState.Update — Замедляет падение, переходы в прыжок от стены/падение/idle.
Player_WallSlideState.HandleWallSlide — Управляет вертикальной скоростью с учётом ввода.

Player_WallJumpState — Прыжок от стены.
Player_WallJumpState.Enter — Придаёт импульс от стены и вверх.
Player_WallJumpState.Update — Переходы в jump‑attack/падение/обратно на скольжение.

Player_DashState — Рывок.
Player_DashState.Enter — Настраивает длительность/направление, отключает гравитацию.
Player_DashState.Update — Движение рывком; выход в idle/fall по таймеру; отмена по стене.
Player_DashState.Exit — Восстанавливает гравитацию и скорость.
Player_DashState.CancelDashIfNeeded — Отменяет рывок при касании стены.

Player_BasicAttackState — Базовая серия ударов (комбо).
Player_BasicAttackState.Enter — Ставит индекс комбо, применяет импульс, настраивает анимацию.
Player_BasicAttackState.Update — Ведёт таймер импульса; ставит очередь следующего удара; логика выхода.
Player_BasicAttackState.Exit — Обновляет индекс комбо и время последнего удара.
Player_BasicAttackState.HandleStateExit — Переход к следующему удару или в idle.
Player_BasicAttackState.QueueNextAttack — Ставит флажок продолжения комбо.
Player_BasicAttackState.HandleAttackVelocity — Ведёт таймер импульса и сбрасывает скорость по окончании.
Player_BasicAttackState.ApplyAttackVelocity — Применяет импульс текущего удара по направлению.
Player_BasicAttackState.ResetComboIndexIfNeeded — Сбрасывает индекс комбо при пропуске окна.

Player_JumpAttack_State — Атака в прыжке.
Player_JumpAttack_State.Enter — Придаёт импульс и триггерит фазу удара.
Player_JumpAttack_State.Update — Логика приземления и выход в idle по триггеру.

Player_DeadState — Состояние смерти игрока.
Player_DeadState.Enter — Отключает ввод и симуляцию физики.

Player_CounterAttackState — Состояние контратаки.
Player_CounterAttackState.Enter — Старт окна контрудара; фиксирует успех; настраивает анимацию.
Player_CounterAttackState.Update — Держит игрока; выход по триггеру или по таймеру без успеха.

ParallaxBackground — Контроллер параллакса: двигает и петлит слои по движению камеры.
ParallaxBackground.CalculateImageLength — Кеширует ширину каждого слоя.

ParallaxLayer — Описание слоя фона и логика движения/петли.
ParallaxLayer.CalculateImageWidth — Вычисляет ширину спрайта для петли.
ParallaxLayer.Move — Смещает слой по X с множителем.
ParallaxLayer.LoopBackground — Перекидывает слой, если слой вышел за край камеры.

UI_MiniHealthBar — Мини‑полоска HP над объектом, синхронизирована с разворотом.
UI_MiniHealthBar.HandleFlip — Сбрасывает поворот, чтобы UI не зеркалился при флипе спрайта.

Chest — Разрушаемый сундук: открывается и отлетает при ударе.
Chest.TakeDamage — Запускает анимацию открытия, задаёт нокбэк/вращение и VFX.

IDamageable — Интерфейс «получающего урон».
IDamageable.TakeDamage — Сигнатура нанесения урона: физ/элем, тип, источник.

ICounterable — Интерфейс «контрпринимаемого».
ICounterable.CanBeCountered — Флаг окна уязвимости к контрудару.
ICounterable.HandleCounter — Реакция на успешный контрудар.
