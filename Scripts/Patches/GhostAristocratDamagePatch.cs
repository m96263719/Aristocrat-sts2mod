using System.Reflection;
using Aristocrat.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 幽灵贵族的 2 倍伤害挂补丁，而不是 override。
///
/// 测试版（0.108 起）给 `AbstractModel.ModifyDamageMultiplicative` 末尾加了一个 `CardPlay?`，
/// 正式版没有 —— `override` 只能对上其中一个签名，另一个分支上这个方法会挂不上基类
/// （0.111 上就表现为效果失效甚至类型加载失败）。补丁是按参数名匹配的，少写几个参数没关系，
/// 所以同一份代码在正式版 / 测试版 都能正常生效。
///
/// 格挡那边（`ModifyBlockMultiplicative`）两个分支签名一样，仍然用 override。
/// </summary>
[HarmonyPatch(typeof(AbstractModel), "ModifyDamageMultiplicative")]
internal static class GhostAristocratDamagePatch
{
    // 只声明两个分支都有的参数：CardPlay 那个新参数不写就行。
    private static void Postfix(AbstractModel __instance, CardModel? cardSource, ref decimal __result)
    {
        if (__instance is GhostAristocratPower power && power.Doubles(cardSource))
        {
            __result *= 2m;
        }
    }
}
