using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Utils;

namespace Aristocrat.Cards;

/// <summary>
/// 修养（1 费，攻击，普通）：造成 7 点伤害，然后从弃牌堆里选 1 张固有牌收回手牌。升级 +3。
/// 对应塔1 的 aristocrat:Refinement。
/// </summary>
public sealed class AristocratRefinement : AristocratCard
{
    public AristocratRefinement()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        CardPile discard = PileType.Discard.GetPile(owner);
        if (!discard.Cards.Any(card => card.Keywords.Contains(CardKeyword.Innate)))
        {
            return;
        }

        IEnumerable<CardModel> chosen = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: discard,
            player: owner,
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            filter: card => card.Keywords.Contains(CardKeyword.Innate));

        foreach (CardModel card in chosen.ToList())
        {
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top, null, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
