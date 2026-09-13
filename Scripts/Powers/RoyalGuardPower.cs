using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 近卫队：每回合你手动打出的第一张固有牌会打出两次。
///
/// 塔2 的"打出两次"就是 CardModel.BaseReplayCount。做法是在 BeforeCardPlayed 里给这张牌加 1 次重放，
/// 等这一串打完了（AfterCardPlayedLate 且已经是最后一次）再还回去，避免影响后续回合。
/// </summary>
public sealed class RoyalGuardPower : PowerModel
{
    private bool _usedThisTurn;

    private CardModel? _boosted;

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

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;

        if (_usedThisTurn || cardPlay.IsAutoPlay || card.Owner?.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        if (!card.Keywords.Contains(CardKeyword.Innate))
        {
            return Task.CompletedTask;
        }

        _usedThisTurn = true;
        _boosted = card;
        card.BaseReplayCount += 1;
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_boosted != null && ReferenceEquals(cardPlay.Card, _boosted) && cardPlay.PlayIndex == cardPlay.PlayCount - 1)
        {
            _boosted.BaseReplayCount -= 1;
            _boosted = null;
        }

        return Task.CompletedTask;
    }
}
