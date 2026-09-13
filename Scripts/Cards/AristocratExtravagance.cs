using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 奢侈（0 费，技能，普通）：最多失去 12 金币，获得等量格挡。升级 上限+4。
/// 特典：升级牌组中的 1 张打击。对应塔1 的 aristocrat:Extravagance。
/// </summary>
public sealed class AristocratExtravagance : AristocratCard, IPerkCard
{
    public AristocratExtravagance()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [AristocratKeywords.Perk];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 12m)];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        // 最多花这么多，花多少就换多少格挡
        int spend = Math.Min(DynamicVars["MagicNumber"].IntValue, owner.Gold);
        if (spend > 0)
        {
            await PlayerCmd.LoseGold(spend, owner);
            await CreatureCmd.GainBlock(owner.Creature, spend, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(4);
    }

    public Task OnPerkTriggered(Player owner)
    {
        PerkHelpers.UpgradeRandomInDeck(owner, IsStrike);
        return Task.CompletedTask;
    }
}