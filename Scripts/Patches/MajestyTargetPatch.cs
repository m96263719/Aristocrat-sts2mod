using Aristocrat.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 威严生效期间，把贵族的打击改成"以所有敌人为目标"。
/// 做法和「升级：点穴」那个符文一样——接管 CardModel.TargetType 的取值。
/// </summary>
[HarmonyPatch(typeof(CardModel), "get_TargetType")]
internal static class MajestyTargetPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref TargetType __result)
    {
        if (__instance is not global::Aristocrat.Cards.AristocratStrike)
        {
            return;
        }

        Player? owner = __instance.Owner;
        if (owner != null && owner.Creature.GetPowerAmount<MajestyPower>() > 0)
        {
            __result = TargetType.AllEnemies;
        }
    }
}