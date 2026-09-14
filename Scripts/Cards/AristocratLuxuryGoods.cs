using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>奢侈品：获得 7 点格挡，然后让手牌中 1 张特典牌的费用变为 0（直到打出）。升级 格挡 +3。</summary>
public sealed class AristocratLuxuryGoods : AristocratCard
{
    public AristocratLuxuryGoods()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        CardPile hand = PileType.Hand.GetPile(owner);
        if (!hand.Cards.Any(PerkSystem.HasPerk))
        {
            return;
        }

        IEnumerable<CardModel> chosen = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: hand,
            player: owner,
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
            filter: PerkSystem.HasPerk);

        foreach (CardModel card in chosen.ToList())
        {
            card.SetToFreeThisTurn();
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
