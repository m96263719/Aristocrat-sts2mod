using System;
using Aristocrat.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 黄金冠：把抽取池里刷出来的「头环」换成黄金冠，商店货架上的遗物按概率也换成黄金冠。
///
/// 塔1 的 GoldCircletRewardPatch 管四件事：Boss 遗物三选一里的头环、商店货架、卡牌奖励、遗物奖励。
/// 塔2 的 Boss 遗物改由先古之民给（没有头环），奖励那两条在 GoldCirclet.TryModifyRewardsLate 里做，
/// 这里补上"抽取池"这一层：
///   * 前池（PullNextRelicFromFront）：奖励 / 宝箱。只做头环替换，掷骰子在奖励钩子里做，避免同一条奖励掷两次。
///   * 后池（PullNextRelicFromBack）：商店。头环替换 + 按持有数掷骰子。
///
/// 补丁里的 player 参数就是这批遗物要发给谁，所以能直接按他的持有数算概率。
/// </summary>
[HarmonyPatch(typeof(RelicFactory), nameof(RelicFactory.PullNextRelicFromFront), new[]
{
    typeof(Player), typeof(RelicRarity), typeof(Func<RelicModel, bool>),
})]
internal static class GoldCircletRewardPullPatch
{
    [HarmonyPostfix]
    private static void Postfix(Player player, ref RelicModel __result)
    {
        __result = GoldCirclet.ReplaceCirclet(player, __result);
    }
}

[HarmonyPatch(typeof(RelicFactory), nameof(RelicFactory.PullNextRelicFromBack), new[]
{
    typeof(Player), typeof(RelicRarity), typeof(Func<RelicModel, bool>),
})]
internal static class GoldCircletShopPullPatch
{
    [HarmonyPostfix]
    private static void Postfix(Player player, ref RelicModel __result)
    {
        __result = GoldCirclet.ReplaceShopRelic(player, __result);
    }
}
