using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Aristocrat.Cards;

/// <summary>
/// 体面（1 费，能力，罕见）：每当你使用打击时获得 M 点格挡；每当你使用防御时对所有敌人造成 M 点伤害。
/// 升级 M+1。对应塔1 的 aristocrat:Dignity。
/// </summary>
public sealed class AristocratDignity : AristocratCard
{
    public AristocratDignity()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 3m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<DignityPower>(
            choiceContext,
            owner.Creature,
            DynamicVars["MagicNumber"].BaseValue,
            owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}