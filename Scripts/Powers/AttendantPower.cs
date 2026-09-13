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
/// 侍从！：本回合内，你的打击与防御费用变为 0（手中已有的和之后抽到的都算）。
///
/// 塔1 的做法是自己记录费用、打出第一张后再手动还原；塔2 有现成的
/// CardModel.SetToFreeThisTurn()，而且它会在回合结束时自动失效，所以不需要还原逻辑。
/// 这里从简：整回合内一直生效，回合结束随能力一起消失。
/// </summary>
public sealed class AttendantPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        FreeHand();
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner?.Creature == Owner)
        {
            Free(card);
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

    private void FreeHand()
    {
        if (Owner is not { } owner)
        {
            return;
        }

        Player? player = owner.Player;
        if (player == null)
        {
            return;
        }

        foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
        {
            Free(card);
        }
    }

    private static void Free(CardModel card)
    {
        if (AristocratCard.IsStrikeOrDefend(card))
        {
            card.SetToFreeThisTurn();
        }
    }
}