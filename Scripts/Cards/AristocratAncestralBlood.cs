using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Cards;

/// <summary>
/// 先祖之血（0 费，技能，罕见）：消耗手中 1 张打击和 1 张防御，每消耗 1 张牌获得 1 点能量。
/// 升级后不再消耗（对应塔1 原版的 upp()：this.exhaust = false）。
/// </summary>
public sealed class AristocratAncestralBlood : AristocratCard
{
    public AristocratAncestralBlood()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        List<CardModel> hand = PileType.Hand.GetPile(owner).Cards.ToList();

        CardModel? strike = hand.FirstOrDefault(IsStrike);
        CardModel? defend = hand.FirstOrDefault(IsDefend);

        if (strike != null)
        {
            await CardCmd.Exhaust(choiceContext, strike);
            await PlayerCmd.GainEnergy(1, owner);
        }

        if (defend != null)
        {
            await CardCmd.Exhaust(choiceContext, defend);
            await PlayerCmd.GainEnergy(1, owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后这张牌本身不再消耗
        RemoveKeyword(CardKeyword.Exhaust);
    }
}