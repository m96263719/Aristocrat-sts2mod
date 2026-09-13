using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 亿万富翁：每回合开始时获得 Amount 金币，然后 Amount 减 1（减到 0 以后就只是占位，不再发钱）。
/// 塔1 是 atStartOfTurn 再自己减一；塔2 用 AfterPlayerTurnStart 加 PowerCmd.Decrement。
/// </summary>
public sealed class BillionairePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainGold(Amount, player);
        await PowerCmd.Decrement(this);
    }
}
