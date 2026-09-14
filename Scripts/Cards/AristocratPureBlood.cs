using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Character;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>纯血：造成 5 伤，然后抽 1 张特典牌。升级 伤害 +3。特典：将 1 张随机稀有贵族牌加入牌组。</summary>
public sealed class AristocratPureBlood : AristocratCard, IPerkCard
{
    public AristocratPureBlood()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        await DrawMatchingFromDrawPile(choiceContext, owner, 1, PerkSystem.HasPerk);
    }

    public async Task OnPerkTriggered(Player owner)
    {
        List<CardModel> rares = ModelDb.CardPool<AristocratCardPool>()
            .AllCards
            .Where(card => card.Rarity == CardRarity.Rare)
            .ToList();

        if (rares.Count == 0)
        {
            return;
        }

        CardModel reward = owner.RunState.CreateCard(rares[Random.Shared.Next(rares.Count)], owner);
        await CardPileCmd.Add(reward, PileType.Deck, CardPilePosition.Top, null, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
