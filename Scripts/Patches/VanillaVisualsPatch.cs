using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 骨架阶段把贵族的战斗相关场景指向铁甲战士的原版场景。
///
/// 之前我是把铁甲战士的 .tscn 复制一份改名，但那种拷贝出来的场景带着原资源的 UID，
/// 在实际运行环境里解析不稳（日志里能看到 "invalid UID" 的告警），
/// 表现就是战斗一片漆黑。改成直接复用原版路径，绕开所有拷贝带来的问题。
///
/// 等贵族自己的 Spine 动画做好之后，把这个补丁删掉，换成 res://scenes/... 里的自有资源即可。
/// </summary>
[HarmonyPatch]
internal static class VanillaVisualsPatch
{
    private const string Source = "ironclad";

    private static bool IsOurs(CharacterModel model) => model is global::Aristocrat.Character.Aristocrat;

    [HarmonyPatch(typeof(CharacterModel), "get_VisualsPath")]
    [HarmonyPostfix]
    private static void VisualsPostfix(CharacterModel __instance, ref string __result)
    {
        if (IsOurs(__instance)) __result = $"res://scenes/creature_visuals/{Source}.tscn";
    }

    [HarmonyPatch(typeof(CharacterModel), "get_EnergyCounterPath")]
    [HarmonyPostfix]
    private static void EnergyCounterPostfix(CharacterModel __instance, ref string __result)
    {
        if (IsOurs(__instance)) __result = $"res://scenes/combat/energy_counters/{Source}_energy_counter.tscn";
    }

    [HarmonyPatch(typeof(CharacterModel), "get_MerchantAnimPath")]
    [HarmonyPostfix]
    private static void MerchantPostfix(CharacterModel __instance, ref string __result)
    {
        if (IsOurs(__instance)) __result = $"res://scenes/merchant/characters/{Source}_merchant.tscn";
    }

    [HarmonyPatch(typeof(CharacterModel), "get_RestSiteAnimPath")]
    [HarmonyPostfix]
    private static void RestSitePostfix(CharacterModel __instance, ref string __result)
    {
        if (IsOurs(__instance)) __result = $"res://scenes/rest_site/characters/{Source}_rest_site.tscn";
    }

    [HarmonyPatch(typeof(CharacterModel), "get_TrailPath")]
    [HarmonyPostfix]
    private static void TrailPostfix(CharacterModel __instance, ref string __result)
    {
        if (IsOurs(__instance)) __result = $"res://scenes/vfx/card_trail_{Source}.tscn";
    }
}