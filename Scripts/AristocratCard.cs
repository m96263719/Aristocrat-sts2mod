using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Character;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat;

/// <summary>
/// 贵族卡牌的公共基类：统一卡图路径与卡池，并提供一批打防机制要用的工具。
/// </summary>
public abstract class AristocratCard : CardModel
{
    protected AristocratCard(int cost, CardType type, CardRarity rarity, TargetType target)
        : base(cost, type, rarity, target)
    {
    }

    public override CardPoolModel Pool => ModelDb.CardPool<AristocratCardPool>();

    public override string PortraitPath => $"res://Aristocrat/images/cards/{GetType().Name}.png";

    /// <summary>
    /// 这张牌算不算"打击 / 防御"。
    /// 塔1 用的是 CardTags.STRIKE / STARTER_DEFEND，塔2 换成了 CardTag，概念一一对应。
    /// </summary>
    public static bool IsStrikeOrDefend(CardModel? card)
    {
        if (card == null)
        {
            return false;
        }

        foreach (CardTag tag in card.Tags)
        {
            if (tag == CardTag.Strike || tag == CardTag.Defend)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsStrike(CardModel? card) => HasTag(card, CardTag.Strike);

    public static bool IsDefend(CardModel? card) => HasTag(card, CardTag.Defend);

    private static bool HasTag(CardModel? card, CardTag tag)
    {
        if (card == null)
        {
            return false;
        }

        foreach (CardTag t in card.Tags)
        {
            if (t == tag)
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasKeyword(CardModel card, CardKeyword keyword) => card.Keywords.Contains(keyword);

    /// <summary>这张牌是不是带特典的牌（用于「抽一张特典牌」这类效果）。</summary>
    public static bool IsPerkCard(CardModel card) => card is Perk.IPerkCard;

    /// <summary>取一个可攻击的敌人，作为自动打出卡牌的目标。</summary>
    public static Creature? PickRandomEnemy(Player player)
    {
        if (player.Creature.CombatState is not CombatState combat)
        {
            return null;
        }

        var enemies = combat.HittableEnemies.ToList();
        return enemies.Count == 0 ? null : enemies[Random.Shared.Next(enemies.Count)];
    }

    /// <summary>从抽牌堆里随机抽若干张满足条件的牌上手（对应塔1 的 DrawMatchingCardAction）。</summary>
    public static async Task DrawMatchingFromDrawPile(
        PlayerChoiceContext choiceContext,
        Player player,
        int count,
        Func<CardModel, bool> predicate,
        Action<CardModel>? afterDraw = null)
    {
        CardPile drawPile = PileType.Draw.GetPile(player);
        var candidates = drawPile.Cards.Where(predicate).ToList();

        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            CardModel card = candidates[Random.Shared.Next(candidates.Count)];
            candidates.Remove(card);

            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top, null, false);
            afterDraw?.Invoke(card);
        }
    }

    /// <summary>把手牌里所有满足条件的牌按顺序自动打出（对应塔1 的 UseMatchingCardsInHandAction）。</summary>
    public static async Task PlayAllMatchingInHand(PlayerChoiceContext choiceContext, Player player, Func<CardModel, bool> predicate)
    {
        CardPile hand = PileType.Hand.GetPile(player);
        foreach (CardModel card in hand.Cards.ToList())
        {
            if (!predicate(card))
            {
                continue;
            }

            // 打出的过程中手牌会变化，所以每张都要重新确认它还在手里
            if (!hand.Cards.Contains(card))
            {
                continue;
            }

            Creature? target = card.TargetType == TargetType.AnyEnemy ? PickRandomEnemy(player) : null;
            await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default);
        }
    }

    /// <summary>把一堆牌里还能升级的普通打击/防御升一级。</summary>
    public static void UpgradeStrikesAndDefends(IEnumerable<CardModel> cards, CardModel? except = null)
    {
        foreach (CardModel card in cards.ToList())
        {
            if (ReferenceEquals(card, except))
            {
                continue;
            }

            if (IsStrikeOrDefend(card) && card.CurrentUpgradeLevel < card.MaxUpgradeLevel)
            {
                CardCmd.Upgrade(card);
            }
        }
    }
}