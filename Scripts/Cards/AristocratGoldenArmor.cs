using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 黄金盔甲（1 费，技能，稀有，消耗）：下回合获得等同于当前格挡的格挡和金币。升级费用 0。
/// 对应塔1 的 aristocrat:GoldenArmor。
/// </summary>
public sealed class AristocratGoldenArmor : AristocratCard
{
    public AristocratGoldenArmor()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        int block = owner.Creature.Block;
        if (block <= 0)
        {
            return;
        }

        await PowerCmd.Apply<GoldenArmorNextTurnPower>(
            choiceContext,
            owner.Creature,
            block,
            owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
