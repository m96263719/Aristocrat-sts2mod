using System.Collections.Generic;
using Aristocrat.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 把「超人」的层数接到卡牌的升级上限上。
///
/// 塔2 原本每张牌只能升级 1 次（MaxUpgradeLevel 默认返回 1），
/// 这里把打击与防御的上限抬到 1 + 超人层数，于是它们就能反复升级了。
/// 战斗中升级只影响本场战斗的那份卡牌副本，所以这就是"临时升级"。
///
/// 没有「超人」时这里完全不插手，保持塔2 原本的行为。
/// </summary>
[HarmonyPatch(typeof(CardModel), "get_MaxUpgradeLevel")]
internal static class UpgradeCapPatch
{
    private static readonly HashSet<string> Logged = [];

    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref int __result)
    {
        // 卡牌总览、奖励预览这些地方用的是"规范模型"（不可变），
        // 而 CardModel.Owner 的取值会断言实例可变，读一下就抛异常。
        // 这里先挡掉，否则整个卡牌总览会因为一张牌报错而空白。
        if (!__instance.IsMutable)
        {
            return;
        }

        if (!AristocratCard.IsStrikeOrDefend(__instance))
        {
            return;
        }

        Player? owner = __instance.Owner;
        if (owner == null)
        {
            return;
        }

        int extra = owner.Creature.GetPowerAmount<SuperhumanPower>();
        if (extra <= 0)
        {
            return;
        }

        int allowed = 1 + extra;
        if (__result < allowed)
        {
            __result = allowed;

            string key = $"{__instance.Id}|{__instance.CurrentUpgradeLevel}|{extra}";
            if (Logged.Add(key))
            {
                Log.Info($"[Aristocrat] 升级上限：{__instance.Id.Entry} 当前+{__instance.CurrentUpgradeLevel}，超人 {extra} 层 → 上限 {allowed}");
            }
        }
    }
}
