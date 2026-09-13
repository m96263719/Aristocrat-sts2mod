using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>锦衣玉食（消耗）：造成 7 伤并回复 3 点生命。升级 伤害 +3、回复 +1。特典：最大生命 +3。</summary>
public sealed class AristocratLuxuryDining : AristocratCard, IPerkCard
{
    private const int PerkMaxHp = 3;

    public AristocratLuxuryDining()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("MagicNumber", 3m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        await CreatureCmd.Heal(owner.Creature, DynamicVars["MagicNumber"].BaseValue);
    }

    public async Task OnPerkTriggered(Player owner)
    {
        await CreatureCmd.GainMaxHp(owner.Creature, PerkMaxHp);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}
