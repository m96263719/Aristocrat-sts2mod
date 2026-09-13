using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>幽灵贵族（下回合生效）：到你的下个回合开始时，换成真正的双倍效果并移除自己。</summary>
public sealed class GhostAristocratNextTurnPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        await PowerCmd.Remove(this);
        await PowerCmd.Apply<GhostAristocratPower>(choiceContext, Owner, 1m, Owner, null);
    }
}