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
/// 黄金之手（X 费，消耗）：对所有敌人造成 4 伤 X 次，给予所有敌人 X 层虚弱和易伤，获得 10X 金币。升级 伤害 +2。
/// 塔2 的 X 费就是 HasEnergyCostX + ResolveEnergyXValue()。
/// </summary>
public sealed class AristocratGoldenHand : AristocratCard
{
    public AristocratGoldenHand()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        int x = ResolveEnergyXValue();
        if (x <= 0)
        {
            return;
        }

        await PlayerCmd.GainGold(10 * x, owner);

        for (int i = 0; i < x; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(owner.Creature.CombatState!)
                .Execute(choiceContext);
        }

        foreach (Creature enemy in owner.Creature.CombatState!.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, x, owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, x, owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
