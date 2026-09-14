using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Relics;

/// <summary>
/// 白金盾（罕见）：塔1 的 aristocrat:PlatinumShield —— 防御牌额外提供 3 点格挡。
///
/// 塔1 是在 AbstractCard.calculateCardDamage / applyPowers 前后临时把 baseBlock +3，
/// 塔2 有现成的 ModifyBlockAdditive 钩子（敏捷用的就是它），判定条件照抄 DexterityPower：
/// 必须是自己打出的牌，而且这次获得格挡属于"被能力加成过的出牌"，否则连敌人的行动都会吃到加成。
/// </summary>
public sealed class PlatinumShield : RelicModel
{
    private const string ExtraBlockKey = "ExtraBlock";

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => "res://Aristocrat/images/relics/PlatinumShield.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/PlatinumShieldOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/PlatinumShield.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(ExtraBlockKey, 3m)];

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource == null || cardSource.Owner != Owner)
        {
            return 0m;
        }

        if (!props.IsPoweredCardOrMonsterMoveBlock())
        {
            return 0m;
        }

        if (!AristocratCard.IsDefend(cardSource))
        {
            return 0m;
        }

        return DynamicVars[ExtraBlockKey].BaseValue;
    }
}
