using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Utils;

namespace Aristocrat.Cards;

/// <summary>
/// 为打击而打击（2 费，攻击，普通）：造成 10 点伤害。本回合只要你打出过打击，这张牌费用变为 0。
/// 升级 +4。特典：将牌组中的 1 张打击变化为究极打击。对应塔1 的 aristocrat:StrikeForStrike。
/// </summary>
public sealed class AristocratStrikeForStrike : AristocratCard, IPerkCard
{
    public AristocratStrikeForStrike()
        : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [AristocratKeywords.Perk];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        RefreshCost();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        RefreshCost();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        RefreshCost();
        return Task.CompletedTask;
    }

    private void RefreshCost()
    {
        if (Owner == null)
        {
            return;
        }

        ICombatState? combat = Owner.Creature.CombatState;
        if (combat == null)
        {
            return;
        }

        bool usedStrike = CombatManager.Instance.History.CardPlaysFinished
            .Where(entry => entry.HappenedThisTurn(combat))
            .Any(entry => IsStrike(entry.CardPlay.Card));

        if (usedStrike)
        {
            EnergyCost.SetThisTurn(0, reduceOnly: true);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }

    public async Task OnPerkTriggered(Player owner)
    {
        // 优先挑普通打击：已经是究极打击的牌不要再被选中
        await PerkHelpers.TransformFirstInDeck<UltimateStrike>(owner, card => IsStrike(card) && card is not UltimateStrike);
    }
}