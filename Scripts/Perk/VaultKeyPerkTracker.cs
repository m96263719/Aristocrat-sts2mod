using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Perk;

/// <summary>
/// 宝库钥匙的特典标记：**在商店里买下的那张稀有牌**身上记一笔
/// （塔1 的 `hasVaultKeyPerk` 就是"买下来之后这张牌一直带着特典"，所以这必须是牌自己的状态，
/// 不能按"稀有度 + 你有没有钥匙"推导出来——那样连奖励里拿到的稀有牌也会带上）。
///
/// 存法沿用肖像那套：BaseLib 的 <see cref="SavedSpireField{TKey,TVal}"/>，
/// 名字会在 mod 后置初始化时被注册进存档的 net-id 表，
/// 于是 JSON 存档、联机同步、战斗回放三条路都能过；CopyOnClone 让复制出来的牌也带着。
/// 这个标记只是"是/否"，不涉及关键词，所以读档、降级都不需要额外补丁。
/// </summary>
internal static class VaultKeyPerkTracker
{
    /// <summary>存档里的名字是 "CardModel_AristocratVaultKeyPerk"。</summary>
    private static readonly SavedSpireField<CardModel, bool> VaultKeyPerk =
        new(() => false, "AristocratVaultKeyPerk");

    static VaultKeyPerkTracker()
    {
        // 复制牌（多利之镜、战斗内克隆、蛋系遗物改牌）时把标记带过去
        VaultKeyPerk.CopyOnClone();
    }

    /// <summary>
    /// 只是碰一下静态字段，让它在 mod 初始化时就构造出来（=向 BaseLib 登记），
    /// 这样 BaseLib 后面把名字注册进存档的 net-id 表时一定包含它。Entry.Init 里调。
    /// </summary>
    internal static void EnsureRegistered()
    {
        _ = VaultKeyPerk;
    }

    /// <summary>给这张牌打上"带宝库钥匙特典"的标记。</summary>
    internal static void Mark(CardModel? card)
    {
        if (card != null)
        {
            VaultKeyPerk[card] = true;
        }
    }

    /// <summary>这张牌是不是带着宝库钥匙的特典。</summary>
    internal static bool IsMarked(CardModel? card)
    {
        return card != null && VaultKeyPerk[card];
    }
}
