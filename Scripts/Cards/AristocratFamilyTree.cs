using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Cards;

/// <summary>
/// 族谱（1 费，技能，罕见）：从抽牌堆各抽 1 张打击、防御和特典牌。
/// 升级后把抽到的这几张在本场战斗中升级。对应塔1 的 aristocrat:FamilyTree。
/// </summary>
public sealed class AristocratFamilyTree : AristocratCard
{
    public AristocratFamilyTree()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DrawMatchingFromDrawPile(choiceContext, owner, 1, IsStrike, MaybeUpgrade);
        await DrawMatchingFromDrawPile(choiceContext, owner, 1, IsDefend, MaybeUpgrade);
        await DrawMatchingFromDrawPile(choiceContext, owner, 1, IsPerkCard, MaybeUpgrade);
    }

    private void MaybeUpgrade(CardModel card)
    {
        if (IsUpgraded && card.CurrentUpgradeLevel < card.MaxUpgradeLevel)
        {
            CardCmd.Upgrade(card);
        }
    }

    protected override void OnUpgrade()
    {
    }
}