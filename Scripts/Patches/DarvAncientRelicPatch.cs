using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Aristocrat.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace Aristocrat.Patches;

/// <summary>
/// 先古之民「达夫」(Darv) 就是塔2 的 Boss 遗物来源：它把可选遗物写死在
/// 一张私有静态表 _validRelicSets 里（星盘 / 黑星 / 空笼 / 蛇眼……），每组先随机挑一件再抽 2~3 组。
///
/// 贵族的两个 Boss 级遗物（贵族印记、黑钻石）塔1 是从 Boss 遗物三选一里出的，
/// 塔2 的对应位置就是达夫，所以把这一组补进表里，并且限定只有贵族才抽得到，
/// 免得别的角色看到贵族专属遗物。
///
/// 表里的 ValidRelicSet 是私有嵌套 struct、字段也是私有的，只能反射构造；
/// 好在结构很简单（一个筛选委托 + 一个 RelicModel 数组），注入一次之后原版逻辑照跑。
/// </summary>
[HarmonyPatch(typeof(Darv), "GenerateInitialOptions")]
internal static class DarvAncientRelicPatch
{
    private static bool _injected;

    /// <summary>
    /// 用前缀（不是后缀）：达夫是"先进事件、再算选项"，选项一旦算出来本次就定了，
    /// 所以要在它算之前把表补好，这样第一次遇到达夫就能抽到贵族的遗物。
    /// </summary>
    [HarmonyPrefix]
    private static void Prefix()
    {
        if (_injected)
        {
            return;
        }

        _injected = true;

        try
        {
            // 先碰一次静态字段，确保 Darv 的静态构造已经跑完（表是在静态构造里填的）
            FieldInfo? field = AccessTools.Field(typeof(Darv), "_validRelicSets");
            if (field?.GetValue(null) is not IList sets)
            {
                Log.Error("[Aristocrat] 达夫遗物表注入失败：找不到 _validRelicSets");
                return;
            }

            Type? setType = typeof(Darv).GetNestedType("ValidRelicSet", BindingFlags.NonPublic);
            if (setType == null)
            {
                Log.Error("[Aristocrat] 达夫遗物表注入失败：找不到 ValidRelicSet");
                return;
            }

            Func<Player, bool> onlyAristocrat = player => player.Character is global::Aristocrat.Character.Aristocrat;
            object? set = Activator.CreateInstance(
                setType,
                onlyAristocrat,
                new RelicModel[] { ModelDb.Relic<NobleMark>(), ModelDb.Relic<BlackDiamond>() });

            if (set == null)
            {
                Log.Error("[Aristocrat] 达夫遗物表注入失败：ValidRelicSet 构造失败");
                return;
            }

            sets.Add(set);
            Log.Info("[Aristocrat] 达夫的遗物表已加入贵族印记 / 黑钻石。");
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 达夫遗物表注入失败：{ex.GetBaseException().Message}");
        }
    }
}
