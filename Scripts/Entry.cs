using System.Reflection;
using Aristocrat.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Aristocrat;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public const string ModId = "Aristocrat";

    public const string HarmonyId = "Aristocrat.Core";

    public static void Init()
    {
        InstallPatches();

        try
        {
            // 遗物必须属于某个池子。否则游戏在选人、算描述时查 RelicModel.Pool
            // 会抛 "Sequence contains no matching element"，表现就是"角色选了但没选上"。
            ModHelper.AddModelToPool<SharedRelicPool, FamilyCrest>();
            ModHelper.AddModelToPool<SharedRelicPool, FamilyHonor>();
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 遗物注册失败: {ex}");
        }

        Log.Info("[Aristocrat] 贵族加载完成。");
    }

    /// <summary>
    /// 逐个补丁单独安装：我们的补丁越来越多，其中一个目标方法改名或签名变化时，
    /// 不应该连带把其他补丁（尤其是角色注册）一起拖下水。
    /// </summary>
    private static void InstallPatches()
    {
        var harmony = new Harmony(HarmonyId);
        int ok = 0;
        int failed = 0;

        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.GetCustomAttribute<HarmonyPatch>() == null)
            {
                continue;
            }

            try
            {
                new PatchClassProcessor(harmony, type).Patch();
                ok++;
            }
            catch (Exception ex)
            {
                failed++;
                Log.Error($"[Aristocrat] 补丁安装失败 {type.Name}: {ex.GetBaseException().Message}");
            }
        }

        Log.Info($"[Aristocrat] 补丁安装完成：成功 {ok} 个，失败 {failed} 个。");
    }
}