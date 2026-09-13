using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 天选意识（固有）：每当你使用一张固有牌，就获得 Amount 金币。
/// 塔1 用的是 onAfterUseCard 配合 isInnate；塔2 对应 AfterCardPlayed 配合 CardKeyword.Innate。
/// </summary>
public sealed class BlueBloodedPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;
        if (card.Owner?.Creature != Owner || !card.Keywords.Contains(CardKeyword.Innate))
        {
            return;
        }

        Player? player = Owner.Player;
        if (player == null)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainGold(Amount, player);
    }
}
