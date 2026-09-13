using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Powers;

/// <summary>
/// 家臣团：本场战斗中，打击与防御每次被使用时都会升级。
///
/// 塔1 用的是 AbstractPower.onAfterUseCard 这个时机。
/// 塔2 对应的是 AbstractModel.AfterCardPlayed —— 所有模型（能力、遗物、卡牌）都能收到这个钩子。
///
/// 升级次数上限自己算（1 + 超人层数），不依赖卡面的 MaxUpgradeLevel，
/// 这样即使别的 mod 也在修改升级上限，这里的行为依然是对的。
/// </summary>
public sealed class VassalsPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;

        // 只管能力持有者自己打出的牌
        if (card.Owner?.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        if (!AristocratCard.IsStrikeOrDefend(card))
        {
            return Task.CompletedTask;
        }

        // 塔1 的判定：timesUpgraded < 1 + 超人层数
        int allowed = 1 + Owner.GetPowerAmount<SuperhumanPower>();
        if (card.CurrentUpgradeLevel >= allowed)
        {
            return Task.CompletedTask;
        }

        Log.Info($"[Aristocrat] 家臣团：{card.Id.Entry} 从 +{card.CurrentUpgradeLevel} 升到 +{card.CurrentUpgradeLevel + 1}（允许上限 {allowed}，卡面上限 {card.MaxUpgradeLevel}）");
        CardCmd.Upgrade(card);
        return Task.CompletedTask;
    }
}