using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Aristocrat.Powers;

/// <summary>
/// 赞助人：每回合开始时付 5 金币，换 Amount 点力量与敏捷。金币不够 5 就什么都不发生。
/// </summary>
public sealed class PatronPower : PowerModel
{
    private const int Price = 5;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || player.Gold < Price)
        {
            return;
        }

        Flash();
        await PlayerCmd.LoseGold(Price, player, GoldLossType.Spent);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
