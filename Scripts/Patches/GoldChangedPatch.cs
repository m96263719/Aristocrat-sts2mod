using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Aristocrat.Patches;

/// <summary>
/// 金币变动的统一广播点，挂在 PlayerCmd 的加钱/扣钱两个方法上。
///
/// 为什么不能挂 Player.Gold 的 setter：塔2 的战斗是一整条 await 链，从 setter 里"甩"出去一个
/// 自己跑的任务是接不进这条链的——加钱那次碰巧能生效，扣钱那次就静悄悄地没了（实测就是只有
/// 获得金币能触发虚荣）。改成在命令方法的 postfix 里返回 Task.WhenAll，调用方 await 的时候
/// 就一定会等到效果结算完。
///
/// 非战斗场景（读档、事件）由 AristocratGold.Notify 里的 CombatState 判空挡掉。
/// </summary>
[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.GainGold))]
internal static class GoldGainedNotifyPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Task __result, Player player, decimal amount)
    {
        if (amount > 0m)
        {
            __result = Task.WhenAll(__result, AristocratGold.Notify(player, (int)amount));
        }
    }
}

[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.LoseGold))]
internal static class GoldLostNotifyPatch
{
    [HarmonyPrefix]
    private static void Prefix(Player player, decimal amount, out int __state)
    {
        // 塔1 的 loseGold 会把金币夹到 0，所以"真正扣掉的量"要自己算
        __state = (int)Math.Min(amount, Math.Max(0, player.Gold));
    }

    [HarmonyPostfix]
    private static void Postfix(ref Task __result, Player player, int __state)
    {
        if (__state > 0)
        {
            __result = Task.WhenAll(__result, AristocratGold.Notify(player, -__state));
        }
    }
}
