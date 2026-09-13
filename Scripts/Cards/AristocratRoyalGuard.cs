using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>近卫队：每回合你手动打出的第一张固有牌会打出两次。升级 费用 2 → 1。</summary>
public sealed class AristocratRoyalGuard : AristocratCard
{
    public AristocratRoyalGuard()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<RoyalGuardPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
