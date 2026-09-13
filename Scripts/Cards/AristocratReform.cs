using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Cards;

/// <summary>
/// 改革（X 费，技能，罕见，消耗）：抽 X+1 张牌，每抽到 1 张打击或防御获得 1 点能量。
/// 升级后不再消耗。对应塔1 的 aristocrat:Reform。
/// </summary>
public sealed class AristocratReform : AristocratCard
{
    public AristocratReform()
        : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        // X 费牌花掉多少能量，X 就是多少
        int x = cardPlay.Resources.EnergySpent;
        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, x + 1, owner);

        int gained = drawn.Count(IsStrikeOrDefend);
        if (gained > 0)
        {
            await PlayerCmd.GainEnergy(gained, owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后这张牌本身不再消耗
        RemoveKeyword(CardKeyword.Exhaust);
    }
}