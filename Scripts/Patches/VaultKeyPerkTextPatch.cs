using System.Reflection;
using Aristocrat.Perk;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 宝库钥匙：让卡面上真的写出这条特典的效果文字。
///
/// 塔2 的卡面文字是 `CardModel.GetDescriptionForPile` 拼出来的（本体描述 + 附魔/苦痛 + 关键词行），
/// 关键词行只有"[gold]特典[/gold]。"这一小截（`CardKeywordExtensions.GetCardText`），
/// 所以我们自己往描述后面补一行"[gold]特典[/gold]：获得一件随机遗物。"，
/// 和我们那些自带特典的卡（卡文里就写了"特典：……"）排版一致。
///
/// 挂在私有重载 `GetDescriptionForPile(PileType, DescriptionPreviewType, Creature)` 上：
/// 公开的那个重载和升级预览最后都会走到它，一处补全所有场合
/// （商店里、牌组里、手里、战斗里、卡牌总览、升级预览）。
///
/// 判定是"稀有牌 + 主人有宝库钥匙"，所以买下之后牌组里照样显示、存读档也不会掉——
/// 这条特典本来就该由"卡稀有 + 你有钥匙"推出来，不需要存任何标记。
/// </summary>
[HarmonyPatch]
internal static class VaultKeyPerkTextPatch
{
    private static MethodBase? TargetMethod()
    {
        // DescriptionPreviewType 是 CardModel 的私有嵌套枚举，只能这样取
        return AccessTools.Method(typeof(CardModel), "GetDescriptionForPile", new[]
        {
            typeof(PileType),
            AccessTools.Inner(typeof(CardModel), "DescriptionPreviewType"),
            typeof(Creature),
        });
    }

    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref string __result)
    {
        if (!PerkSystem.HasVaultKeyPerk(__instance))
        {
            return;
        }

        string? line = PerkSystem.VaultKeyPerkLine();
        if (!string.IsNullOrEmpty(line))
        {
            __result = __result + "\n" + line;
        }
    }
}
