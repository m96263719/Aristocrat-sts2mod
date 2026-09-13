using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Cards;

/// <summary>
/// 加冕（2 费，技能，稀有，固有，消耗）：本场战斗中，把手牌、抽牌堆、弃牌堆里的
/// 打击、防御和固有牌全部升级。升级后费用降为 1。对应塔1 的 aristocrat:Coronation。
/// </summary>
public sealed class AristocratCoronation : AristocratCard
{
    public AristocratCoronation()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        foreach (PileType pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard })
        {
            foreach (CardModel card in pileType.GetPile(owner).Cards.ToList())
            {
                if (ReferenceEquals(card, this))
                {
                    continue;
                }

                bool shouldUpgrade = IsStrikeOrDefend(card) || HasKeyword(card, CardKeyword.Innate);
                if (shouldUpgrade && card.CurrentUpgradeLevel < card.MaxUpgradeLevel)
                {
                    CardCmd.Upgrade(card);
                }
            }
        }

        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}