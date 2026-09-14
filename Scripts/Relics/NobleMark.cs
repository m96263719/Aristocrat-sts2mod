using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Relics;

/// <summary>
/// 贵族印记（塔1 是 BOSS 级，塔2 对应先古级）：塔1 的 aristocrat:NobleMark。
///   * 每回合多 1 点能量（塔1 直接 energyMaster +1，塔2 对应的就是 ModifyMaxEnergy）
///   * 拾取时从牌组里挑最多 3 张打击 / 防御，各复制一张
///
/// 复制用的是 RunState.CloneCard（和「多利之镜」同一套），所以升级、附魔都会一起复制。
/// </summary>
public sealed class NobleMark : RelicModel
{
    /// <summary>最多复制几张（塔1 是 min(3, 候选数)）。</summary>
    private const int CopyCount = 3;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => "res://Aristocrat/images/relics/NobleMark.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/NobleMarkOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/NobleMark.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != Owner)
        {
            return amount;
        }

        return amount + DynamicVars.Energy.BaseValue;
    }

    public override async Task AfterObtained()
    {
        Player owner = Owner;
        int available = owner.Deck.Cards.Count(AristocratCard.IsStrikeOrDefend);
        if (available == 0)
        {
            return;
        }

        Flash();

        // 候选不够 3 张时就按实际数量选，避免出现"要选 3 张但只有 2 张"的卡死界面
        CardSelectorPrefs prefs = new(SelectionScreenPrompt, Math.Min(CopyCount, available));
        foreach (CardModel card in await CardSelectCmd.FromDeckGeneric(owner, prefs, AristocratCard.IsStrikeOrDefend))
        {
            CardModel copy = owner.RunState.CloneCard(card);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(copy, PileType.Deck));
        }
    }
}
