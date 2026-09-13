using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Aristocrat.Perk;

/// <summary>
/// 「特典」关键词的注册。
///
/// 塔2 的关键词是"枚举值 + card_keywords 文本表"的结构，mod 要加新关键词必须借助
/// BaseLib 的 [CustomEnum] 机制（崩坠2 也是这么做的）。
///
/// 文本键的拼法（BaseLib 源码：GetPrefix() + 字段名大写）：
///   前缀 = 命名空间第一段大写 + "-"   →  ARISTOCRAT-
///   字段 = Perk 大写                  →  PERK
/// 所以文本键是 ARISTOCRAT-PERK，对应 localization/{语言}/card_keywords.json。
/// </summary>
public class AristocratKeywords
{
    [CustomEnum(null)]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Perk;
}