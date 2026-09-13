using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 特权：每回合你打出的第一张固有牌费用变为 0。
///
/// 塔1 是回合开始把手里的固有牌 costForTurn 改成 0、打出第一张后再还原那套做法。
/// 塔2 有原生的按卡改费用钩子 TryModifyEnergyCostInCombat，只要记住本回合用没用掉就行，
/// 之后抽到的牌也自动生效。
/// </summary>
public sealed class PrivilegePower : PowerModel
{
    private bool _usedThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            _usedThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;
        if (card.Owner?.Creature == Owner && card.Keywords.Contains(CardKeyword.Innate))
        {
            _usedThisTurn = true;
        }

        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (_usedThisTurn || originalCost <= 0m || card.Owner?.Creature != Owner)
        {
            return false;
        }

        if (!card.Keywords.Contains(CardKeyword.Innate))
        {
            return false;
        }

        modifiedCost = 0m;
        return true;
    }
}
