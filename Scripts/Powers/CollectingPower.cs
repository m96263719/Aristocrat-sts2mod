using System.Collections.Generic;
using System.Threading.Tasks;
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
/// 追缴（挂在敌人身上）：本回合中，你每对这个敌人造成一次受加成的攻击伤害，就获得 Amount 金币。
/// 塔1 用 onAttacked 加上 atEndOfRound（一轮结束才移除，也就是敌人回合也还算数）；
/// 塔2 对应 AfterDamageReceived 与 AfterSideTurnEnd(敌人侧)。
/// </summary>
public sealed class CollectingPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || result.UnblockedDamage <= 0)
        {
            return;
        }

        // 塔1 判定的是玩家打出的普通攻击（DamageType.NORMAL）；不吃力量加成的伤害（例如荆棘）不算。
        if (dealer == null || !dealer.IsPlayer || !props.IsPoweredAttack())
        {
            return;
        }

        Player? player = dealer.Player;
        if (player == null)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainGold(Amount, player);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.Remove(this);
        }
    }
}
