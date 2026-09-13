using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 收回财产（不能使用，虚无）：一张纯特典牌——买下它的时候直接拿 300 金币。
/// 对应塔1 的 aristocrat:RecoveredProperty（那边费用 -2、不可使用）。
/// </summary>
public sealed class AristocratRecoveredProperty : AristocratCard, IPerkCard
{
    private const int PerkGold = 300;

    public AristocratRecoveredProperty()
        : base(0, CardType.Skill, CardRarity.Event, TargetType.None)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];

    public override int MaxUpgradeLevel => 0;

    public async Task OnPerkTriggered(Player owner)
    {
        await PlayerCmd.GainGold(PerkGold, owner);
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }
}
