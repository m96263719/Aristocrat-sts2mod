using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 遗产税（1 费，技能，罕见）：每拥有 20 金币获得 1 点格挡。升级费用 0。
/// 对应塔1 的 aristocrat:InheritanceTax。
/// </summary>
public sealed class AristocratInheritanceTax : AristocratCard
{
    public AristocratInheritanceTax()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 20m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        int per = (int)DynamicVars["MagicNumber"].BaseValue;
        int block = per > 0 ? owner.Gold / per : 0;
        if (block > 0)
        {
            await CreatureCmd.GainBlock(owner.Creature, block, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
