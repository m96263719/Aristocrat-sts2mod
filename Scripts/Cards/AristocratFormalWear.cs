using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Aristocrat.Cards;

/// <summary>
/// 礼服（1 费，技能，罕见，固有 + 保留 + 消耗）：抽 3 张牌。升级 +1。
/// 对应塔1 的 aristocrat:FormalWear。
/// </summary>
public sealed class AristocratFormalWear : AristocratCard
{
    public AristocratFormalWear()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Innate, CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 3m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await CardPileCmd.Draw(choiceContext, DynamicVars["MagicNumber"].BaseValue, owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}
