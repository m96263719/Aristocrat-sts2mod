using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
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
/// 敕令（2 费，攻击，普通）：造成 12 点伤害；选择抽牌堆中至多 2 张状态牌，在本场战斗中把它们变化为打击。
/// 升级 +4。对应塔1 的 aristocrat:Edict（那边用的是 SelectDrawPileCardsAction）。
/// </summary>
public sealed class AristocratEdict : AristocratCard
{
    public AristocratEdict()
        : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        IEnumerable<CardModel> chosen = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: PileType.Draw.GetPile(owner),
            player: owner,
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, 2),
            filter: card => card.Type == CardType.Status);

        ICombatState combat = owner.Creature.CombatState!;
        foreach (CardModel card in chosen.ToList())
        {
            CardModel replacement = combat.CreateCard<AristocratStrike>(owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(replacement);
            }

            await CardCmd.Transform(new[] { new CardTransformation(card, replacement) }, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}
