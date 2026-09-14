using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 侍从！：你下一张打出的打击或防御费用变为 0。对应塔1 的 NextStrikeDefendFreePower。
///
/// 塔1 是自己记住每张牌原本的费用、打完再手动还原（onAfterUseCard 里 amount--，用完就移除）。
/// 塔2 有现成的费用钩子（本体的「腐化」也是这么写"技能费用为 0"的），
/// 所以只需要：层数 > 0 时把打击/防御的费用改成 0，打出一次就减一层，减完移除——
/// 钩子一撤费用自然就恢复了，不用自己记原值。
/// </summary>
public sealed class AttendantPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (Amount <= 0 || card.Owner?.Creature != Owner || !AristocratCard.IsStrikeOrDefend(card))
        {
            return false;
        }

        modifiedCost = 0m;
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Amount <= 0 || cardPlay.Card.Owner?.Creature != Owner || !AristocratCard.IsStrikeOrDefend(cardPlay.Card))
        {
            return;
        }

        await PowerCmd.Decrement(this);
        if (Amount <= 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}
