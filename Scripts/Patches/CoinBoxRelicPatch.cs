using System.Linq;
using Aristocrat.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 钱币盒：把「接下来 2 次获得的遗物」换成老旧钱币。
///
/// 这是兜底路径——战斗奖励以外的入口（商店购买、事件、宝箱等）最后都会走 RelicCmd.Obtain，
/// 所以在这里统一替换。奖励条目本身已经在 CoinBox.TryModifyRewards 里换成老旧钱币了，
/// 那种情况下拿到的本来就是老旧钱币（在替换范围外），不会重复扣次数。
///
/// 用前缀改参数是因为 Obtain 是 async：状态机会在方法体里捕获参数，
/// 前缀改完再进原方法，后面的流程拿到的就是改过的遗物。
/// </summary>
[HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), new[] { typeof(RelicModel), typeof(Player), typeof(int) })]
internal static class CoinBoxRelicPatch
{
    [HarmonyPrefix]
    private static void Prefix(ref RelicModel relic, Player player)
    {
        CoinBox? box = player.Relics.OfType<CoinBox>().FirstOrDefault();
        if (box == null || CoinBox.IsExempt(relic))
        {
            return;
        }

        if (box.TryConsume())
        {
            relic = CoinBox.NewOldCoin();
        }
    }
}
