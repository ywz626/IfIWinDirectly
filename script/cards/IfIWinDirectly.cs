using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Win.script.powers;

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

    public override string PortraitPath => $"res://Win/images/cards/win.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3),
        new EnergyVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    // BaseLib 会自动在 key 前拼接模型的 Entry ID，所以这里只需要提供 "title" 和 "description"
    public override List<(string, string)>? Localization =>
    [
        ("title", "假如我直接赢"),
        ("description", "每回合开始时，获得{Energy}点能量，抽{Cards}张牌。")
    ];

    public IfIWinDirectly() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<IfIWinDirectlyPower>(
            Owner.Creature,
            DynamicVars.Cards.BaseValue,
            Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2);
        DynamicVars.Energy.UpgradeValueBy(2);
    }
}
