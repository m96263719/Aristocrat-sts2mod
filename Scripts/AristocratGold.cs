using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Helpers;

namespace Aristocrat;

/// <summary>
/// 想要金币一变就有反应的能力实现这个接口（例：VanityPower）。触发点见 Patches/GoldChangedPatch。
/// </summary>
public interface IGoldChangedListener
{
    Task OnGoldChanged(Player player, int delta);
}

/// <summary>
/// 效果要花固定数额金币的牌实现这个接口。
/// 金币不足时会被 GoldCostPlayPatch 拦下来（当作"不能打出"），并弹一句"我的金币不足"。
/// </summary>
public interface IGoldCostCard
{
    /// <summary>这张牌需要多少金币。</summary>
    int GoldCost { get; }
}

/// <summary>贵族金币流的公共工具。</summary>
public static class AristocratGold
{
    /// <summary>
    /// 尽量花掉 amount 金币（不够就有多少花多少），返回真正花掉的数量。
    /// 对应塔1 的 AristocratHelper.spendGoldUpTo。
    /// </summary>
    public static async Task<int> SpendUpTo(int amount, Player player)
    {
        int spent = Math.Min(amount, player.Gold);
        if (spent > 0)
        {
            await PlayerCmd.LoseGold(spent, player, GoldLossType.Spent);
        }

        return spent;
    }

    /// <summary>
    /// 把"金币变了 delta"广播给在场的、关心金子的能力，并等它们做完。
    /// 这里必须等：塔2 的战斗流程是 await 链，扔一个"自己跑自己的"任务是接不进去的。
    /// </summary>
    public static async Task Notify(Player player, int delta)
    {
        if (player.Creature.CombatState == null)
        {
            return;
        }

        List<Task> tasks = [];
        foreach (IGoldChangedListener listener in player.Creature.Powers.OfType<IGoldChangedListener>())
        {
            tasks.Add(listener.OnGoldChanged(player, delta));
        }

        if (tasks.Count == 0)
        {
            return;
        }

        Log.Info($"[Aristocrat] 金币变动 {delta:+#;-#;0}，触发 {tasks.Count} 个效果。");
        await Task.WhenAll(tasks);
    }
}
