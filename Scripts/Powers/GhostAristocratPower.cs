using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Powers;

/// <summary>
/// 幽灵贵族：本回合内，你使用的打击与防御造成 2 倍伤害、获得 2 倍格挡。
/// 塔1 是直接改伤害数值，塔2 有现成的 ModifyDamageMultiplicative / ModifyBlockMultiplicative 钩子。
/// </summary>
public sealed class GhostAristocratPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return IsOurs(cardSource) ? 2m : 1m;
    }

    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        return IsOurs(cardSource) ? 2m : 1m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Any(creature => creature == Owner))
        {
            await PowerCmd.Remove(this);
        }
    }

    private bool IsOurs(CardModel? card)
    {
        return card != null && card.Owner?.Creature == Owner && AristocratCard.IsStrikeOrDefend(card);
    }
}