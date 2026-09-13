using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Cards;

/// <summary>
/// 下令（1 费，技能，罕见）：抽 3 张牌；若其中抽到打击或防御，自动打出它们。升级后抽 4 张。
/// 对应塔1 的 aristocrat:Command。
/// </summary>
public sealed class AristocratCommand : AristocratCard
{
    public AristocratCommand()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 3m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        CardPile hand = PileType.Hand.GetPile(owner);

        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, DynamicVars["MagicNumber"].IntValue, owner);

        foreach (CardModel card in drawn.ToList())
        {
            if (!IsStrikeOrDefend(card) || !hand.Cards.Contains(card))
            {
                continue;
            }

            Creature? target = card.TargetType == TargetType.AnyEnemy ? PickRandomEnemy(owner) : null;
            await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}