# 假如我直接赢 — 猎人能力牌设计

## 概述
为杀戮尖塔2（STS2）mod 新增一张猎人职业能力牌，替换猎人初始卡组中的 Strike。

## 卡牌属性

| 属性 | 基础值 | 升级后 |
|------|--------|--------|
| 名称 | 假如我直接赢 | 假如我直接赢+ |
| 费用 | 0 | 0 |
| 类型 | Power（能力牌） | Power |
| 稀有度 | Rare | Rare |
| 目标 | Self | Self |
| 固有 | 是 | 是 |
| 效果 | 每回合开始时：抽3张牌，获得3点能量 | 每回合开始时：抽5张牌，获得5点能量 |

## 替换规则
- 替换猎人初始卡组中的 **Strike**（打击）
- 作为猎人职业卡注册到对应卡池

## 架构

### 文件结构
```
script/
  cards/
    IfIWinDirectly.cs      # 卡牌本体
  powers/
    IfIWinDirectlyPower.cs # 每回合触发效果的 Power
```

### 职责划分
- **IfIWinDirectly.cs**：继承 `CustomCardModel`，定义卡牌元数据。`OnPlay` 中给玩家附加 `IfIWinDirectlyPower`。
- **IfIWinDirectlyPower.cs**：继承 BaseLib 的 Power 基类，重写回合开始钩子，执行抽牌和获得能量。

### 升级机制
卡牌升级时，销毁旧 Power，附加新数值的 Power；或通过 Power 内部更新数值。优先采用 Power 内部更新方式以减少副作用。

## 边界情况
- 如果玩家通过其他手段（如瓶装）获得多张此卡，打出多张时 Power 应该叠加还是刷新？**采用刷新机制**（后打出的覆盖之前的数值），避免指数级收益。
- 第一回合打出时，当前回合是否立即触发？**不立即触发**，Power 效果仅在"每回合开始时"生效，符合 STS2 能力牌惯例。

## 依赖
- BaseLib `CustomCardModel`
- BaseLib Power 基类及回合开始钩子
- `PlayerCmd.GainEnergy`
- `CardPileCmd.Draw`
