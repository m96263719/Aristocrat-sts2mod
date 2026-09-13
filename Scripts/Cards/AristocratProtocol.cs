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
/// 礼仪程序（2 费，技能，罕见）：获得 12 点格挡；本回合内，手中所有打击与防御获得保留。
/// 升级 格挡+4。对应塔1 的 aristocrat:Protocol。
/// </summary>
public sealed class AristocratProtocol : AristocratCard
{
    public AristocratProtocol()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(12, ValueProp.Move)];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        foreach (CardModel card in PileType.Hand.GetPile(owner).Cards)
        {
            if (AristocratCard.IsStrikeOrDefend(card))
            {
                card.GiveSingleTurnRetain();
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}