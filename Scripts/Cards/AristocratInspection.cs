using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Cards;

/// <summary>
/// 检阅（1 费，技能，罕见）：从抽牌堆各抽 1 张打击与防御，并把抽到的牌在本场战斗中升级。
/// 升级后费用降为 0。对应塔1 的 aristocrat:Inspection。
/// </summary>
public sealed class AristocratInspection : AristocratCard
{
    public AristocratInspection()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DrawMatchingFromDrawPile(choiceContext, owner, 1, IsStrike, UpgradeIfPossible);
        await DrawMatchingFromDrawPile(choiceContext, owner, 1, IsDefend, UpgradeIfPossible);
    }

    private static void UpgradeIfPossible(CardModel card)
    {
        if (card.CurrentUpgradeLevel < card.MaxUpgradeLevel)
        {
            CardCmd.Upgrade(card);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}