using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Perk;

/// <summary>特典效果要用到的牌组操作。</summary>
public static class PerkHelpers
{
    /// <summary>
    /// 往主牌组里加一张牌（可选以升级状态加入）。
    /// 加完要自己调一次 PreviewCardPileAdd（本体的多利之镜就是这么写的），
    /// 否则"牌飞进牌组"的预览动画不会播，玩家什么都看不到。
    /// </summary>
    public static async Task AddCardToDeck<T>(Player player, bool upgraded = false) where T : CardModel
    {
        CardModel card = player.RunState.CreateCard<T>(player);
        if (upgraded)
        {
            CardCmd.Upgrade(card);
        }

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Top, null, false));
    }

    /// <summary>
    /// 把牌组里第一张满足条件的牌变化成另一张牌。
    ///
    /// 两个要点：
    ///   1. 会继承原牌的升级等级（塔1 的 transformMasterDeckCard 也是这么做的）；
    ///   2. 会继承原牌的关键词与附魔（AristocratCard.InheritCardModifiers）；
    ///   3. 具体挑哪张由调用方的 predicate 决定，特典里应该排除"已经是究极版本"的牌，
    ///      否则会白白变化一张究极牌。
    /// </summary>
    public static async Task<bool> TransformFirstInDeck<T>(Player player, Func<CardModel, bool> predicate) where T : CardModel
    {
        CardModel? target = player.Deck.Cards.FirstOrDefault(predicate);
        if (target == null)
        {
            return false;
        }

        int upgradeLevel = target.CurrentUpgradeLevel;
        CardModel replacement = player.RunState.CreateCard<T>(player);

        // 继承原牌的升级等级（受目标牌自身的升级上限约束）
        for (int i = 0; i < upgradeLevel && replacement.CurrentUpgradeLevel < replacement.MaxUpgradeLevel; i++)
        {
            CardCmd.Upgrade(replacement);
        }

        // 关键词 / 附魔也跟着走
        AristocratCard.InheritCardModifiers(replacement, target);

        await CardCmd.Transform(target, replacement);
        return true;
    }

    /// <summary>升级牌组里所有满足条件的牌。</summary>
    public static int UpgradeAllInDeck(Player player, Func<CardModel, bool> predicate)
    {
        int count = 0;
        foreach (CardModel card in player.Deck.Cards.ToList())
        {
            if (!predicate(card) || card.CurrentUpgradeLevel >= card.MaxUpgradeLevel)
            {
                continue;
            }

            CardCmd.Upgrade(card);
            count++;
        }

        return count;
    }
/// <summary>随机升级牌组里一张满足条件的牌。</summary>
    public static bool UpgradeRandomInDeck(Player player, Func<CardModel, bool> predicate)
    {
        List<CardModel> candidates = player.Deck.Cards
            .Where(card => predicate(card) && card.CurrentUpgradeLevel < card.MaxUpgradeLevel)
            .ToList();

        if (candidates.Count == 0)
        {
            return false;
        }

        CardCmd.Upgrade(candidates[Random.Shared.Next(candidates.Count)]);
        return true;
    }
}
