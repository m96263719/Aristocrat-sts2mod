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

/// <summary>
/// 肖像：给予手牌中 1 张牌「保留」。升级 张数 +1。特典：永久给予牌组中 1 张牌「固有」。
///
/// 特典给的固有是"永久"的，而塔2 的卡牌存档里没有关键词这一项，
/// 所以走 PersistentKeywordTracker：加关键词的同时打上存档标记，读档 / 复制 / 降级时再补回来。
/// </summary>
public sealed class AristocratPortrait : AristocratCard, IPerkCard
{
    public AristocratPortrait()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        CardPile hand = PileType.Hand.GetPile(owner);
        if (hand.Cards.Count == 0)
        {
            return;
        }

        IEnumerable<CardModel> chosen = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: hand,
            player: owner,
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, (int)DynamicVars["MagicNumber"].BaseValue));

        foreach (CardModel card in chosen.ToList())
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
        }
    }

    public async Task OnPerkTriggered(Player owner)
    {
        IEnumerable<CardModel> chosen = await CardSelectCmd.FromDeckGeneric(
            owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, 1));

        foreach (CardModel card in chosen.ToList())
        {
            PersistentKeywordTracker.MakePersistentInnate(card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}
