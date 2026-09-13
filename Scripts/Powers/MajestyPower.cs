using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 威严：本回合内，打击以所有敌人为目标。回合结束时自动移除。
/// 具体表现由 MajestyTargetPatch（改目标类型）和 AristocratStrike（改成打全体）配合实现。
/// </summary>
public sealed class MajestyPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Any(creature => creature == Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}