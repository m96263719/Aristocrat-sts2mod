using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Powers;

/// <summary>
/// 体面：每当你使用打击时获得 Amount 点格挡；每当你使用防御时对所有敌人造成 Amount 点伤害。
/// 塔1 用的是 AbstractPower.onAfterUseCard，塔2 对应 AbstractModel.AfterCardPlayed。
/// </summary>
public sealed class DignityPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;
        if (card.Owner?.Creature != Owner)
        {
            return;
        }

        if (AristocratCard.IsStrike(card))
        {
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
        }
        else if (AristocratCard.IsDefend(card))
        {
            await DamageCmd.Attack(Amount)
                .FromCard(card)
                .TargetingAllOpponents(Owner.CombatState!)
                .Unpowered()
                .Execute(choiceContext);
        }
    }
}