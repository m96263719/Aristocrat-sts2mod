using System.Collections.Generic;
using Aristocrat.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace Aristocrat.Shop;

/// <summary>
/// 家族纹章两条商店机制的判定。
///
/// 关键点：这里**不保存任何自定义状态**，全部从游戏自己的存档数据里推导。
/// 塔2 的 RunState.MapPointHistory 会记录每个地图节点进过哪些房间，而且它本身就是存档内容，
/// 所以这样推导出来的结果，存档、读档、退出重进都不会丢。
///
/// （对比塔1：那边要自己写一整套存档读写，还得在载入时把坐标、原房间类型都还原回去。）
/// </summary>
public static class FamilyCrestShopRules
{
    /// <summary>当前这局有没有人带着家族纹章或它的升级版家族荣誉。</summary>
    public static bool AnyPlayerHasRelic(IRunState state)
    {
        foreach (Player player in state.Players)
        {
            if (player.GetRelic<FamilyCrest>() != null || player.GetRelic<FamilyHonor>() != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 本局是否已经用过"首战变商店"。
    /// 判定方式：房间历史里有没有出现过"地图节点是普通战斗、但实际进的是商店"这种组合——
    /// 那正是我们替换出来的那一场。
    /// </summary>
    public static bool FirstCombatShopUsed(IRunState state)
    {
        foreach (IReadOnlyList<MapPointHistoryEntry> act in state.MapPointHistory)
        {
            foreach (MapPointHistoryEntry entry in act)
            {
                if (entry.MapPointType == MapPointType.Monster && entry.HasRoomOfType(RoomType.Shop))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>这场普通战斗是否应该被换成商店。</summary>
    public static bool ShouldReplaceFirstCombat(IRunState state)
    {
        return AnyPlayerHasRelic(state) && !FirstCombatShopUsed(state);
    }

    /// <summary>
    /// 是否该在打开地图时插入精英战后的商店。
    ///
    /// 判定方式：当前房间是一场已经打完的精英战斗，而且这个节点的历史里还没有商店。
    /// 读档后回到"打完精英、正在选奖励"的状态时，这个条件依然成立，所以商店不会丢。
    /// 商店开完之后当前房间变成商店，条件自然不成立，不会重复触发。
    /// </summary>
    public static bool ShouldOpenPostEliteShop(IRunState state)
    {
        if (!AnyPlayerHasRelic(state))
        {
            return false;
        }

        if (state.CurrentRoom is not CombatRoom combat || combat.RoomType != RoomType.Elite)
        {
            return false;
        }

        MapPointHistoryEntry? entry = state.CurrentMapPointHistoryEntry;
        return entry != null && !entry.HasRoomOfType(RoomType.Shop);
    }
}