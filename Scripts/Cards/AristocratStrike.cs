using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 打击（1 费，攻击，基础）：造成 6 点伤害，升级 +3。对应塔1 的 aristocrat:Strike。
///
/// 塔1 里这张牌会检查玩家是否有「威严」，有的话改成打全体；塔2 同样处理，
/// 因为威严会让打击的目标类型变成"所有敌人"，此时游戏不会传目标进来。
/// </summary>
public sealed class AristocratStrike : AristocratCard
{
    public AristocratStrike()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        AttackCommand command = DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this);

        if (owner.Creature.GetPowerAmount<MajestyPower>() > 0)
        {
            command = command.TargetingAllOpponents(owner.Creature.CombatState!);
        }
        else
        {
            command = command.Targeting(cardPlay.Target!);
        }

        await command.Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}