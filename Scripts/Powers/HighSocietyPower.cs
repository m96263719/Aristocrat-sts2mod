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
/// 关键词用 TryModifyKeywordsInCombat 动态给（本体的「命运之线」HexPower 给牌加虚无也是这么做的）：
/// 塔1 那边是打在 AbstractCard 上的补丁，所有打击/防御——包括往昔石像变出来的究极打击、
/// 天命塞进手牌的究极防御——都自动带消耗；如果改成挨个 AddKeyword，
/// 新生成 / 变化出来的牌就漏掉了（这正是之前的 bug）。
/// </summary>
public sealed class HighSocietyPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card.Owner?.Creature != Owner || !AristocratCard.IsStrikeOrDefend(card))
        {
            return false;
        }

        return keywords.Add(CardKeyword.Exhaust);
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
}
