using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 茶会邀请（1 费，技能，普通，固有）：失去 5 金币，获得 2 点能量。金币不足 5 时白打（不扣钱也不给能量）。
/// 升级费用 0。对应塔1 的 aristocrat:TeaTimeInvitation。
/// </summary>
public sealed class AristocratTeaTimeInvitation : AristocratCard, IGoldCostCard
{
    private const int Price = 5;

    public int GoldCost => Price;

    public AristocratTeaTimeInvitation()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        // 金币不足时这张牌根本打不出来（见 GoldCostPlayPatch），所以这里必然是够的
        await AristocratGold.SpendUpTo(Price, owner);
        await PlayerCmd.GainEnergy(2, owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
