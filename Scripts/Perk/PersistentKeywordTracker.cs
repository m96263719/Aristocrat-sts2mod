using System;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Aristocrat.Perk;

/// <summary>
/// 「永久」关键词的存档支持。目前只有肖像特典给的[固有]用它。
///
/// 塔2 的卡牌存档里没有关键词这一项，所以要自己存一个"这张牌有我们的永久关键词"的标记。
///
/// **不要自己往 SerializableCard.Props 里塞新名字**：存档走 JSON 时未知名字会被跳过，
/// 但联机同步 / 战斗回放走的是二进制 `SavedProperties.Serialize`，那里每个名字都要先在
/// `SavedPropertiesTypeCache` 里注册成 net ID，没注册的名字会直接抛
/// "SavedProperty name ... could not be mapped to any net ID!"。
/// （第一版就是这么写的，结果快速 SL 的重启房间、以及战斗结算写回放的时候都会炸。）
///
/// BaseLib 正好提供了这套设施：<see cref="SavedSpireField{TKey,TVal}"/>。
/// 在 mod 里声明一个挂在 CardModel 上的静态字段，BaseLib 会在"mod 后置初始化"时
/// 自动把它注册进 SavedPropertiesTypeCache（连 NetIdBitSize 一起调好），
/// 并接管 JSON 存档的导出 / 导入；CopyOnClone() 还能让它跟着牌一起被复制（多利之镜、战斗内克隆）。
///
/// 剩下两件事得自己干（见 Patches/PersistentKeywordPatches.cs）：
///   * 关键词本身不在存档里，读档后要补回来；
///   * `DowngradeInternal` 会用本体牌重建 `_keywords`，临时升级结束时也要补回来。
/// </summary>
internal static class PersistentKeywordTracker
{
    /// <summary>
    /// 永久固有标记。存档里的名字是 "CardModel_AristocratPersistentInnate"（BaseLib 用 类型名_名字 拼）。
    /// </summary>
    internal static readonly SavedSpireField<CardModel, bool> PersistentInnate =
        new(() => false, "AristocratPersistentInnate");

    /// <summary>第一版写在 Props.ints 里的标记名；现在只读不写，用来兼容已经存过档的存档。</summary>
    private const string LegacyPropsMarker = "SavedAristocratPortraitInnateMarker";

    static PersistentKeywordTracker()
    {
        // 复制牌（多利之镜、战斗内克隆）时把标记带过去
        PersistentInnate.CopyOnClone();
    }

    /// <summary>只是碰一下静态字段，让它在 mod 初始化时就构造出来（=向 BaseLib 登记）。Entry.Init 里调。</summary>
    internal static void EnsureRegistered()
    {
        _ = PersistentInnate;
    }

    /// <summary>给一张牌永久加上[固有]，并记下"它要一直带着"。</summary>
    internal static void MakePersistentInnate(CardModel? card)
    {
        if (card == null)
        {
            return;
        }

        PersistentInnate[card] = true;
        ApplyKeyword(card);
    }

    internal static bool IsPersistentInnate(CardModel? card)
    {
        return card != null && PersistentInnate[card];
    }

    /// <summary>读档 / 降级之后把关键词补回来。</summary>
    internal static void RestoreInnate(CardModel? card)
    {
        if (card == null || !PersistentInnate[card])
        {
            return;
        }

        ApplyKeyword(card);
    }

    /// <summary>复制 / 变化出一张新牌时，把"永久"这个标记也带过去。</summary>
    internal static void CarryOverInnate(CardModel? source, CardModel? target)
    {
        if (source == null || target == null || !IsPersistentInnate(source))
        {
            return;
        }

        MakePersistentInnate(target);
    }

    /// <summary>
    /// 旧存档兼容：第一版把标记写在 Props.ints 里，读到就当成永久固有，
    /// 下一次存档会以新字段的形式写回去。
    /// </summary>
    internal static void MigrateLegacyMarker(CardModel card, SerializableCard save)
    {
        try
        {
            bool legacy = save.Props?.ints?.Any(property => property.name == LegacyPropsMarker && property.value != 0) == true;
            if (legacy)
            {
                MakePersistentInnate(card);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 迁移旧的永久固有标记失败：{ex.GetBaseException().Message}");
        }
    }

    private static void ApplyKeyword(CardModel card)
    {
        if (!card.Keywords.Contains(CardKeyword.Innate))
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Innate);
        }
    }
}
