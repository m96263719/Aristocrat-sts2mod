using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Compat;

/// <summary>
/// 正式版（Steam 分支 public）/ 测试版（public-beta）双分支兼容层。
///
/// 塔2 的两个分支偶尔会改同一个方法的签名。C# 是编译期绑定的，直接写调用的话，
/// 在另一个分支上会直接 `MissingMethodException`；`override` 更狠，签名对不上时
/// 那份方法根本挂不上去（轻则效果失效，重则类型加载失败）。
/// 所以凡是"两个分支签名不一样"的地方，都从这个口子绕过去（反射按名字找方法）。
///
/// 这套思路和 BaseLib 的 `BaseLib.Utils.BetaMainCompatibility` 一样：
/// 能用 BaseLib 现成的兼容扩展就用它的（例如 `AttackCommand.FromCardCompatibility`），
/// 它没覆盖的（`CardCmd.Exhaust` 这种）就在这儿自己补。
///
/// 已确认的差异（正式版 0.107.1 → 测试版 0.111；签名是从工坊里**按测试版编译的 mod**
/// 反编译出来的，不是猜的）：
///   • `CardCmd.Exhaust`              返回类型 `Task` → `Task&lt;CardPileAddResult?&gt;`
///   • `CreatureCmd.Damage`（带 dealer/cardSource 那组）末尾多一个 `CardPlay?` 参数
///   • `AttackCommand.FromCard`       多一个 `CardPlay?` 参数 → 用 BaseLib 的兼容扩展
///   • `AbstractModel.ModifyDamageMultiplicative` 多一个 `CardPlay?` 参数 → 用补丁而不是 override
/// </summary>
internal static class BetaCompat
{
    private static readonly MethodInfo? ExhaustMethod = AccessTools.Method(
        typeof(CardCmd),
        "Exhaust",
        [typeof(PlayerChoiceContext), typeof(CardModel), typeof(bool), typeof(bool)]);

    private static readonly MethodInfo? DamageMethod = ResolveDamageMethod();

    private static MethodInfo? ResolveDamageMethod()
    {
        // 两个分支的参数只差最后那个 CardPlay?，所以先按测试版的 7 参数找，
        // 找不到就退回正式版的 6 参数。
        Type[] betaSignature =
        [
            typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal),
            typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay)
        ];

        return AccessTools.Method(typeof(CreatureCmd), "Damage", betaSignature)
            ?? AccessTools.Method(typeof(CreatureCmd), "Damage", betaSignature[..6]);
    }

    /// <summary>
    /// 消耗一张牌。正式版返回 `Task`、测试版返回 `Task&lt;CardPileAddResult?&gt;`，
    /// 反射调用完直接当成 `Task` 等，两边的表现都一样。
    /// </summary>
    public static async Task Exhaust(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal = false,
        bool skipVisuals = false)
    {
        if (ExhaustMethod is null)
        {
            Log.Error("[Aristocrat] 找不到 CardCmd.Exhaust，这次消耗被跳过。");
            return;
        }

        try
        {
            object? result = ExhaustMethod.Invoke(null, [choiceContext, card, causedByEthereal, skipVisuals]);
            if (result is Task task)
            {
                await task;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 消耗卡牌失败：{ex.GetBaseException().Message}");
        }
    }

    /// <summary>
    /// 对一组敌人直接结算伤害（不经过卡牌）。测试版多一个 `CardPlay?`，
    /// 我们这边没有卡牌上下文，就传 null。
    /// </summary>
    public static Task Damage(
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature> targets,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay = null)
    {
        if (DamageMethod is null)
        {
            Log.Error("[Aristocrat] 找不到 CreatureCmd.Damage，这次伤害被跳过。");
            return Task.CompletedTask;
        }

        object?[] args = DamageMethod.GetParameters().Length >= 7
            ? [choiceContext, targets, amount, props, dealer, cardSource, cardPlay]
            : [choiceContext, targets, amount, props, dealer, cardSource];

        try
        {
            return (Task)DamageMethod.Invoke(null, args)!;
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 结算伤害失败：{ex.GetBaseException().Message}");
            return Task.CompletedTask;
        }
    }
}
