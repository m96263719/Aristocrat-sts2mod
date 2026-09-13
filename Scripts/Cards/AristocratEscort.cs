using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 护卫队（2 费，技能，罕见）：自动打出手中所有打击与防御。升级后先抽 2 张。
/// 对应塔1 的 aristocrat:Escort。
/// </summary>
public sealed class AristocratEscort : AristocratCard
{
    public AristocratEscort()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        if (IsUpgraded)
        {
            await CardPileCmd.Draw(choiceContext, 2, owner);
        }

        await PlayAllMatchingInHand(choiceContext, owner, IsStrikeOrDefend);
    }

    protected override void OnUpgrade()
    {
    }
}