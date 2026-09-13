using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Aristocrat.Cards;

/// <summary>古典（1 费，技能，普通）：从抽牌堆各抽 M 张打击与防御。升级 M+1。对应塔1 的 aristocrat:Classic。</summary>
public sealed class AristocratClassic : AristocratCard
{
    public AristocratClassic()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        int count = DynamicVars["MagicNumber"].IntValue;

        await DrawMatchingFromDrawPile(choiceContext, owner, count, IsStrike);
        await DrawMatchingFromDrawPile(choiceContext, owner, count, IsDefend);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}