using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 光辉（0 费，攻击，普通）：造成 4 点伤害，抽 2 张牌，消耗。升级 +2。
/// 特典：获得 1 张这张牌的复制。对应塔1 的 aristocrat:Radiance。
/// </summary>
public sealed class AristocratRadiance : AristocratCard, IPerkCard
{
    public AristocratRadiance()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, AristocratKeywords.Perk];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        await CardPileCmd.Draw(choiceContext, 2, Owner!);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }

    public async Task OnPerkTriggered(Player owner)
    {
        // 买的这张是升级版，复制出来的也是升级版
        await PerkHelpers.AddCardToDeck<AristocratRadiance>(owner, IsUpgraded);
    }
}