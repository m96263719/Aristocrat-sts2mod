using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 祖训：本场战斗中，你的固有牌费用减 1。
///
/// 塔2 原生的按卡改费用钩子（TryModifyEnergyCostInCombat），
/// 比塔1 那种手动遍历手牌改 costForTurn 的做法干净得多，而且抽到的牌自动生效。
/// </summary>
public sealed class AncestralPreceptsPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Owner?.Creature != Owner || originalCost <= 0m)
        {
            return false;
        }

        if (!card.Keywords.Contains(CardKeyword.Innate))
        {
            return false;
        }

        modifiedCost = Math.Max(0m, originalCost - 1m);
        return true;
    }
}