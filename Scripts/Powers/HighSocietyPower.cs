using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Aristocrat.Character;

namespace Aristocrat.Powers;

/// <summary>
/// 上流社交界：本场战斗中，你的打击与防御获得消耗；每当一张打击或防御被消耗，
/// 将一张随机稀有贵族牌加入你的手牌。
///
/// 塔1 用的是 onInitialApplication / onCardDraw / onExhaust 三个时机，塔2 对应
/// AfterApplied / AfterCardDrawn / AfterCardExhausted。
/// </summary>
public sealed class HighSocietyPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        MarkAll();
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner?.Creature == Owner)
        {
            Mark(card);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Mark(cardPlay.Card);
        return Task.CompletedTask;
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Owner?.Creature != Owner || !AristocratCard.IsStrikeOrDefend(card))
        {
            return;
        }

        Player? player = Owner.Player;
        if (player?.Creature.CombatState is null)
        {
            return;
        }

        List<CardModel> rares = ModelDb.CardPool<AristocratCardPool>()
            .AllCards
            .Where(candidate => candidate.Rarity == CardRarity.Rare)
            .ToList();

        if (rares.Count == 0)
        {
            return;
        }

        Flash();
        CardModel reward = player.Creature.CombatState.CreateCard(rares[Random.Shared.Next(rares.Count)], player);
        await CardPileCmd.AddGeneratedCardToCombat(reward, PileType.Hand, player, CardPilePosition.Top);
    }

    private void MarkAll()
    {
        Player? player = Owner.Player;
        if (player == null)
        {
            return;
        }

        foreach (PileType pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust })
        {
            foreach (CardModel card in pileType.GetPile(player).Cards)
            {
                Mark(card);
            }
        }
    }

    private static void Mark(CardModel card)
    {
        if (AristocratCard.IsStrikeOrDefend(card))
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
        }
    }
}