using Aristocrat.Cards;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Character;

/// <summary>
/// 贵族的卡池。Title / EnergyColorName 决定卡框、能量球等资源的查找名，
/// 对应资源按游戏约定放在 res:// 下（见工程里的 images/ 与 materials/ 目录）。
/// </summary>
public sealed class AristocratCardPool : CardPoolModel
{
    public override string Title => "aristocrat";

    public override string EnergyColorName => "aristocrat";

    public override string CardFrameMaterialPath => "card_frame_aristocrat";

    public override Color DeckEntryCardColor => Aristocrat.Gold;

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards() =>
    [
        // 基础
        ModelDb.Card<AristocratStrike>(),
        ModelDb.Card<AristocratDefend>(),
        ModelDb.Card<AristocratTaxation>(),
        ModelDb.Card<AristocratLineage>(),

        // 普通
        ModelDb.Card<AristocratRadiance>(),
        ModelDb.Card<AristocratExploitation>(),
        ModelDb.Card<AristocratSpur>(),
        ModelDb.Card<AristocratClassic>(),
        ModelDb.Card<AristocratExtravagance>(),
        ModelDb.Card<AristocratCalling>(),
        ModelDb.Card<AristocratEdict>(),
        ModelDb.Card<AristocratHonor>(),
        ModelDb.Card<AristocratAttendant>(),
        ModelDb.Card<AristocratMajesty>(),
        ModelDb.Card<AristocratHitAndCut>(),
        ModelDb.Card<AristocratStrikeForStrike>(),
        ModelDb.Card<AristocratEscortOut>(),
        ModelDb.Card<AristocratTeaTimeInvitation>(),
        ModelDb.Card<AristocratPeasants>(),
        ModelDb.Card<AristocratRefinement>(),
        ModelDb.Card<AristocratParry>(),
        ModelDb.Card<AristocratBestow>(),
        ModelDb.Card<AristocratLuxuryGoods>(),
        ModelDb.Card<AristocratPureBlood>(),

        // 罕见
        ModelDb.Card<AristocratSuperhuman>(),
        ModelDb.Card<AristocratVassals>(),
        ModelDb.Card<AristocratIronBlood>(),
        ModelDb.Card<AristocratVersatile>(),
        ModelDb.Card<AristocratEscort>(),
        ModelDb.Card<AristocratInspection>(),
        ModelDb.Card<AristocratRampart>(),
        ModelDb.Card<AristocratFamilyTree>(),
        ModelDb.Card<AristocratCommand>(),
        ModelDb.Card<AristocratReform>(),
        ModelDb.Card<AristocratCoolHeadedBlow>(),
        ModelDb.Card<AristocratSelection>(),
        ModelDb.Card<AristocratDignity>(),
        ModelDb.Card<AristocratProtocol>(),
        ModelDb.Card<AristocratAncestralBlood>(),
        ModelDb.Card<AristocratExtremePunishment>(),
        ModelDb.Card<AristocratGhostAristocrat>(),
        ModelDb.Card<AristocratGoldenBlood>(),
        ModelDb.Card<AristocratPatron>(),
        ModelDb.Card<AristocratVanity>(),
        ModelDb.Card<AristocratCollection>(),
        ModelDb.Card<AristocratInheritanceTax>(),
        ModelDb.Card<AristocratTreasuredSword>(),
        ModelDb.Card<AristocratFormalWear>(),
        ModelDb.Card<AristocratProof>(),
        ModelDb.Card<AristocratBlueBlooded>(),
        ModelDb.Card<AristocratPrivilege>(),
        ModelDb.Card<AristocratLightAndSalt>(),
        ModelDb.Card<AristocratLuxuryDining>(),
        ModelDb.Card<AristocratPardon>(),
        ModelDb.Card<AristocratMasterStrike>(),
        ModelDb.Card<AristocratLastResort>(),

        // 先古
        ModelDb.Card<AristocratMandatoryTaxation>(),
        ModelDb.Card<AristocratAncestralPrecepts>(),

        // 稀有
        ModelDb.Card<AristocratMandate>(),
        ModelDb.Card<AristocratGrant>(),
        ModelDb.Card<AristocratGreatLineage>(),
        ModelDb.Card<AristocratSilkCurtain>(),
        ModelDb.Card<AristocratHighSociety>(),
        ModelDb.Card<AristocratCoronation>(),
        ModelDb.Card<AristocratBillionaire>(),
        ModelDb.Card<AristocratGoldenArmor>(),
        ModelDb.Card<AristocratImmunityPrivilege>(),
        ModelDb.Card<AristocratLaw>(),
        ModelDb.Card<AristocratTrueAncestor>(),
        ModelDb.Card<AristocratBornRuler>(),
        ModelDb.Card<AristocratGoldenHand>(),
        ModelDb.Card<AristocratPrideForm>(),
        ModelDb.Card<AristocratRecoveredProperty>(),
        ModelDb.Card<AristocratFreeGift>(),
        ModelDb.Card<AristocratGreatVault>(),
        ModelDb.Card<AristocratPortrait>(),
        ModelDb.Card<AristocratReforgedBlade>(),
        ModelDb.Card<AristocratRoyalGuard>(),
    ];
}
