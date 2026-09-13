using System.Collections.Generic;
using System.Linq;
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
/// 贱民！（1 费，攻击，普通，固有）：对所有敌人造成 4 点伤害并给予 1 层虚弱。升级伤害 +2。
/// 对应塔1 的 aristocrat:Peasants。
/// </summary>
public sealed class AristocratPeasants : AristocratCard
{
    public AristocratPeasants()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new DynamicVar("MagicNumber", 1m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(owner.Creature.CombatState!)
            .Execute(choiceContext);

        foreach (Creature enemy in owner.Creature.CombatState!.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                enemy,
                DynamicVars["MagicNumber"].BaseValue,
                owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
