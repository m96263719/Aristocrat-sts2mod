using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 法度（2 费，能力，稀有）：本场战斗中你的固有牌具有保留；固有牌每被保留一次，费用再降 1。升级费用 1。
/// 对应塔1 的 aristocrat:Law。
/// </summary>
public sealed class AristocratLaw : AristocratCard
{
    public AristocratLaw()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<LawPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
