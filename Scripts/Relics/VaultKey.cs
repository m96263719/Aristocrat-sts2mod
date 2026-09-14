using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Relics;

/// <summary>
/// 宝库钥匙（稀有）：塔1 的 aristocrat:VaultKey —— 商店里买的稀有牌都带一条额外效果：
/// 「特典：获得一件随机遗物。」
///
/// 范围要在"商店里买下来的那张牌"上，不能按"稀有度 + 你有钥匙"推导——那样连奖励里拿到的稀有牌也会带上。
/// 所以流程是：
///   1. 商店摆货时（ModifyMerchantCardCreationResults）给稀有牌实例打一个标记，
///      「是有/否」这点状态跟着这张牌存档（见 Perk\VaultKeyPerkTracker，BaseLib 的 SavedSpireField），
///      所以商店里卡面就有说明、买下之后牌组里也一直有、SL 也不掉；
///   2. 买下时照旧触发一次特典（发一件随机遗物）；
///   3. 之后这张牌算"特典牌"，能被「天生支配者」这类特典触发效果再触发——
///      判定与触发统一走 Perk\PerkSystem；
///   4. 卡面那行说明由 Patches\VaultKeyPerkTextPatch 补上。
/// </summary>
public sealed class VaultKey : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => "res://Aristocrat/images/relics/VaultKey.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/VaultKeyOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/VaultKey.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(AristocratKeywords.Perk)];

    /// <summary>商店摆牌时给稀有牌打上特典标记（买下后跟着牌一起走）。</summary>
    public override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
    {
        if (player != Owner)
        {
            return;
        }

        foreach (CardCreationResult result in cards)
        {
            MarkShopCard(result.Card);
        }
    }

    /// <summary>
    /// 给"在商店里买下的稀有牌"打特典标记。
    /// 蛋系遗物那种在后面克隆/改牌的钩子会被 CopyOnClone 带过去，所以谁先谁后都不怕。
    /// </summary>
    internal static void MarkShopCard(CardModel? card)
    {
        if (card is { } target && target.Rarity == CardRarity.Rare)
        {
            VaultKeyPerkTracker.Mark(target);
        }
    }

    /// <summary>
    /// 宝库钥匙的特典本体：发一件随机遗物。
    /// 由 <see cref="PerkSystem.TriggerPerk"/> 统一调用（商店买下稀有牌、或者别的效果触发特典时）。
    /// </summary>
    internal static async Task GrantRelic(Player owner)
    {
        // 钥匙还在就闪一下（牌身上带着的特典即使钥匙后来没了也照常发）
        owner.Relics.OfType<VaultKey>().FirstOrDefault()?.Flash();
        await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(owner).ToMutable(), owner);
    }
}
