using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Aristocrat.Cards;

/// <summary>
/// 伟大血脉（1 费，技能，罕见）：获得 M 点力量和敏捷，并把 1 张打击、1 张防御和 1 张伤口洗入抽牌堆。
/// 升级 M+1。对应塔1 的 aristocrat:GreatLineage。
/// </summary>
public sealed class AristocratGreatLineage : AristocratCard
{
    public AristocratGreatLineage()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        decimal amount = DynamicVars["MagicNumber"].BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, owner.Creature, amount, owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, owner.Creature, amount, owner.Creature, this);

        ICombatState combat = owner.Creature.CombatState!;
        CardModel strike = combat.CreateCard<AristocratStrike>(owner);
        CardModel defend = combat.CreateCard<AristocratDefend>(owner);
        CardModel wound = combat.CreateCard<Wound>(owner);

        foreach (CardModel card in new[] { strike, defend, wound })
        {
            // 洗进抽牌堆也是要预览的（本体的 Metamorphosis / CaptureSpirit 都是这么写的）
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, owner, CardPilePosition.Random));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}
