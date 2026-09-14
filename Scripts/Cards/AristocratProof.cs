using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Utils;

namespace Aristocrat.Cards;

/// <summary>
/// 证明（1 费，攻击，罕见，固有）：造成 5 点伤害，手中每有 1 张其他固有牌，伤害再加 3。升级 +2 伤害、每张 +2。
/// 对应塔1 的 aristocrat:Proof。
/// 注意：加成是在打出的瞬间算的，卡面预览里不会提前显示那部分（塔2 没有塔1 那种 applyPowers 重算入口）。
/// </summary>
public sealed class AristocratProof : AristocratCard
{
    public AristocratProof()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new DynamicVar("MagicNumber", 3m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        int others = PileType.Hand.GetPile(owner).Cards
            .Count(card => !ReferenceEquals(card, this) && card.Keywords.Contains(CardKeyword.Innate));

        int damage = (int)DynamicVars.Damage.BaseValue + (int)DynamicVars["MagicNumber"].BaseValue * others;

        await DamageCmd.Attack(damage)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["MagicNumber"].UpgradeValueBy(2);
    }
}
