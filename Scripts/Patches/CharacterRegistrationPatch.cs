using System.Collections.Generic;
using System.Linq;
using Aristocrat.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Patches;

/// <summary>
/// 把贵族注册进角色列表。
///
/// 塔2 的 ModelDb.AllCharacters 是写死的五个人物数组，mod 角色必须打补丁把自己加进去，
/// 观者 mod 用的也是这个办法。
/// </summary>
[HarmonyPatch(typeof(ModelDb), "get_AllCharacters")]
internal static class CharacterRegistrationPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref IEnumerable<CharacterModel> __result)
    {
        CharacterModel? ours = ModelDb.GetByIdOrNull<CharacterModel>(
            ModelDb.GetId(typeof(global::Aristocrat.Character.Aristocrat)));

        if (ours != null)
        {
            __result = __result.Concat([ours]).Distinct().ToArray();
        }
    }
}