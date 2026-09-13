using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 家臣团（1 费，能力，罕见）：
///   本场战斗中，打击与防御每次被使用时都会升级。升级后额外获得 1 层「超人」。
/// 对应塔1 的 aristocrat:Vassals。
/// </summary>
public sealed class AristocratVassals : AristocratCard
{
    public AristocratVassals()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await PowerCmd.Apply<VassalsPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<SuperhumanPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
