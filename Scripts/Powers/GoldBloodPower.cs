using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Powers;

/// <summary>
/// 黄金之血：每当你受到未被格挡的伤害时，获得 Amount 金币。
/// 塔1 用 onAttacked 每次命中结算一次；塔2 对应 AfterDamageReceived，时机一样。
/// </summary>
public sealed class GoldBloodPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

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

        Player? player = Owner.Player;
        if (player == null)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainGold(Amount, player);
    }
}
