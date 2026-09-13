using Aristocrat.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 角色的攻击/施法/死亡音效路径是按 ID 拼出来的（不是虚属性，改不了），
/// 骨架阶段没有自己的语音资源，这里统一指向铁甲战士的，避免每次动作都报"找不到音频"。
/// 等以后有了自己的音效，把这里删掉即可。
/// </summary>
[HarmonyPatch]
internal static class CharacterSfxPatch
{
    private static bool IsOurs(CharacterModel model) => model is global::Aristocrat.Character.Aristocrat;

    [HarmonyPatch(typeof(CharacterModel), "get_AttackSfx")]
    [HarmonyPostfix]
    private static void AttackSfxPostfix(CharacterModel __instance, ref string __result)
    {
        if (IsOurs(__instance)) __result = "event:/sfx/characters/ironclad/ironclad_attack";
    }

    [HarmonyPatch(typeof(CharacterModel), "get_CastSfx")]
    [HarmonyPostfix]
    private static void CastSfxPostfix(CharacterModel __instance, ref string __result)
    {
        if (IsOurs(__instance)) __result = "event:/sfx/characters/ironclad/ironclad_cast";
    }

    [HarmonyPatch(typeof(CharacterModel), "get_DeathSfx")]
    [HarmonyPostfix]
    private static void DeathSfxPostfix(CharacterModel __instance, ref string __result)
    {
        if (IsOurs(__instance)) __result = "event:/sfx/characters/ironclad/ironclad_die";
    }
}