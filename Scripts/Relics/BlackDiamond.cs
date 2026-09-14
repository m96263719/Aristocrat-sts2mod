using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Aristocrat.Relics;

/// <summary>
/// 黑钻石（塔1 是 BOSS 级，塔2 对应先古级）：塔1 的 aristocrat:BlackDiamond。
///   * 拾取时升级牌组里所有的打击与防御
///   * 每到一个休息处，随机把 1 张究极打击 / 究极防御加入牌组
///
/// 塔1 用 lastGrantedRestFloor 防止同一层重复发放；塔2 的休息处在读档后会重新触发
/// AfterRoomEntered，所以这条记录也要存档（[SavedProperty]），否则读档进出休息处会白拿牌。
/// </summary>
public sealed class BlackDiamond : RelicModel
{
    private int _lastGrantedRestFloor = -1;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => "res://Aristocrat/images/relics/BlackDiamond.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/BlackDiamondOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/BlackDiamond.png";

    /// <summary>上一次发放奖励的楼层。</summary>
    [SavedProperty]
    public int LastGrantedRestFloor
    {
        get => _lastGrantedRestFloor;
        set
        {
            AssertMutable();
            _lastGrantedRestFloor = value;
        }
    }

    public override Task AfterObtained()
    {
        PerkHelpers.UpgradeAllInDeck(Owner, AristocratCard.IsStrikeOrDefend);
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        Player? owner = Owner;
        if (owner == null || room is not RestSiteRoom)
        {
            return Task.CompletedTask;
        }

        int floor = owner.RunState.TotalFloor;
        if (LastGrantedRestFloor == floor)
        {
            return Task.CompletedTask;
        }

        LastGrantedRestFloor = floor;
        return GrantRandomUltimate(owner);
    }

    private async Task GrantRandomUltimate(Player owner)
    {
        Flash();
        if (owner.RunState.Rng.Niche.NextBool())
        {
            await PerkHelpers.AddCardToDeck<UltimateStrike>(owner);
        }
        else
        {
            await PerkHelpers.AddCardToDeck<UltimateDefend>(owner);
        }
    }
}
