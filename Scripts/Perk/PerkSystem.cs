using System.Threading.Tasks;
using Aristocrat.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Perk;

/// <summary>
/// 「特典」的统一判定与触发。
///
/// 塔1 的 PerkHelper 是这么写的：hasPerk(card) = 卡自带的特典 || 宝库钥匙标记，
/// 而 trigger(card) 里两件事都可能发生（卡自己的特典 + 宝库钥匙发遗物）。
/// 塔2 这边原来只有"商店购买"那一处处理宝库钥匙，于是
/// 「天生支配者」（下一张打出的特典牌额外触发一次特典）、赠品、奢侈品、纯血、族谱、加冕这些
/// 认"特典牌"的效果全都不认它。现在统一走这里：凡是问"是不是特典牌""触发特典"都找本类。
/// </summary>
public static class PerkSystem
{
    /// <summary>宝库钥匙那条特典的卡面文字（"特典：获得一件随机遗物。"）。</summary>
    public const string VaultKeyPerkTextKey = "VAULT_KEY.perkText";

    /// <summary>这张牌现在算不算特典牌（卡自带的特典，或者买自商店、带着宝库钥匙特典的牌）。</summary>
    public static bool HasPerk(CardModel? card)
    {
        if (card == null)
        {
            return false;
        }

        return card is IPerkCard || HasVaultKeyPerk(card);
    }

    /// <summary>
    /// 宝库钥匙的特典：**在商店里买下的那张稀有牌**会带着它（买的时候由
    /// <see cref="VaultKeyPerkTracker"/> 打标记并跟着牌一起存档）。
    /// 奖励 / 事件里拿到的稀有牌不在此列。
    /// </summary>
    public static bool HasVaultKeyPerk(CardModel? card)
    {
        return VaultKeyPerkTracker.IsMarked(card);
    }

    /// <summary>触发一张牌的特典：卡自己的效果和宝库钥匙那条都会触发（可能同时触发两条）。</summary>
    public static async Task TriggerPerk(CardModel card, Player owner)
    {
        if (card is IPerkCard perk)
        {
            await perk.OnPerkTriggered(owner);
        }

        if (HasVaultKeyPerk(card))
        {
            await VaultKey.GrantRelic(owner);
        }
    }

    /// <summary>卡面追加用的那行特典说明；本地化缺失时返回 null（宁可不显示也不要抛）。</summary>
    public static string? VaultKeyPerkLine()
    {
        LocString? text = LocString.GetIfExists("relics", VaultKeyPerkTextKey);
        return text?.GetFormattedText();
    }
}
