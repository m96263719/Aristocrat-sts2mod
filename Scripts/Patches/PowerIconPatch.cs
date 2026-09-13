using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 给 mod 自己的能力指定图标。
///
/// 塔2 的能力图标默认走"图集"路径（res://images/atlases/power_atlas.sprites/xxx.tres），
/// 那个图集是游戏本体打包时生成的，mod 加不进去。
/// 所以通用做法是接管取路径的属性，把它指到我们自己的 png 上。
/// 观者拓展包用的也是这个思路（它直接接管了 Icon 和 BigIcon 的取值函数）。
/// </summary>
[HarmonyPatch]
internal static class PowerIconPatch
{
    private const string IconFolder = "res://Aristocrat/images/powers/";

    private static bool TryGetOurIcon(PowerModel power, out string path)
    {
        if (power.GetType().Assembly == typeof(PowerIconPatch).Assembly)
        {
            path = IconFolder + power.GetType().Name + ".png";
            return true;
        }

        path = string.Empty;
        return false;
    }

    [HarmonyPatch(typeof(PowerModel), "get_PackedIconPath")]
    [HarmonyPostfix]
    private static void PackedIconPathPostfix(PowerModel __instance, ref string __result)
    {
        if (TryGetOurIcon(__instance, out string path))
        {
            __result = path;
        }
    }

    [HarmonyPatch(typeof(PowerModel), "get_ResolvedBigIconPath")]
    [HarmonyPostfix]
    private static void ResolvedBigIconPathPostfix(PowerModel __instance, ref string __result)
    {
        if (TryGetOurIcon(__instance, out string path))
        {
            __result = path;
        }
    }
}