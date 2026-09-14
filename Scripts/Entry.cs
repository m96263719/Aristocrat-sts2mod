using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Aristocrat;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public const string ModId = "Aristocrat";

    public const string HarmonyId = "Aristocrat.Core";

    public static void Init()
    {
        // 提前把两个存档字段构造出来：BaseLib 的 SavedSpireField 在构造时就登记自己，
        // 之后它会把这个名字注册进存档的 net-id 表（联机同步 / 战斗回放要用，不注册会抛异常）。
        Perk.PersistentKeywordTracker.EnsureRegistered();
        Perk.VaultKeyPerkTracker.EnsureRegistered();

        InstallPatches();

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
