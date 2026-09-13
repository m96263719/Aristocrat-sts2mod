using System.Collections.Generic;
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
/// 丝绸帷幕（1 费，技能，罕见，保留）：获得 6 点格挡。
/// 另外：如果你在手中有这张牌时开始回合，抽 1 张防御并将其升级。升级 格挡+3。
/// 对应塔1 的 aristocrat:SilkCurtain（回合开始那部分原本写在 SilkCurtainStartTurnPatch 里，
/// 塔2 的卡牌在手牌里时本身就能收到战斗钩子，所以直接写在卡上）。
/// </summary>
public sealed class AristocratSilkCurtain : AristocratCard
{
    public AristocratSilkCurtain()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6, ValueProp.Move)];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner!.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
    }

    /// <summary>回合开始时如果这张牌还在手上，抽 1 张防御并升级它。</summary>
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player != Owner || Pile?.Type != PileType.Hand)
        {
            return;
        }

        await DrawMatchingFromDrawPile(choiceContext, player, 1, IsDefend, UpgradeIfPossible);
    }

    private static void UpgradeIfPossible(CardModel card)
    {
        if (card.CurrentUpgradeLevel < card.MaxUpgradeLevel)
        {
            CardCmd.Upgrade(card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}