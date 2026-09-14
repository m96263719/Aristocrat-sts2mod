using System.Collections.Generic;
using Aristocrat.Cards;
using Aristocrat.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Aristocrat.Character;

/// <summary>
/// 贵族：塔1 的 aristocrat:ModdedCharacter 移植版。
///
/// 类名决定角色的内部 ID（Slugify("Aristocrat") = ARISTOCRAT），
/// 而 ID 又决定所有按约定查找的资源路径（character_icon_aristocrat.png 等），
/// 所以这个类名不能随便改。
///
/// 数据来自 CharacterFile.java：
///   80 血、999 起始金币、3 能量、0 充能球位、初始牌组 7 打击 + 7 防御 + 征收 + 血统、
///   初始遗物「家族纹章」。
/// </summary>
public sealed class Aristocrat : CharacterModel
{
    /// <summary>原 mod 的角色色：金色。</summary>
    public static readonly Color Gold = new("FFD142");

    public override Color NameColor => Gold;

    public override CharacterGender Gender => CharacterGender.Neutral;

    protected override CharacterModel? UnlocksAfterRunAs => null;

    public override int StartingHp => 80;

    public override int StartingGold => 999;

    public override float AttackAnimDelay => 0.15f;

    public override float CastAnimDelay => 0.25f;

    public override CardPoolModel CardPool => ModelDb.CardPool<AristocratCardPool>();

    /// <summary>
    /// 贵族的遗物池（自己的 11 件遗物；共享池那部分由 RelicGrabBag 自动拼上）。
    /// </summary>
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<AristocratRelicPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SharedPotionPool>();

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<AristocratStrike>(), ModelDb.Card<AristocratStrike>(), ModelDb.Card<AristocratStrike>(), ModelDb.Card<AristocratStrike>(),
        ModelDb.Card<AristocratStrike>(), ModelDb.Card<AristocratStrike>(), ModelDb.Card<AristocratStrike>(),
        ModelDb.Card<AristocratDefend>(), ModelDb.Card<AristocratDefend>(), ModelDb.Card<AristocratDefend>(), ModelDb.Card<AristocratDefend>(),
        ModelDb.Card<AristocratDefend>(), ModelDb.Card<AristocratDefend>(), ModelDb.Card<AristocratDefend>(),
        ModelDb.Card<AristocratTaxation>(),
        ModelDb.Card<AristocratLineage>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<FamilyCrest>()];

    public override Color EnergyLabelOutlineColor => new("6B4E12");

    public override Color DialogueColor => new("3A2E10");

    public override Color MapDrawingColor => Gold;

    public override Color RemoteTargetingLineColor => new("FFE08A");

    public override Color RemoteTargetingLineOutline => new("6B4E12");

    // 骨架阶段没有自己的语音，先静音并把切换音效指向铁甲战士的，避免缺资源报错
    public override string CharacterSelectSfx => "";

    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_attack_blunt",
    ];
}
