using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 城墙（1 费，技能，罕见）：获得 7 点格挡；把手中所有状态牌和诅咒牌在本场战斗中变化为防御。升级 格挡+3。
/// 对应塔1 的 aristocrat:Rampart。
/// </summary>
public sealed class AristocratRampart : AristocratCard
{
    public AristocratRampart()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        ICombatState combat = owner.Creature.CombatState!;
        foreach (CardModel card in PileType.Hand.GetPile(owner).Cards.ToList())
        {
            if (card.Type != CardType.Status && card.Type != CardType.Curse)
            {
                continue;
            }

            CardModel defend = combat.CreateCard<AristocratDefend>(owner);
            await CardCmd.Transform(card, defend);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}