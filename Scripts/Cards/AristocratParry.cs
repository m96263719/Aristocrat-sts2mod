using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>招架：对所有敌人造成 5 伤；每有一名意图攻击的敌人，获得 3 格挡。升级各 +2。</summary>
public sealed class AristocratParry : AristocratCard
{
    public AristocratParry()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new BlockVar(3, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(owner.Creature.CombatState!)
            .Execute(choiceContext);

        int attackers = owner.Creature.CombatState!.HittableEnemies
            .Count(enemy => enemy.Monster?.IntendsToAttack ?? false);

        if (attackers > 0)
        {
            await CreatureCmd.GainBlock(
                owner.Creature,
                DynamicVars.Block.BaseValue * attackers,
                ValueProp.Move,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
