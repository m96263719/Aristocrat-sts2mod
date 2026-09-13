using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Aristocrat.Cards;

/// <summary>
/// 大金库：失去 10 金币，然后选择 1 次：获得 2 点力量 / 获得 2 点敏捷 / 获得 [E][E]。升级 选择次数 +1。
///
/// "三选一"在塔2 里就是原生那个选牌大界面（CardSelectCmd.FromChooseACardScreen）——
/// 塞三张 0 费、打出来没效果的"选项牌"进去，玩家选哪张由我们自己判断。观者的许愿也是这么写的。
/// </summary>
public sealed class AristocratGreatVault : AristocratCard
{
    private const int Price = 10;
    private const int Boost = 2;

    public AristocratGreatVault()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("MagicNumber", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;
        await AristocratGold.SpendUpTo(Price, owner);

        int times = (int)DynamicVars["MagicNumber"].BaseValue;
        for (int i = 0; i < times; i++)
        {
            var combat = owner.Creature.CombatState;
            if (combat == null)
            {
                return;
            }

            List<CardModel> options =
            [
                combat.CreateCard<AristocratVaultStrength>(owner),
                combat.CreateCard<AristocratVaultDexterity>(owner),
                combat.CreateCard<AristocratVaultEnergy>(owner),
            ];

            CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                options,
                owner);

            switch (chosen)
            {
                case AristocratVaultStrength:
                    await PowerCmd.Apply<StrengthPower>(choiceContext, owner.Creature, Boost, owner.Creature, this);
                    break;
                case AristocratVaultDexterity:
                    await PowerCmd.Apply<DexterityPower>(choiceContext, owner.Creature, Boost, owner.Creature, this);
                    break;
                case AristocratVaultEnergy:
                    await PlayerCmd.GainEnergy(Boost, owner);
                    break;
                default:
                    return;
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy(1);
    }
}
