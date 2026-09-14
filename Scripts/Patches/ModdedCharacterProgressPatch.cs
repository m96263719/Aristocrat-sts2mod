using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace Aristocrat.Patches;

/// <summary>
/// 修本体对 mod 角色的一个硬编码缺陷。
///
/// ProgressSaveManager 里有两个"纪元"统计方法（击败 15 个精英 / 15 个首领），
/// 它们是这么写的：
///
///     if (!(character is Ironclad)) if (!(character is Silent)) ... if (!(character is Deprived))
///         throw new ArgumentOutOfRangeException("character", character, null);
///
/// 只认识本体那 6 个角色。**任何 mod 角色打赢精英或首领都会在这里抛异常**，
/// 而这段代码是在战斗结算流程里调用的，异常会直接打断结算——
/// 表现就是"打完精英就卡住"。这是本体的问题，不是某个 mod 的错。
///
/// 处理办法：如果是 mod 角色（不在游戏本体程序集里），直接跳过这套纪元统计。
/// mod 角色本来也没有对应的纪元内容。
/// </summary>
[HarmonyPatch]
internal static class ModdedCharacterProgressPatch
{
    [HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch")]
    [HarmonyPrefix]
    private static bool ElitesPrefix(Player localPlayer) => IsVanillaCharacter(localPlayer);

    [HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenBossesDefeatedEpoch")]
    [HarmonyPrefix]
    private static bool BossesPrefix(Player localPlayer) => IsVanillaCharacter(localPlayer);

    /// <summary>
    /// 首领战结算时本体还会调这个方法：它按"角色 ID 加章节号"去查纪元，
    /// 比如铁甲战士是 IRONCLAD2_EPOCH / IRONCLAD3_EPOCH……mod 角色没有这套纪元，
    /// EpochModel.Get 会直接抛 ArgumentException，把结算流程打断——表现就是打完首领卡住、
    /// 存读档后才能弹出奖励。和上面两个方法一样，mod 角色直接跳过。
    /// </summary>
    [HarmonyPatch(typeof(ProgressSaveManager), "ObtainCharUnlockEpoch")]
    [HarmonyPrefix]
    private static bool CharUnlockPrefix(Player localPlayer) => IsVanillaCharacter(localPlayer);

    private static bool IsVanillaCharacter(Player localPlayer)
    {
        CharacterModel character = localPlayer.Character;
        bool vanilla = character.GetType().Assembly == typeof(CharacterModel).Assembly;

        if (!vanilla)
        {
            Log.Info($"[Aristocrat] 跳过本体纪元统计（mod 角色 {character.Id.Entry} 不在本体角色表里）。");
        }

        return vanilla;
    }
}
