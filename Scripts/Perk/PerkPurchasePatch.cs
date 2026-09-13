using System;
using System.Threading.Tasks;
using Aristocrat.Perk;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Perk;

/// <summary>
/// 商店购买钩子：买下带特典的牌时触发其特典效果。
///
/// 为什么要在"购买开始"时就把牌记下来：购买完成后商店条目会被清空，
/// CreationResult 变成 null，之后就取不到买的是哪张牌了。
/// 所以用前缀把牌存进 __state，再给返回的 Task 接一段后续处理，等购买真正完成再触发。
/// </summary>
[HarmonyPatch(typeof(MerchantEntry), "OnTryPurchaseWrapper")]
internal static class PerkPurchasePatch
{
    [HarmonyPrefix]
    private static void Prefix(MerchantEntry __instance, out CardModel? __state)
    {
        __state = (__instance as MerchantCardEntry)?.CreationResult?.Card;
    }

    [HarmonyPostfix]
    private static void Postfix(ref Task<bool> __result, CardModel? __state)
    {
        if (__state is IPerkCard)
        {
            __result = TriggerAfterPurchase(__result, (IPerkCard)__state, __state);
        }
    }

    private static async Task<bool> TriggerAfterPurchase(Task<bool> original, IPerkCard perk, CardModel card)
    {
        bool success = await original;
        if (!success)
        {
            return false;
        }

        Player? owner = card.Owner;
        if (owner == null)
        {
            return true;
        }

        try
        {
            Log.Info($"[Aristocrat] 特典触发：{card.Id.Entry}");
            await perk.OnPerkTriggered(owner);
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 特典触发失败 {card.Id.Entry}：{ex}");
        }

        return true;
    }
}