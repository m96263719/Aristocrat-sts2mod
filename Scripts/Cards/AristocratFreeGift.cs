using System;
using System.Threading.Tasks;
using Aristocrat.Perk;
using Aristocrat.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;

namespace Aristocrat.Cards;

/// <summary>
/// 赠品：每当你使用一张有特典的牌，抽 1 张牌。升级 费用 2 → 1。
/// 特典：购买时换一间全新的商店（整间店的商品都会重新生成，等价于把已售罄的补满）。
///
/// 一开始想直接给卖掉的格子补货，但塔2 的补货入口是 protected、而且每种子类实现还不一样，
/// 反射调不通（抽象方法没法 Invoke）。改成"重新进一次同一节点的商店"——
/// 新商店是一个全新的 MerchantRoom，库存自然全满，这也正是家族纹章那套开商店用的路子。
/// </summary>
public sealed class AristocratFreeGift : AristocratCard, IPerkCard
{
    public AristocratFreeGift()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await PowerCmd.Apply<FreeGiftPower>(
            choiceContext,
            owner.Creature,
            DynamicVars["MagicNumber"].BaseValue,
            owner.Creature,
            this);
    }

    public async Task OnPerkTriggered(Player owner)
    {
        RunManager? manager = RunManager.Instance;
        RunState? state = manager?.DebugOnlyGetState();
        if (manager == null || state == null)
        {
            return;
        }

        try
        {
            Log.Info("[Aristocrat] 赠品：特典触发，重开一间商店。");
            await manager.EnterMapPointInternal(
                state.ActFloor,
                MapPointType.Shop,
                null,
                saveGame: false);
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 赠品：重开商店失败：{ex}");
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
