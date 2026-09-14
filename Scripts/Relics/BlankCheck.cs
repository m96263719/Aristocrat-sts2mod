using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace Aristocrat.Relics;

/// <summary>
/// 空白支票（普通）：塔1 的 aristocrat:BlankCheck —— 牌奖励里多出一个「金币 +15」选项。
///
/// 塔1 是往 CardRewardScreen 的候选牌里塞一张特殊卡；塔2 有专门的机制：
/// 奖励界面底下那排「跳过 / 献祭」按钮就是 CardRewardAlternative，
/// 所以这里挂 TryModifyCardRewardAlternatives，选中后直接拿钱并结束这次奖励（和献祭一样）。
///
/// 注意上限：原版一次最多只支持 2 个额外选项（含「跳过」），超了会直接抛异常，
/// 所以先数一下已经被别人加过几个，装不下就不加，免得把奖励界面搞崩。
/// </summary>
public sealed class BlankCheck : RelicModel
{
    /// <summary>选项 ID，本地化键是 OPTION_&lt;ID 大写&gt;.name（见 localization/*/card_reward_ui.json）。</summary>
    private const string OptionId = "BLANK_CHECK";

    private const decimal GoldAmount = 15m;

    /// <summary>原版奖励界面的额外选项上限（CardRewardAlternative.Generate 会检查）。</summary>
    private const int MaxAlternatives = 2;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override string PackedIconPath => "res://Aristocrat/images/relics/BlankCheck.png";

    protected override string PackedIconOutlinePath => "res://Aristocrat/images/relics/BlankCheckOutline.png";

    protected override string BigIconPath => "res://Aristocrat/images/relics/BlankCheck.png";

    public override bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
    {
        if (player != Owner || alternatives.Count >= MaxAlternatives)
        {
            return false;
        }

        alternatives.Add(new CardRewardAlternative(OptionId, Claim, PostAlternateCardRewardAction.EndSelectionAndCompleteReward));
        return true;
    }

    private async Task Claim()
    {
        Flash();
        await PlayerCmd.GainGold(GoldAmount, Owner);
    }
}
