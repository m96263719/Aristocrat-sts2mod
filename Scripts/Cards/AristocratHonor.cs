using System.Collections.Generic;
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
/// 名誉（0 费，攻击，普通）：造成 12 点伤害。只有手牌里没有"打击以外的攻击牌"时才能打出。升级 +4。
/// 对应塔1 的 aristocrat:Honor（那边重写 canUse）。
/// </summary>
public sealed class AristocratHonor : AristocratCard
{
    public AristocratHonor()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];

    protected override bool IsPlayable
    {
        get
        {
            Player? owner = Owner;
            if (owner == null)
            {
                return true;
            }

            foreach (CardModel card in PileType.Hand.GetPile(owner).Cards)
            {
                if (ReferenceEquals(card, this) || card.Type != CardType.Attack || IsStrike(card))
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}