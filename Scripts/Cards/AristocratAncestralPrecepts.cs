using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 祖训（3 费，能力，先古，固有）：本场战斗中，你的固有牌费用减 1。
/// 这张是给「尘封魔典」用的先古卡——尘封魔典会从角色的卡池里随机挑一张先古稀有度的卡，
/// 所以只要把它加进贵族卡池，魔典就有可能给出它（并自动是升级版）。
/// </summary>
public sealed class AristocratAncestralPrecepts : AristocratCard
{
    public AristocratAncestralPrecepts()
        : base(3, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<AncestralPreceptsPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级只降费（3 → 2），效果不变：尘封魔典给的就是升级版，所以玩家实际拿到的都是这张。
        EnergyCost.UpgradeBy(-1);
    }
}
