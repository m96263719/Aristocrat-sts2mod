using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Aristocrat.Cards;

/// <summary>
/// 追缴（0 费，技能，罕见，消耗）：给予目标 1 层易伤，并挂上「追缴」——
/// 本回合中你每对它造成一次受加成的攻击伤害，就获得 3 金币。升级易伤 +1。
/// 对应塔1 的 aristocrat:Collection（那边金币数固定 3，升级加的是易伤层数）。
/// </summary>
public sealed class AristocratCollection : AristocratCard
{
    public AristocratCollection()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("MagicNumber", 1m),
        new DynamicVar("MagicNumber2", 3m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            target,
            DynamicVars["MagicNumber"].BaseValue,
            owner.Creature,
            this);

        await PowerCmd.Apply<CollectingPower>(
            choiceContext,
            target,
            DynamicVars["MagicNumber2"].BaseValue,
            owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}
