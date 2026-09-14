using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Aristocrat.Relics;

/// <summary>
/// 往昔石像（稀有）：塔1 的 aristocrat:OldStatue —— 第 4 回合开始时，把本场战斗里所有的打击
/// 变化为究极打击（手牌 / 抽牌堆 / 弃牌堆，塔1 也是这三个堆）。
///
/// 战斗中的牌都是牌组牌的临时副本，战斗结束就恢复，所以"本场战斗"这条不用自己处理。
/// </summary>
public sealed class OldStatue : RelicModel
{
    /// <summary>第几回合开始时触发（塔1 是 turns == 4）。</summary>
    private const int TriggerTurn = 4;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => "res://Aristocrat/images/relics/OldStatue.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/OldStatueOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/OldStatue.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        if (player.PlayerCombatState is not { } state || state.TurnNumber != TriggerTurn)
        {
            return;
        }

        if (player.Creature.CombatState is not { } combat)
        {
            return;
        }

        List<CardTransformation> transformations = [];
        foreach (CardPile pile in new[] { state.Hand, state.DrawPile, state.DiscardPile })
        {
            foreach (CardModel card in pile.Cards)
            {
                // 已经是究极打击的不再变化（塔1 也是这么排除的）
                if (card is UltimateStrike || !AristocratCard.IsStrike(card) || !card.IsTransformable)
                {
                    continue;
                }

                // 究极打击继承原牌的升级等级
                CardModel replacement = combat.CreateCard<UltimateStrike>(player);
                for (int i = 0; i < card.CurrentUpgradeLevel && replacement.CurrentUpgradeLevel < replacement.MaxUpgradeLevel; i++)
                {
                    CardCmd.Upgrade(replacement);
                }

                // 原牌身上的关键词（比如「上流社交界」给的消耗）和附魔也要一起带过去
                AristocratCard.InheritCardModifiers(replacement, card);

                transformations.Add(new CardTransformation(card, replacement));
            }
        }

        if (transformations.Count == 0)
        {
            return;
        }

        Flash();
        await CardCmd.Transform(transformations, null);
    }
}
