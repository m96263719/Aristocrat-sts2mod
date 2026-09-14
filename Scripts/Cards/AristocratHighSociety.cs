using System.Threading.Tasks;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Cards;

/// <summary>
/// 上流社交界（2 费，能力，稀有）：本场战斗中，你的打击与防御获得消耗；
/// 每当一张打击或防御被消耗，将一张随机稀有牌加入你的手牌。升级后费用降为 1。
/// 对应塔1 的 aristocrat:HighSociety。
/// </summary>
public sealed class AristocratHighSociety : AristocratCard
{
    public AristocratHighSociety()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        // [消耗] 由 HighSocietyPower 用动态关键词给（新生成 / 变化出来的打防也能吃到）
        await PowerCmd.Apply<HighSocietyPower>(choiceContext, owner.Creature, 1m, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
