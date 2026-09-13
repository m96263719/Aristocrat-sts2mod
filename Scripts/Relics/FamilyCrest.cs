using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Aristocrat.Relics;

/// <summary>
/// 家族纹章（起始遗物），对应塔1 的 aristocrat:FamilyCrest：
///   * 第一场战斗被替换为商店
///   * 精英战斗后开启商店
///   * 每次访问商店时获得 50 金币
///
/// 前两条的判定放在 FamilyCrestShopRules 里，从存档数据推导，所以存读档不会失效。
/// </summary>
public sealed class FamilyCrest : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override string PackedIconPath => "res://Aristocrat/images/relics/FamilyCrest.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/FamilyCrestOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/FamilyCrest.png";

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        Player? owner = Owner;
        if (owner == null || room is not MerchantRoom)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainGold(50, owner);
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