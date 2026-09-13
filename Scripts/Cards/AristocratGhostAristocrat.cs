using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 幽灵贵族（1 费，技能，稀有）：下回合使用的打击与防御造成 2 倍伤害并获得 2 倍格挡。
/// 升级后费用降为 0。对应塔1 的 aristocrat:GhostAristocrat。
/// </summary>
public sealed class AristocratGhostAristocrat : AristocratCard
{
    public AristocratGhostAristocrat()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<GhostAristocratNextTurnPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}