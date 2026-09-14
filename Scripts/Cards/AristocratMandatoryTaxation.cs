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
/// 强制征税（2 费，攻击，先古）：对所有敌人造成 20 点伤害，并获得等同于未被格挡伤害的金币。
/// 升级 +5（变成 25）。这是征收的先古升级版，卡面沿用征收，先古卡框由稀有度自动带出。
/// </summary>
public sealed class AristocratMandatoryTaxation : AristocratCard
{
    public AristocratMandatoryTaxation()
        : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
    {
    }

    /// <summary>卡面暂时沿用征收，只有卡框会变成先古样式。</summary>
    public override string PortraitPath => "res://Aristocrat/images/cards/AristocratTaxation.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        var command = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .TargetingAllOpponents(owner.Creature.CombatState!);

        await command.Execute(choiceContext);

        int unblocked = command.Results.SelectMany(list => list).Sum(result => result.UnblockedDamage);
        if (unblocked > 0)
        {
            await PlayerCmd.GainGold(unblocked, owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}