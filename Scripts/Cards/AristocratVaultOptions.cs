using System.Collections.Generic;
using System.Threading.Tasks;
using Aristocrat.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Aristocrat.Cards;

/// <summary>
/// 大金库的三张"选项牌"（力量 / 敏捷 / 能量）。它们只是给原生的三选一界面用来显示的，
/// 打出来什么都不做——真正的效果由大金库自己判断选中的是哪张再执行。
/// 这正是观者 mod 的许愿用的那套做法（WatcherWishAlmighty 那三张）。
/// </summary>
public abstract class AristocratVaultOption : CardModel
{
    protected AristocratVaultOption()
        : base(-1, CardType.Skill, CardRarity.Event, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    public override CardPoolModel Pool => ModelDb.CardPool<AristocratCardPool>();

    public override string PortraitPath => "res://Aristocrat/images/cards/AristocratGreatVault.png";

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }
}

public sealed class AristocratVaultStrength : AristocratVaultOption
{
}

public sealed class AristocratVaultDexterity : AristocratVaultOption
{
}

public sealed class AristocratVaultEnergy : AristocratVaultOption
{
}
