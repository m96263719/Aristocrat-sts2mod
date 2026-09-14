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

namespace Aristocrat.Cards;

/// <summary>赦免：消耗至多 2 张手牌。升级 3 张。特典：从牌组中永久移除 1 张牌。</summary>
public sealed class AristocratPardon : AristocratCard, IPerkCard
{
    public AristocratPardon()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        int count = (int)DynamicVars["MagicNumber"].BaseValue;

        IEnumerable<CardModel> chosen = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: PileType.Hand.GetPile(owner),
            player: owner,
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, count));

        foreach (CardModel card in chosen.ToList())
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
    }

    public async Task OnPerkTriggered(Player owner)
    {
        IEnumerable<CardModel> chosen = await CardSelectCmd.FromDeckForRemoval(
            owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, 1));

        foreach (CardModel card in chosen.ToList())
        {
            // showPreview 和本体商店的删牌服务一样（OneOffSynchronizer 里也是这个调用）
            await CardPileCmd.RemoveFromDeck(card, showPreview: true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}
