using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Utils;

namespace Aristocrat.Cards;

/// <summary>
/// 天命（1 费，攻击，稀有，消耗）：造成 10 点伤害，获得 M 张究极防御。升级 伤害+4 / 张数+1。
/// 特典：升级牌组中所有打击。对应塔1 的 aristocrat:Mandate。
/// </summary>
public sealed class AristocratMandate : AristocratCard, IPerkCard
{
    public AristocratMandate()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, AristocratKeywords.Perk];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new DynamicVar("MagicNumber", 1m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        int count = DynamicVars["MagicNumber"].IntValue;
        ICombatState combat = owner.Creature.CombatState!;

        for (int i = 0; i < count; i++)
        {
            CardModel card = combat.CreateCard<UltimateDefend>(owner);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, owner, CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }

    public Task OnPerkTriggered(Player owner)
    {
        PerkHelpers.UpgradeAllInDeck(owner, IsStrike);
        return Task.CompletedTask;
    }
}