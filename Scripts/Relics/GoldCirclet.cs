using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Aristocrat.Relics;

/// <summary>
/// 黄金冠（商店）：塔1 的 aristocrat:GoldCirclet。两件事：
///
/// 1. 战斗开始时随机获得 2 点力量或 2 点敏捷。
///
/// 2. 「买过了也会再出现」——塔1 的 GoldCircletRewardPatch 不是靠遗物池，而是靠"污染"：
///    身上每有一顶黄金冠，就有 5% 的概率（上限 50%）把别的奖励替换成黄金冠，
///    商店货架上的遗物同理，本体的「头环」一旦出现也直接变成黄金冠。
///    所以这里也要覆盖奖励（TryModifyRewardsLate + Patches\GoldCircletRewardPatch），
///    光把它塞回抽取池是不够的，而且转经轮那种"额外加一条牌奖励"的效果也算在内。
/// </summary>
public sealed class GoldCirclet : RelicModel
{
    private const decimal Boost = 2m;

    /// <summary>每有一顶黄金冠 +5% 概率（塔1 的 CHANCE_PER_CIRCLET）。</summary>
    private const int ChancePerCirclet = 5;

    /// <summary>概率上限 50%（塔1 的 MAX_CHANCE）。</summary>
    private const int MaxChance = 50;

    public override RelicRarity Rarity => RelicRarity.Shop;

    /// <summary>和本体「头环」一样可叠加：拿到手里也不会被从抽取池划掉，所以还能再刷到。</summary>
    public override bool IsStackable => true;

    public override string PackedIconPath => "res://Aristocrat/images/relics/GoldCirclet.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/GoldCircletOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/GoldCirclet.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>((int)Boost),
        HoverTipFactory.FromPower<DexterityPower>((int)Boost),
    ];

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        Player? owner = Owner;
        if (owner == null || room is not CombatRoom)
        {
            return;
        }

        Flash();
        if (owner.RunState.Rng.Niche.NextBool())
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), owner.Creature, Boost, owner.Creature, null);
        }
        else
        {
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), owner.Creature, Boost, owner.Creature, null);
        }
    }

    /// <summary>
    /// 塔1 的「卡牌奖励 / 遗物奖励有概率变成黄金冠」。
    ///
    /// 用 Late 版本：转经轮（PrayerWheel）这类遗物是在 TryModifyRewards 里"追加"一条牌奖励的，
    /// 只有等所有 TryModifyRewards 跑完才看得到它——塔1 里小怪也有可能掉两个黄金冠就是这个原因。
    /// </summary>
    public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || OwnedCount(player) <= 0)
        {
            return false;
        }

        bool replaced = false;
        for (int i = 0; i < rewards.Count; i++)
        {
            if (!ShouldReplaceReward(player, rewards[i]))
            {
                continue;
            }

            rewards[i] = new RelicReward(NewMutable(), player);
            replaced = true;
        }

        if (replaced)
        {
            Flash();
        }

        return replaced;
    }

    /// <summary>抽取池里刷出本体「头环」时的替换（奖励 / 宝箱；有概率的那部分交给奖励钩子）。</summary>
    internal static RelicModel ReplaceCirclet(Player player, RelicModel relic)
    {
        return ReplacePulledRelic(player, relic, null);
    }

    /// <summary>商店货架上的遗物替换（塔1 是 StoreRelic 构造时替换，用商店自己的 RNG）。</summary>
    internal static RelicModel ReplaceShopRelic(Player player, RelicModel relic)
    {
        return ReplacePulledRelic(player, relic, player.PlayerRng.Shops);
    }

    private static RelicModel ReplacePulledRelic(Player player, RelicModel relic, Rng? rng)
    {
        if (IsGoldCirclet(relic))
        {
            return relic;
        }

        int owned = OwnedCount(player);
        if (owned <= 0)
        {
            return relic;
        }

        bool isCirclet = IsCirclet(relic);
        if (!isCirclet && (rng == null || !RollReplaceChance(owned, rng)))
        {
            return relic;
        }

        // 注意返回的是规范模型：调用方（奖励 / 商店）随后会自己 ToMutable()
        return ModelDb.Relic<GoldCirclet>();
    }

    private static bool ShouldReplaceReward(Player player, Reward reward)
    {
        int owned = OwnedCount(player);

        switch (reward)
        {
            case CardReward:
                return RollReplaceChance(owned, player.PlayerRng.Rewards);

            case RelicReward relicReward:
                if (relicReward.Relic is not { } relic || IsGoldCirclet(relic))
                {
                    return false;
                }

                // 本体头环一旦出现就直接变黄金冠（塔1 的同一条规则，不掷骰）
                return IsCirclet(relic) || RollReplaceChance(owned, player.PlayerRng.Rewards);

            default:
                return false;
        }
    }

    private static bool RollReplaceChance(int owned, Rng rng)
    {
        if (owned <= 0)
        {
            return false;
        }

        return rng.NextInt(100) < Math.Min(MaxChance, owned * ChancePerCirclet);
    }

    private static int OwnedCount(Player player)
    {
        return player.Relics.Count(relic => relic is GoldCirclet);
    }

    private static bool IsGoldCirclet(RelicModel relic)
    {
        return relic is GoldCirclet;
    }

    private static bool IsCirclet(RelicModel relic)
    {
        return relic.Id == ModelDb.Relic<Circlet>().Id;
    }

    private static GoldCirclet NewMutable()
    {
        return (GoldCirclet)ModelDb.Relic<GoldCirclet>().ToMutable();
    }
}
