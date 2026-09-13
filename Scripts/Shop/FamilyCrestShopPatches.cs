using System;
using System.Threading.Tasks;
using Aristocrat.Shop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Aristocrat.Shop;

/// <summary>
/// 家族纹章的两条商店机制。
///
/// 判定条件全部从游戏的存档数据（房间历史 + 当前房间）推导，不保存自定义状态，
/// 所以存读档不会丢。
///
/// 时机上也有讲究：不能在房间切换流程内部再发起一次房间切换（会撞上同步/加载锁卡死），
/// 所以两条机制都挑"界面调用"这种安全时机：
///   1) 第一场普通战斗 → 在游戏建房的地方（RunManager.CreateRoom）直接换成商店；
///   2) 精英战斗后 → 和塔1 一样挂在"打开地图"那一刻（NMapScreen.Open）。
/// </summary>
[HarmonyPatch]
internal static class FamilyCrestShopPatches
{
    /// <summary>正在开商店，防止 NMapScreen.Open 被重入时又触发一次。</summary>
    private static bool _openingShop;

    [HarmonyPatch(typeof(RunManager), "CreateRoom")]
    [HarmonyPrefix]
    private static bool CreateRoomPrefix(RunManager __instance, RoomType roomType, MapPointType mapPointType, ref AbstractRoom __result)
    {
        if (roomType != RoomType.Monster || mapPointType == MapPointType.Unassigned)
        {
            return true;
        }

        RunState? state = __instance.DebugOnlyGetState();
        if (state == null || !FamilyCrestShopRules.ShouldReplaceFirstCombat(state))
        {
            return true;
        }

        __result = new MerchantRoom();
        Log.Info("[Aristocrat] 家族纹章：第一场战斗被替换为商店。");
        return false;
    }

    [HarmonyPatch(typeof(NMapScreen), "Open")]
    [HarmonyPrefix]
    private static bool MapOpenPrefix(NMapScreen __instance, ref NMapScreen __result)
    {
        if (_openingShop)
        {
            return true;
        }

        RunState? state = RunManager.Instance.DebugOnlyGetState();
        if (state == null || !FamilyCrestShopRules.ShouldOpenPostEliteShop(state))
        {
            return true;
        }

        __result = __instance;
        _ = OpenPostEliteShop(state);
        return false;
    }

    private static async Task OpenPostEliteShop(RunState state)
    {
        _openingShop = true;
        try
        {
            Log.Info("[Aristocrat] 家族纹章：开始进入精英战后的商店。");
            Task entering = RunManager.Instance.EnterMapPointInternal(state.ActFloor, MapPointType.Shop, null, saveGame: false);

            Task finished = await Task.WhenAny(entering, Task.Delay(TimeSpan.FromSeconds(10)));
            if (finished != entering)
            {
                Log.Error("[Aristocrat] 家族纹章：进入商店超过 10 秒仍未完成，疑似卡住。");
                return;
            }

            await entering;
            Log.Info("[Aristocrat] 家族纹章：精英战后的商店已开启。");
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 家族纹章：开启精英商店失败：{ex}");
        }
        finally
        {
            _openingShop = false;
        }
    }
}