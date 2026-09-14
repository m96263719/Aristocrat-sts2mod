using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Compat;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Powers;

/// <summary>
/// 虚荣：每当你获得或失去金币时，对所有敌人造成 Amount 点伤害。
///
/// 塔2 只在获得金币时给了钩子，而且那个钩子只发给牌组里的牌、遗物、药水，不发给战斗中的能力；
/// 失去金币则完全没有钩子。所以这里自己盯着 Player.Gold 的变化，见 Patches/GoldChangedPatch。
/// 伤害用 ThrowingPlayerChoiceContext 直接结算，原版的能力（例如 Sleight of Flesh）也是这么做的。
/// </summary>
public sealed class VanityPower : PowerModel, IGoldChangedListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public Task OnGoldChanged(Player player, int delta)
    {
        if (Amount <= 0 || player.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        ICombatState? combatState = Owner.CombatState;
        IReadOnlyList<Creature> enemies = combatState?.HittableEnemies ?? [];
        if (enemies.Count == 0)
        {
            return Task.CompletedTask;
        }

        Flash();
        // 走兼容层：正式版 / 测试版的 CreatureCmd.Damage 参数个数不一样
        return BetaCompat.Damage(new ThrowingPlayerChoiceContext(), enemies, Amount, ValueProp.Unpowered, Owner, null);
    }
}
