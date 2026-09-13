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
/// 赞助人（2 费，能力，罕见）：每回合开始时失去 5 金币，获得 1 点力量与 1 点敏捷。升级费用 1。
/// 对应塔1 的 aristocrat:Patron。
/// </summary>
public sealed class AristocratPatron : AristocratCard
{
    public AristocratPatron()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<PatronPower>(
            choiceContext,
            owner.Creature,
            DynamicVars["MagicNumber"].BaseValue,
            owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
