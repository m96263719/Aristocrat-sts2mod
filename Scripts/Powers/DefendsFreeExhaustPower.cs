using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 冷静一击：本回合内，你手中的防御费用变为 0，但获得消耗。
/// 回合结束时随能力一起失效。
///
/// [消耗] 同样走 TryModifyKeywordsInCombat（动态关键词），不往牌上加死——
/// 之前直接 AddKeyword 有两个毛病：变出来的究极防御不带消耗，而且能力到期后
/// 关键词还留在那张牌上（整场战斗都变成消耗）。
/// 判定不按牌堆过滤：牌打出去时已经在打出堆里，只认手牌的话消耗就判不出来了。
/// </summary>
public sealed class DefendsFreeExhaustPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card.Owner?.Creature != Owner || !AristocratCard.IsDefend(card))
        {
            return false;
        }

        return keywords.Add(CardKeyword.Exhaust);
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ApplyToHand();
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner?.Creature == Owner)
        {
            Apply(card);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Any(creature => creature == Owner))
        {
            await PowerCmd.Remove(this);
        }
    }

    private void ApplyToHand()
    {
        if (Owner?.Player is not { } player)
        {
            return;
        }

        foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
        {
            Apply(card);
        }
    }

    private static void Apply(CardModel card)
    {
        if (!AristocratCard.IsDefend(card))
        {
            return;
        }

        card.SetToFreeThisTurn();
    }
}
