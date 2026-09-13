using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 真祖（1 费，攻击，稀有）：造成 10 点伤害，然后把弃牌堆里所有固有牌收回手牌。升级 +4。
/// 对应塔1 的 aristocrat:TrueAncestor。
/// </summary>
public sealed class AristocratTrueAncestor : AristocratCard
{
    public AristocratTrueAncestor()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        List<CardModel> innates = PileType.Discard.GetPile(owner).Cards
            .Where(card => card.Keywords.Contains(CardKeyword.Innate))
            .ToList();

        foreach (CardModel card in innates)
        {
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top, null, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}
