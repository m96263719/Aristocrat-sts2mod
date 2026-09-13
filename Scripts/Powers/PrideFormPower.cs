using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 傲慢形态：每回合结束时，从弃牌堆和抽牌堆里各随机打出一张固有牌（层数就是次数）。
/// 塔1 用一个自制的 PlayRandomInnateFromPilesAction；塔2 直接"搬进手里再 AutoPlay"即可。
///
/// 放在 BeforeSideTurnEnd 而不是 After：塔2 的注释明确说会伤害敌人的效果要写在 Before 里，
/// 否则结算时战斗可能已经开始收尾。
/// </summary>
public sealed class PrideFormPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
        {
            return;
        }

        Player? player = Owner.Player;
        if (player == null)
        {
            return;
        }

        for (int i = 0; i < Amount; i++)
        {
            Flash();
            await PlayRandomInnate(choiceContext, player, PileType.Discard);
            await PlayRandomInnate(choiceContext, player, PileType.Draw);
        }
    }

    private static async Task PlayRandomInnate(PlayerChoiceContext choiceContext, Player player, PileType pileType)
    {
        List<CardModel> candidates = pileType.GetPile(player).Cards
            .Where(card => card.Keywords.Contains(CardKeyword.Innate))
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        CardModel card = candidates[Random.Shared.Next(candidates.Count)];
        await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top, null, false);

        Creature? target = card.TargetType == TargetType.AnyEnemy ? AristocratCard.PickRandomEnemy(player) : null;
        await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default);
    }
}
