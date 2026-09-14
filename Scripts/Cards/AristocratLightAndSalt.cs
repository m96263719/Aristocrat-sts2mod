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
/// 光与盐（0 费，攻击，罕见）：造成 5 点伤害，然后给手牌中 1 张固有牌「保留」。升级 +2。
/// 对应塔1 的 aristocrat:LightAndSalt。
///
/// 塔1 用它自己的 TemporaryRetainHelper 做「保留，直到被打出」；
/// 塔2 的 Retain 关键词在这个场景下效果一样（打出去牌就没了，所以等价于保留到打出为止）。
/// </summary>
public sealed class AristocratLightAndSalt : AristocratCard
{
    public AristocratLightAndSalt()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        CardPile hand = PileType.Hand.GetPile(owner);
        bool anyCandidate = hand.Cards.Any(card => !ReferenceEquals(card, this) && IsInnate(card));
        if (!anyCandidate)
        {
            return;
        }

        IEnumerable<CardModel> chosen = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: hand,
            player: owner,
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            filter: card => !ReferenceEquals(card, this) && IsInnate(card));

        foreach (CardModel card in chosen.ToList())
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }

    private static bool IsInnate(CardModel card) => card.Keywords.Contains(CardKeyword.Innate);
}
