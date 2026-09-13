using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 超人：打击与防御可以额外升级 Amount 次。
///
/// 塔1 里的写法是把判定塞进卡牌的 canUpgrade()：timesUpgraded &lt; 1 + 能力层数。
/// 塔2 的卡牌有 MaxUpgradeLevel 属性，所以这里反过来做——能力只是个记号，
/// 由 UpgradeCapPatch 读它的层数去抬高升级上限。
/// </summary>
public sealed class SuperhumanPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
