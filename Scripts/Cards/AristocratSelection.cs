using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 筛选（2 费，攻击，罕见）：造成 6 点伤害；选择任意数量的手牌，在本场战斗中把它们变化为打击，
/// 然后把这些打击对随机敌人打出。升级 +3。对应塔1 的 aristocrat:Selection。
/// </summary>
public sealed class AristocratSelection : AristocratCard
{
    public AristocratSelection()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        CardPile hand = PileType.Hand.GetPile(owner);

        IEnumerable<CardModel> chosen = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: hand,
            player: owner,
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, 99),
            filter: card => !ReferenceEquals(card, this));

        ICombatState combat = owner.Creature.CombatState!;
        foreach (CardModel card in chosen.ToList())
        {
            CardModel replacement = combat.CreateCard<AristocratStrike>(owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(replacement);
            }

            await CardCmd.Transform(card, replacement);

            if (hand.Cards.Contains(replacement))
            {
                Creature? target = PickRandomEnemy(owner);
                await CardCmd.AutoPlay(choiceContext, replacement, target, AutoPlayType.Default);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}