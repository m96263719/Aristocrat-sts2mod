using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 征收（2 费，攻击，基础）：造成 14 点伤害，然后获得等同于未被格挡伤害的金币。升级 +4。
/// 对应塔1 的 aristocrat:Taxation（那边用的是自制的 GoldDamageAction）。
///
/// 塔2 的伤害命令会把每次结算的结果留在 AttackCommand.Results 里，
/// 每个 DamageResult 都带 UnblockedDamage 字段，直接取和即可。
/// 观者的「当头棒喝」也是靠这套结果实现的（只是它给的是格挡）。
/// </summary>
public sealed class AristocratTaxation : AristocratCard
{
    public AristocratTaxation()
        : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        var command = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!);

        await command.Execute(choiceContext);

        int unblocked = command.Results.SelectMany(list => list).Sum(result => result.UnblockedDamage);
        if (unblocked > 0)
        {
            await PlayerCmd.GainGold(unblocked, owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}