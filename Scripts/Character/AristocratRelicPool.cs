using System.Collections.Generic;
using Aristocrat.Relics;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Character;

/// <summary>
/// 贵族的遗物池。
///
/// 塔2 的遗物是按池子分角色的：抓遗物时把「共享池 + 角色池」拼起来随机
/// （RelicGrabBag.Populate 先取 SharedRelicPool，再取 player.Character.RelicPool），
/// 所以把贵族遗物放进自己的池子，共享池里那些普通遗物照常能出，贵族专属遗物则不会漏给别的角色。
///
/// EnergyColorName 决定资源查找名（images/packed/sprite_fonts/aristocrat_energy_icon.png），
/// 与卡池用的是同一个色名，观者 mod 也是这么做的（WatcherRelicPool）。
/// </summary>
public sealed class AristocratRelicPool : RelicPoolModel
{
    public override string EnergyColorName => "aristocrat";

    public override Color LabOutlineColor => Aristocrat.Gold;

    protected override IEnumerable<RelicModel> GenerateAllRelics() =>
    [
        ModelDb.Relic<FamilyCrest>(),
        ModelDb.Relic<FamilyHonor>(),
        ModelDb.Relic<PlatinumShield>(),
        ModelDb.Relic<OldStatue>(),
        ModelDb.Relic<NobleMark>(),
        ModelDb.Relic<BlackDiamond>(),
        ModelDb.Relic<GoldCirclet>(),
        ModelDb.Relic<CoinBox>(),
        ModelDb.Relic<BottledChaos>(),
        ModelDb.Relic<BlankCheck>(),
        ModelDb.Relic<VaultKey>(),
    ];
}
