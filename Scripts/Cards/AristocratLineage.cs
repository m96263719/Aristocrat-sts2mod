using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>血统（3 费，技能，基础，固有 + 保留）：获得 17 点格挡，升级 +6。对应塔1 的 aristocrat:Lineage。</summary>
public sealed class AristocratLineage : AristocratCard
{
    public AristocratLineage()
        : base(3, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(17, ValueProp.Move)];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner!.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(6);
    }
}
