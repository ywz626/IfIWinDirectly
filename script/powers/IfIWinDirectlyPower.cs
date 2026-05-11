using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace Win.script.powers;

public class IfIWinDirectlyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // BaseLib 会自动在 key 前拼接模型的 Entry ID，所以这里只需要提供 "title" 和 "description"
    public override List<(string, string)>? Localization =>
    [
        ("title", "直接赢"),
        ("description", "每回合开始时，获得{Amount}点能量，抽{Amount}张牌。")
    ];

    // 为 description 添加 Amount 变量，使 {Amount} 能被正确替换为实际数值
    public override LocString Description
    {
        get
        {
            var desc = base.Description;
            desc.Add("Amount", Amount);
            return desc;
        }
    }

    // 回合开始时触发
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        // 只在自己的回合触发
        if (side != Owner.Side)
            return;

        // 获得能量
        await PlayerCmd.GainEnergy(Amount, Owner.Player!);

        // 抽牌 - 必须使用有效的 PlayerChoiceContext，null 会导致游戏卡住
        var context = new HookPlayerChoiceContext(Owner.Player!, LocalContext.NetId.Value, GameActionType.Combat);
        await CardPileCmd.Draw(context, Amount, Owner.Player!);
    }
}