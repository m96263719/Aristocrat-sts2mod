using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Aristocrat.Relics;

/// <summary>
/// 瓶装混沌（罕见）：塔1 的 aristocrat:BottledChaos —— 拾取时选一张牌装瓶，
/// 之后每场战斗开始时这张牌都会出现在起手手牌里。
///
/// 塔1 记的是卡牌的 uuid（升级后 uuid 不变），塔2 的 CardModel 没有 uuid，
/// 所以这里按 [SavedProperty] 存一份 SerializableCard（含升级等级 / 附魔）：
/// 先按"同 ID 同升级等级"找，找不到再退化成"同 ID"，免得升级过的牌失去瓶装效果。
///
/// 起手判定用 BeforeHandDrawLate + MoveToTopInternal：塔2 抽起手牌就是从抽牌堆顶开始抽的
/// （CardPilePosition.Top 就是索引 0），所以把牌挪到堆顶就等于"起手必有"。
/// </summary>
public sealed class BottledChaos : RelicModel
{
    private SerializableCard? _bottledCard;

    private IEnumerable<IHoverTip> _extraHoverTips = Array.Empty<IHoverTip>();

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => "res://Aristocrat/images/relics/BottledChaos.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/BottledChaosOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/BottledChaos.png";

    /// <summary>瓶中的牌。存的是牌本身的快照，读档后照样能对上。</summary>
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public SerializableCard? BottledCard
    {
        get => _bottledCard;
        set
        {
            AssertMutable();
            _bottledCard = value;
            _extraHoverTips = BuildHoverTips(value);
        }
    }

    /// <summary>把瓶中的牌做成悬浮提示，这样遗物说明里能直接看到装的是什么。</summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips => _extraHoverTips;

    public override async Task AfterObtained()
    {
        Player owner = Owner;
        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1);
        CardModel? chosen = (await CardSelectCmd.FromDeckGeneric(owner, prefs, card => card.Type != CardType.Quest)).FirstOrDefault();
        if (chosen == null)
        {
            return;
        }

        BottledCard = chosen.ToSerializable();
        Flash();
    }

    public override Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner || BottledCard?.Id == null)
        {
            return Task.CompletedTask;
        }

        if (player.PlayerCombatState is not { } state || state.TurnNumber != 1)
        {
            return Task.CompletedTask;
        }

        CardModel? card = FindBottledCard(state.DrawPile);
        if (card == null)
        {
            return Task.CompletedTask;
        }

        Flash();
        state.DrawPile.MoveToTopInternal(card);
        return Task.CompletedTask;
    }

    private CardModel? FindBottledCard(CardPile drawPile)
    {
        ModelId id = BottledCard!.Id!;
        return drawPile.Cards.FirstOrDefault(card => card.Id == id && card.CurrentUpgradeLevel == BottledCard.CurrentUpgradeLevel)
            ?? drawPile.Cards.FirstOrDefault(card => card.Id == id);
    }

    private static IEnumerable<IHoverTip> BuildHoverTips(SerializableCard? card)
    {
        if (card?.Id == null)
        {
            return Array.Empty<IHoverTip>();
        }

        return [HoverTipFactory.FromCard(SaveUtil.CardOrDeprecated(card.Id), card.CurrentUpgradeLevel > 0)];
    }
}
