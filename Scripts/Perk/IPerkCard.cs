using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Aristocrat.Perk;

/// <summary>
/// 「特典」：从商店买下这张牌时触发的额外效果。
///
/// 塔1 那边是维护一张写死的卡牌 ID 清单（PerkHelper.hasNativePerk），
/// 然后在 ShopScreen.purchaseCard 上挂钩子。塔2 这边改用接口：
/// 谁有特典谁实现这个接口，购买钩子统一分发，不用维护清单。
/// </summary>
public interface IPerkCard
{
    /// <summary>被打包买走时触发。</summary>
    Task OnPerkTriggered(Player owner);
}