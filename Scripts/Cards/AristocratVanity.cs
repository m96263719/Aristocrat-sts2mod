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
/// 虚荣（1 费，能力，罕见）：每当你获得或失去金币时，对所有敌人造成 5 点伤害。升级 +3。
/// 对应塔1 的 aristocrat:Vanity。
/// </summary>
public sealed class AristocratVanity : AristocratCard
{
    public AristocratVanity()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 5m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<VanityPower>(
            choiceContext,
            owner.Creature,
            DynamicVars["MagicNumber"].BaseValue,
            owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(3);
    }
}
