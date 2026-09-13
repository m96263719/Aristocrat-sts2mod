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
/// 亿万富翁（2 费，能力，稀有）：每回合开始时获得 10 金币，这个数值每触发一次减 1。升级费用 1。
/// 对应塔1 的 aristocrat:Billionaire。
/// </summary>
public sealed class AristocratBillionaire : AristocratCard
{
    public AristocratBillionaire()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 10m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<BillionairePower>(
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
