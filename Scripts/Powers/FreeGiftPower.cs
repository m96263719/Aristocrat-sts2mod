using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>赠品：每当你使用一张有特典的牌，抽 Amount 张牌。</summary>
public sealed class FreeGiftPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card is not IPerkCard)
        {
            return;
        }

        Player? player = Owner.Player;
        if (player == null)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, player);
    }
}
