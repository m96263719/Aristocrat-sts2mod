using System;
using Aristocrat.Perk;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Aristocrat.Patches;

/// <summary>
/// 永久关键词（肖像特典给的[固有]）剩下的两个补丁。
///
/// 存档本身交给 BaseLib 的 SavedSpireField（见 PersistentKeywordTracker）：
/// 它会把这个字段注册进 SavedPropertiesTypeCache 并接管 JSON 的导出/导入，
/// 所以这里不需要碰 ToSerializable / FromSerializable 的存档逻辑，也不用管复制牌。
///
/// 需要自己补的只有两处——因为**关键词本身**不在存档里，只存了"要带着它"的标记：
///   1. 读档后把关键词补回去；
///   2. `DowngradeInternal` 会拿本体牌重建 `_keywords`，临时升级结束时关键词会被冲掉。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable), new[] { typeof(SerializableCard) })]
internal static class PersistentKeywordLoadPatch
{
    [HarmonyPostfix]
    private static void Postfix(SerializableCard save, CardModel __result)
    {
        PersistentKeywordTracker.MigrateLegacyMarker(__result, save);
        PersistentKeywordTracker.RestoreInnate(__result);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.DowngradeInternal), new Type[0])]
internal static class PersistentKeywordDowngradePatch
{
    [HarmonyPrefix]
    private static void Prefix(CardModel __instance, out bool __state)
    {
        __state = PersistentKeywordTracker.IsPersistentInnate(__instance);
    }

    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, bool __state)
    {
        if (__state)
        {
            PersistentKeywordTracker.RestoreInnate(__instance);
        }
    }
}
