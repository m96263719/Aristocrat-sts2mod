using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Aristocrat.Cards;

/// <summary>
/// 授予（1 费，技能，罕见）：将 1 张究极防御加入手牌。
/// 特典：将 1 张究极打击加入你的牌组。对应塔1 的 aristocrat:Grant。
/// </summary>
public sealed class AristocratGrant : AristocratCard, IPerkCard
{
    public AristocratGrant()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [AristocratKeywords.Perk];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        ICombatState combat = owner.Creature.CombatState!;

        CardModel card = combat.CreateCard<UltimateDefend>(owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, owner, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
    }

    public async Task OnPerkTriggered(Player owner)
    {
        await PerkHelpers.AddCardToDeck<UltimateStrike>(owner);
    }
}