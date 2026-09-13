using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Aristocrat.Cards;

/// <summary>
/// 请带走（1 费，攻击，普通）：最多失去 30 金币，造成等量伤害。升级费用 0。
/// 对应塔1 的 aristocrat:EscortOut。塔1 用的是荆棘伤害（不吃力量），塔2 用 Unpowered 表示同样的东西。
/// </summary>
public sealed class AristocratEscortOut : AristocratCard
{
    private const int MaxGold = 30;

    public AristocratEscortOut()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        int spent = await AristocratGold.SpendUpTo(MaxGold, owner);
        if (spent <= 0 || cardPlay.Target is not { } target)
        {
            return;
        }

        await DamageCmd.Attack(spent)
            .FromCard(this)
            .Targeting(target)
            .Unpowered()
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
