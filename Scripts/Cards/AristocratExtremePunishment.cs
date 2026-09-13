using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 极刑（2 费，攻击，稀有）：对所有敌人造成 10 点伤害，然后自动打出抽牌堆里所有打击（目标随机）。升级 +4。
/// 对应塔1 的 aristocrat:ExtremePunishment。
/// </summary>
public sealed class AristocratExtremePunishment : AristocratCard
{
    public AristocratExtremePunishment()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(owner.Creature.CombatState!)
            .Execute(choiceContext);

        CardPile drawPile = PileType.Draw.GetPile(owner);
        foreach (CardModel card in drawPile.Cards.ToList())
        {
            if (!IsStrike(card) || !drawPile.Cards.Contains(card))
            {
                continue;
            }

            await CardCmd.AutoPlay(choiceContext, card, PickRandomEnemy(owner), AutoPlayType.Default);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}