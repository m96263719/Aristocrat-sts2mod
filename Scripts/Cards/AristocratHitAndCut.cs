using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Utils;

namespace Aristocrat.Cards;

/// <summary>
/// 击切（1 费，攻击，普通）：造成 7 点伤害，然后从抽牌堆抽 1 张防御。升级 +3。
/// 特典：将牌组中的 1 张防御变化为究极防御。对应塔1 的 aristocrat:HitAndCut。
/// </summary>
public sealed class AristocratHitAndCut : AristocratCard, IPerkCard
{
    public AristocratHitAndCut()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [AristocratKeywords.Perk];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        await DrawMatchingFromDrawPile(choiceContext, owner, 1, IsDefend);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    public async Task OnPerkTriggered(Player owner)
    {
        // 优先挑普通防御：已经是究极防御的牌不要再被选中
        await PerkHelpers.TransformFirstInDeck<UltimateDefend>(owner, card => IsDefend(card) && card is not UltimateDefend);
    }
}