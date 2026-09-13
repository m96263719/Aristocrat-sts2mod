using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Aristocrat.Cards;

/// <summary>
/// 重铸剑刃：造成 6 伤；如果金币够 10，就失去 10 金币，并让这张牌永久多造成 1 点伤害。升级 成长 +1。
///
/// 塔1 是直接改 masterDeck 里那张牌的 baseDamage。塔2 这边照抄本体「巨镰」(TheScythe) 的写法：
///   1. 用 [SavedProperty] 存 CurrentDamage / IncreasedDamage，存档时跟着卡一起走；
///   2. DamageVar 用 CurrentDamage 构造，CurrentDamage 的 setter 顺手把卡面数值同步过去；
///   3. 打出时把成长同时加到战斗中的自己身上，和"牌组里的那张原件"（DeckVersion）上。
/// 之前我用 CloneOf 找原件，那个指针在战斗卡上并不指向牌组牌，所以只在当场生效。
/// </summary>
public sealed class AristocratReforgedBlade : AristocratCard
{
    private const int Price = 10;

    private const int BaseDamage = 6;

    private int _currentDamage = BaseDamage;

    private int _increasedDamage;

    [SavedProperty]
    public int CurrentDamage
    {
        get => _currentDamage;
        set
        {
            AssertMutable();
            _currentDamage = value;
            DynamicVars.Damage.BaseValue = value;
        }
    }

    [SavedProperty]
    public int IncreasedDamage
    {
        get => _increasedDamage;
        set
        {
            AssertMutable();
            _increasedDamage = value;
        }
    }

    public AristocratReforgedBlade()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(CurrentDamage, ValueProp.Move),
        new IntVar("Increase", 1m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = Owner!;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 钱不够 10 就一分不花：不能让玩家花了 1-9 金币却什么都得不到
        if (owner.Gold >= Price)
        {
            await AristocratGold.SpendUpTo(Price, owner);
            int increase = DynamicVars["Increase"].IntValue;
            Forge(increase);
            (DeckVersion as AristocratReforgedBlade)?.Forge(increase);
        }
    }

    private void Forge(int amount)
    {
        IncreasedDamage += amount;
        UpdateDamage();
    }

    private void UpdateDamage()
    {
        CurrentDamage = BaseDamage + IncreasedDamage;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(1);
    }
}
