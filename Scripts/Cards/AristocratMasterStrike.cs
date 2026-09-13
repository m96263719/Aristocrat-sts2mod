using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 大师一击：造成 6 伤；如果目标意图攻击，本回合把它的力量压制为 0。升级 伤害 +3。
///
/// 塔1 是先加负力量、再补一个回合末还回去的能力。塔2 原生就有干这事的
/// （暗影镣铐的 DarkShacklesPower，继承自 TemporaryStrengthPower 且 IsPositive=false），直接复用最省事。
/// </summary>
public sealed class AristocratMasterStrike : AristocratCard
{
    public AristocratMasterStrike()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        Creature? target = cardPlay.Target;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target!)
            .Execute(choiceContext);

        if (target?.Monster == null || !target.Monster.IntendsToAttack)
        {
            return;
        }

        int strength = target.GetPowerAmount<StrengthPower>();
        if (strength > 0)
        {
            await PowerCmd.Apply<DarkShacklesPower>(choiceContext, target, strength, owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
