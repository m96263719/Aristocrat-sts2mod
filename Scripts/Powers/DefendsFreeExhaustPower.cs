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
/// </summary>
public sealed class DefendsFreeExhaustPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

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
        CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
    }
}