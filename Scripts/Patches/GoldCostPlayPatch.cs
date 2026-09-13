using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 金币不足的牌不能打出，并弹一句"我的金币不足"。
///
/// 塔2 的"不能打出"是一整套现成的东西：CardModel.CanPlay 给出 UnplayableReason，
/// 玩家点牌时 NMouseCardPlay 会拿 reason 去换一句话，用人物的思想气泡弹出来。
/// 但枚举里没有"金币不足"这一项，所以：
///   1. 在 CanPlay 之后给这类牌塞一个自定义的 reason 位；
///   2. 在 UnplayableReasonExtensions.GetPlayerDialogueLine 之后把这句话换成我们自己的文本。
///      （那个类在游戏程序集里是 internal，只能用名字去找）
/// </summary>
internal static class GoldCostReasons
{
    /// <summary>自定义的"金币不足"标记（避开枚举里已有的位）。</summary>
    internal const UnplayableReason NotEnoughGold = (UnplayableReason)0x4000;
}

[HarmonyPatch]
internal static class GoldCostCanPlayPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        // CanPlay 有两个重载，这里要的是带 out 参数的那个
        return AccessTools.Method(
            typeof(CardModel),
            nameof(CardModel.CanPlay),
            [typeof(UnplayableReason).MakeByRefType(), typeof(AbstractModel).MakeByRefType()])!;
    }

    [HarmonyPostfix]
    private static void CanPlayPostfix(
        CardModel __instance,
        ref UnplayableReason reason,
        ref AbstractModel? preventer,
        ref bool __result)
    {
        if (__instance is not IGoldCostCard goldCost)
        {
            return;
        }

        int gold = __instance.Owner?.Gold ?? 0;
        if (gold >= goldCost.GoldCost)
        {
            return;
        }

        reason = GoldCostReasons.NotEnoughGold;
        preventer = __instance;
        __result = false;
    }
}

[HarmonyPatch]
internal static class GoldCostDialoguePatch
{
    [HarmonyPatch("MegaCrit.Sts2.Core.Entities.Cards.UnplayableReasonExtensions", "GetPlayerDialogueLine")]
    [HarmonyPostfix]
    private static void DialoguePostfix(UnplayableReason reason, ref LocString? __result)
    {
        if (reason == GoldCostReasons.NotEnoughGold)
        {
            __result = new LocString("combat_messages", "ARISTOCRAT_NOT_ENOUGH_GOLD");
        }
    }
}
