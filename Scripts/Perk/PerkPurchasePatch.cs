using System;
using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Perk;

/// <summary>
/// 商店购买钩子：买下的牌如果带特典（或玩家有「宝库钥匙」而这是一张稀有牌），
/// 就在购买真正完成之后触发对应效果。
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
        if (__state == null)
        {
            return;
        }

        // 稀有牌要顺带检查宝库钥匙，所以即使没有特典也得接一段
        if (__state is not IPerkCard && __state.Rarity != CardRarity.Rare)
        {
            return;
        }

        __result = TriggerAfterPurchase(__result, __state);
    }

    private static async Task<bool> TriggerAfterPurchase(Task<bool> original, CardModel card)
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
            // 保险：商店摆牌时已经打过标记了（蛋系克隆牌也会继承），这里再兜一次，
            // 保证"买下的这张牌"一定算特典牌（后面天生支配者之类再触发时也认它）
            if (owner.Relics.OfType<VaultKey>().Any())
            {
                VaultKey.MarkShopCard(card);
            }

            Log.Info($"[Aristocrat] 特典触发：{card.Id.Entry}");
            await PerkSystem.TriggerPerk(card, owner);
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 特典触发失败 {card.Id.Entry}：{ex}");
        }

        return true;
    }
}
