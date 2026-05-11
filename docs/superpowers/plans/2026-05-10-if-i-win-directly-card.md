# 假如我直接赢 — 猎人能力牌实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现一张0费稀有猎人能力牌"假如我直接赢"，固有，替换猎人初始卡组中的Strike，每回合开始时抽3/5张牌并获得3/5点能量。

**Architecture:** 采用标准STS2能力牌模式：卡牌`OnPlay`时通过`PowerCmd.Apply`给玩家附加自定义Power；Power重写`AfterSideTurnStart`在每回合开始时执行抽牌和获得能量。通过`PlaceholderCharacterModel`覆盖猎人初始卡组实现Strike替换。

**Tech Stack:** C#, BaseLib (Alchyr.Sts2.BaseLib 3.1.2), STS2 (MegaCrit.Sts2)

---

### Task 1: 创建自定义 Power

**Files:**
- Create: `script/powers/IfIWinDirectlyPower.cs`

- [ ] **Step 1: 编写 Power 代码**

```csharp
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Win.script.powers;

public class IfIWinDirectlyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 回合开始时触发
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        // 只在自己的回合触发
        if (side != Owner.Side)
            return;

        // 获得能量
        await PlayerCmd.GainEnergy(Amount, Owner.Player!);
        // 抽牌
        await CardPileCmd.Draw(combatState.PlayerChoiceContext, (int)Amount, Owner.Player!);
    }
}
```

- [ ] **Step 2: 编译验证**

Run: `dotnet build`
Expected: Build succeeds or shows expected errors (other files not yet created)

---

### Task 2: 创建能力牌卡牌

**Files:**
- Create: `script/cards/IfIWinDirectly.cs`

- [ ] **Step 1: 编写卡牌代码**

```csharp
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Win.script.cards;

// 注册到猎人卡池
[Pool(typeof(SilentCardPool))]
public class IfIWinDirectly : CustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    // 基础数值：抽3张，获得3能量
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move)];

    public IfIWinDirectly() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        AddKeyword(CardKeyword.Innate);
    }

    // 打出时附加 Power
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<IfIWinDirectlyPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Damage.BaseValue,
            Owner.Creature,
            this
        );
    }

    // 升级后数值从3提升到5
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
```

- [ ] **Step 2: 编译验证**

Run: `dotnet build`
Expected: Build succeeds (IfIWinDirectlyPower.cs 已存在)

---

### Task 3: 创建自定义角色以替换初始卡组

**Files:**
- Create: `script/characters/SilentStarterReplacer.cs`

- [ ] **Step 1: 编写角色覆盖代码**

```csharp
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Win.script.characters;

public class SilentStarterReplacer : PlaceholderCharacterModel
{
    public override string PlaceholderID => "silent";

    // 覆盖初始卡组，将其中一个Strike替换为"假如我直接赢"
    public override IEnumerable<CardModel> StartingDeck => new List<CardModel>
    {
        (CardModel)(object)ModelDb.Card<IfIWinDirectly>(),
        (CardModel)(object)ModelDb.Card<StrikeSilent>(),
        (CardModel)(object)ModelDb.Card<StrikeSilent>(),
        (CardModel)(object)ModelDb.Card<StrikeSilent>(),
        (CardModel)(object)ModelDb.Card<StrikeSilent>(),
        (CardModel)(object)ModelDb.Card<DefendSilent>(),
        (CardModel)(object)ModelDb.Card<DefendSilent>(),
        (CardModel)(object)ModelDb.Card<DefendSilent>(),
        (CardModel)(object)ModelDb.Card<DefendSilent>(),
        (CardModel)(object)ModelDb.Card<DefendSilent>(),
        (CardModel)(object)ModelDb.Card<Survivor>(),
        (CardModel)(object)ModelDb.Card<Neutralize>()
    };
}
```

> **注意**：`StrikeSilent`、`DefendSilent`、`Survivor`、`Neutralize` 是STS2猎人原有的卡牌类型。如果这些类型名不正确，需要根据实际游戏API调整。

- [ ] **Step 2: 编译验证**

Run: `dotnet build`
Expected: Build succeeds or shows errors about missing vanilla card type names

---

### Task 4: 处理可能的编译问题

**Files:**
- Modify: `script/characters/SilentStarterReplacer.cs`

- [ ] **Step 1: 如果原生卡牌类型名不正确，查找正确名称**

检查 `sts2.dll` 中猎人初始卡牌的准确类名，常见可能：
- `Strike` / `SilentStrike` / `Strike_Silent`
- `Defend` / `SilentDefend` / `Defend_Silent`

- [ ] **Step 2: 修正并重新编译**

Run: `dotnet build`
Expected: Build succeeds

---

### Task 5: 最终测试验证

- [ ] **Step 1: 完整编译**

Run: `dotnet build`
Expected: Build succeeds with no errors

- [ ] **Step 2: 运行游戏测试**

1. 启动 Slay the Spire 2
2. 选择猎人角色
3. 验证初始卡组中有一张"假如我直接赢"（替代了一张Strike）
4. 打出卡牌，验证Power被正确附加
5. 进入下一回合，验证抽3张牌并获得3能量
6. 升级卡牌，验证下回合抽5张牌并获得5能量

---

## Self-Review

**Spec coverage check:**
- [x] 0费能力牌 — Task 2 中 `energyCost = 0`, `CardType.Power`
- [x] 稀有品质 — Task 2 中 `CardRarity.Rare`
- [x] 每回合开始抽牌+能量 — Task 1 中 `AfterSideTurnStart` 触发
- [x] 固有 — Task 2 中 `AddKeyword(CardKeyword.Innate)`
- [x] 替换初始Strike — Task 3 中 `PlaceholderCharacterModel.StartingDeck`
- [x] 升级后3→5 — Task 2 中 `OnUpgrade` 增加2

**Placeholder scan:**
- [x] 无 TBD/TODO
- [x] 所有代码完整

**Type consistency:**
- [x] `IfIWinDirectlyPower` 在 Task 1 创建，Task 2 中以泛型参数引用 — 名称一致
- [x] `Amount` 在 Power 中用于存储数值（抽牌数和能量数共享同一数值）
