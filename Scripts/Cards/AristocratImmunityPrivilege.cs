using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Aristocrat.Cards;

/// <summary>
/// 豁免特权（2 费，技能，稀有，消耗）：失去 40 金币，获得 1 层无实体。金币不足 40 时白打。
/// 升级层数 +1。对应塔1 的 aristocrat:ImmunityPrivilege。
/// </summary>
public sealed class AristocratImmunityPrivilege : AristocratCard, IGoldCostCard
{
    private const int Price = 40;

    public int GoldCost => Price;

    public AristocratImmunityPrivilege()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        // 金币不足时这张牌根本打不出来（见 GoldCostPlayPatch），所以这里必然是够的
        await AristocratGold.SpendUpTo(Price, owner);

        await PowerCmd.Apply<IntangiblePower>(
            choiceContext,
            owner.Creature,
            DynamicVars["MagicNumber"].BaseValue,
            owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}
