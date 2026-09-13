using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Perk;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 召命（2 费，技能，普通）：获得 10 点格挡，给予所有敌人 1 层虚弱。升级 格挡+4 / 虚弱+1。
/// 特典：升级牌组中的 1 张防御。对应塔1 的 aristocrat:Calling。
/// </summary>
public sealed class AristocratCalling : AristocratCard, IPerkCard
{
    public AristocratCalling()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [AristocratKeywords.Perk];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10, ValueProp.Move),
        new DynamicVar("MagicNumber", 1m),
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        List<Creature> enemies = owner.Creature.CombatState!.HittableEnemies.ToList();
        if (enemies.Count > 0)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemies, DynamicVars["MagicNumber"].BaseValue, owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }

    public Task OnPerkTriggered(Player owner)
    {
        PerkHelpers.UpgradeRandomInDeck(owner, IsDefend);
        return Task.CompletedTask;
    }
}