using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Aristocrat.Relics;

/// <summary>
/// 家族荣誉：家族纹章的先古升级版，对应塔1 的 aristocrat:FamilyHonor。
///
/// 除了继承纹章那两条商店机制（第一场战斗变商店、精英战后开商店）之外：
///   * 每次访问商店获得 100 金币（纹章是 50）
///   * 商店所有商品打八折（这条是移植时新加的）
/// </summary>
public sealed class FamilyHonor : RelicModel
{
    /// <summary>商店折扣比例：打八折。</summary>
    private const decimal DiscountMultiplier = 0.8m;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => "res://Aristocrat/images/relics/FamilyHonor.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/FamilyHonorOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/FamilyHonor.png";

    /// <summary>商店里的所有东西打八折。塔2 原生就有这个钩子，不用去改商店逻辑。</summary>
    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
    {
        if (Owner != player)
        {
            return cost;
        }

        return Math.Ceiling(cost * DiscountMultiplier);
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        Player? owner = Owner;
        if (owner == null || room is not MerchantRoom)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainGold(100, owner);
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        if (Owner != null && room.RoomType == RoomType.Elite)
        {
            Flash();
        }

        return Task.CompletedTask;
    }
}