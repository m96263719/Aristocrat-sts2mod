using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Powers;

/// <summary>
/// 黄金盔甲（延迟一层）：下回合开始时，获得 Amount 点格挡和 Amount 金币，然后自己消失。
/// Amount 在打出时锁定为当时的格挡值，所以之后格挡再怎么变都不影响。
/// </summary>
public sealed class GoldenArmorNextTurnPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
        await PlayerCmd.GainGold(Amount, player);
        await PowerCmd.Remove(this);
    }
}
