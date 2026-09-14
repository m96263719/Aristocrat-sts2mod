using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Aristocrat.Relics;

/// <summary>
/// 钱币盒（稀有）：塔1 的 aristocrat:CoinBox。
///   * 拾取时获得一张「老旧钱币」
///   * 接下来 2 次拿到的遗物换成「老旧钱币」
///
/// 替换分两条路：
///   1. 战斗 / 宝箱的遗物奖励走 TryModifyRewards，直接把奖励条目换成老旧钱币，
///      这样奖励界面上显示的就是老旧钱币（塔1 也是在奖励条目构造时替换的）；
///   2. 商店购买、事件给的遗物走 Patches\CoinBoxRelicPatch（挂在 RelicCmd.Obtain 上）兜底。
///
/// 两条路合起来正好是"接下来获得 2 件遗物"，且都通过 TryConsume 计数，不会多扣。
/// </summary>
public sealed class CoinBox : RelicModel
{
    /// <summary>总共替换几件（塔1 的 counter = 2）。</summary>
    public const int ReplacementCount = 2;

    private int _replacementsLeft;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    public override bool ShowCounter => true;

    public override int DisplayAmount => ReplacementsLeft;

    public override string PackedIconPath => "res://Aristocrat/images/relics/CoinBox.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/CoinBoxOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/CoinBox.png";

    /// <summary>还能替换几件遗物。</summary>
    [SavedProperty]
    public int ReplacementsLeft
    {
        get => _replacementsLeft;
        set
        {
            AssertMutable();
            _replacementsLeft = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterObtained()
    {
        Flash();
        ReplacementsLeft = ReplacementCount;
        await RelicCmd.Obtain<OldCoin>(Owner);
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || ReplacementsLeft <= 0)
        {
            return false;
        }

        bool replaced = false;
        for (int i = 0; i < rewards.Count && ReplacementsLeft > 0; i++)
        {
            if (rewards[i] is not RelicReward reward || reward.Relic is not { } relic || IsExempt(relic))
            {
                continue;
            }

            rewards[i] = new RelicReward(NewOldCoin(), player);
            TryConsume();
            replaced = true;
        }

        return replaced;
    }

    /// <summary>扣一次替换次数；扣成功就闪一下。</summary>
    internal bool TryConsume()
    {
        if (ReplacementsLeft <= 0)
        {
            return false;
        }

        ReplacementsLeft--;
        Flash();
        return true;
    }

    /// <summary>老旧钱币本身和钱币盒自己不算在替换范围内。</summary>
    internal static bool IsExempt(RelicModel relic)
    {
        return relic.Id == OldCoinId || relic is CoinBox;
    }

    internal static RelicModel NewOldCoin()
    {
        return ModelDb.Relic<OldCoin>().ToMutable();
    }

    private static ModelId OldCoinId => ModelDb.Relic<OldCoin>().Id;
}
