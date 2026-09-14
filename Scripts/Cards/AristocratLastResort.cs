using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Utils;

namespace Aristocrat.Cards;

/// <summary>
/// 最后手段（固有）：造成 7 伤 3 次。每回合结束时只要它还留在手里（被保留也算），
/// 本场战斗中它的费用就降低 1；被丢弃时同样降 1。升级 伤害 +2。
///
/// 塔1 是 triggerOnManualDiscard 加 triggerOnEndOfTurnForPlayingCard 两个入口，都是 updateCost(-1)。
/// 塔2 对应 AfterCardDiscarded 和 BeforeSideTurnEnd，费用用 EnergyCost.SetThisCombat 直接写，
/// 这样和别的费用修正互不干扰。
/// </summary>
public sealed class AristocratLastResort : AristocratCard
{
    private int _discardCount;

    public AristocratLastResort()
        : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("MagicNumber", 3m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hits = (int)DynamicVars["MagicNumber"].BaseValue;
        for (int i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompatibility(this, cardPlay)
                .Targeting(cardPlay.Target!)
                .Execute(choiceContext);
        }
    }

    public override Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (ReferenceEquals(card, this))
        {
            Reduce();
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 回合结束时还在手里（没打出去、被保留也算）就降费
        if (side == CombatSide.Player && Pile?.Type == PileType.Hand)
        {
            Reduce();
        }

        return Task.CompletedTask;
    }

    private void Reduce()
    {
        int current = CanonicalEnergyCost - _discardCount;
        if (current <= 0)
        {
            return;
        }

        _discardCount++;
        EnergyCost.SetThisCombat(CanonicalEnergyCost - _discardCount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
