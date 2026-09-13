using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 法度：本场战斗中你的固有牌具有保留；固有牌每被保留下来一次，它的费用就降低 1（本场战斗内累积）。
///
/// 塔1 在回合结束前遍历手牌 reduceCostForCombat。塔2 有 AfterFlush 钩子，直接给出"本回合被保留的牌"，
/// 记下次数再用 TryModifyEnergyCostInCombat 抵扣即可，不用手动改费用，也就不需要还原逻辑。
/// </summary>
public sealed class LawPower : PowerModel
{
    private readonly Dictionary<CardModel, int> _retainCounts = [];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        Player? player = Owner.Player;
        if (player != null)
        {
            foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
            {
                GrantRetain(card);
            }
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        GrantRetain(card);
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        GrantRetain(cardPlay.Card);
        return Task.CompletedTask;
    }

    public override Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (player.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        foreach (CardModel card in retainedCards)
        {
            if (!card.Keywords.Contains(CardKeyword.Innate))
            {
                continue;
            }

            _retainCounts[card] = _retainCounts.GetValueOrDefault(card) + 1;
        }

        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Owner?.Creature != Owner || originalCost <= 0m)
        {
            return false;
        }

        int reduction = _retainCounts.GetValueOrDefault(card);
        if (reduction <= 0)
        {
            return false;
        }

        modifiedCost = Math.Max(0m, originalCost - reduction);
        return true;
    }

    private void GrantRetain(CardModel card)
    {
        if (card.Owner?.Creature == Owner && card.Keywords.Contains(CardKeyword.Innate))
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
        }
    }
}
