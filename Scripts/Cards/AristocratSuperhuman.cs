using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 超人（1 费，能力，罕见，固有）：
///   获得 1 层「超人」。升级后，额外把手牌中所有可升级的打击与防御升级一次。
/// 对应塔1 的 aristocrat:Superhuman。
/// </summary>
public sealed class AristocratSuperhuman : AristocratCard
{
    public AristocratSuperhuman()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 固有
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await PowerCmd.Apply<SuperhumanPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);

        if (!IsUpgraded)
        {
            return;
        }

        foreach (CardModel card in PileType.Hand.GetPile(owner).Cards.ToList())
        {
            if (AristocratCard.IsStrikeOrDefend(card) && card.CurrentUpgradeLevel < card.MaxUpgradeLevel)
            {
                CardCmd.Upgrade(card);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
