using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>你打出的下一张特典牌会额外触发一次它的特典。对应塔1 的 NextPerkTriggersPower。</summary>
public sealed class NextPerkTriggersPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Amount <= 0 || cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        if (cardPlay.Card is not IPerkCard perk)
        {
            return;
        }

        Player? player = Owner.Player;
        if (player == null)
        {
            return;
        }

        Flash();
        await perk.OnPerkTriggered(player);

        await PowerCmd.Decrement(this);
        if (Amount <= 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}
