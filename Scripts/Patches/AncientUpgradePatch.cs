using System;
using System.Collections.Generic;
using Aristocrat.Cards;
using Aristocrat.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Aristocrat.Patches;

/// <summary>
/// 先古之民「奥罗巴斯」(Hive 章节) 有两个"升级你的起手配置"的选项：
///   * 奥罗巴斯的触碰 (TouchOfOrobas)：把初始遗物替换成它的先古版本
///   * 太古之齿 (ArchaicTooth)    ：把一张起始牌替换成它的先古版本
///
/// 塔2 把两张映射表都写死在代码里（只覆盖五个本体角色），所以：
///   * 奥罗巴斯的触碰对贵族会退化成给一个「头环」——纯废品
///   * 太古之齿对贵族根本不会出现（SetupForPlayer 找不到可转化的牌）
///
/// 这里各补一条映射，剩下的流程（换遗物、替换卡牌并继承升级状态）全部交给原版逻辑。
/// 两张表原本就是每次访问现构造的，所以直接往里加不会污染别的角色。
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas), "get_RefinementUpgrades")]
internal static class TouchOfOrobasPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, RelicModel> __result)
    {
        try
        {
            // 家族纹章 → 家族荣誉（拿到「奥罗巴斯的触碰」时会把纹章换成荣誉）
            __result[ModelDb.Relic<FamilyCrest>().Id] = ModelDb.Relic<FamilyHonor>();
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 奥罗巴斯的触碰：注册家族纹章升级失败: {ex.GetBaseException().Message}");
        }
    }
}

[HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
internal static class ArchaicToothPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        try
        {
            // 征收 → 强制征税（拿到「太古之齿」时会替换一张起始的征收）
            __result[ModelDb.Card<AristocratTaxation>().Id] = ModelDb.Card<AristocratMandatoryTaxation>();
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 太古之齿：注册征收升级失败: {ex.GetBaseException().Message}");
        }
    }
}
