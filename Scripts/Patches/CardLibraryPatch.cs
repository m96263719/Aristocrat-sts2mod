using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Runs;
using Aristocrat.Character;

namespace Aristocrat.Patches;

/// <summary>
/// 卡牌总览（百科 → 卡牌）对 mod 角色的兜底。
///
/// NCardLibrary 里那张"角色 → 筛选按钮"的表是写死的五个本体角色，
/// 打开界面时会拿当前角色去查表（_cardPoolFilters[character]），
/// mod 角色查不到就抛 KeyNotFoundException，整个界面初始化失败——表现就是一片黑屏。
///
/// 处理办法：给贵族复制一个筛选按钮（照着本体的铁甲战士那个节点），
/// 注册进两张表，并把按钮图标换成贵族自己的。
/// 万一 UI 那边出问题，至少还有兜底，不会黑屏。
/// </summary>
internal static class CardLibraryPatch
{
    private static readonly System.Reflection.FieldInfo? FiltersField =
        AccessTools.Field(typeof(NCardLibrary), "_cardPoolFilters");

    private static readonly FieldInfo? PoolFiltersField =
        AccessTools.Field(typeof(NCardLibrary), "_poolFilters");

    private static readonly FieldInfo? IroncladFilterField =
        AccessTools.Field(typeof(NCardLibrary), "_ironcladFilter");

    private static readonly MethodInfo? UpdateFilterMethod =
        AccessTools.Method(typeof(NCardLibrary), "UpdateCardPoolFilter");

    private static NCardPoolFilter? _ourFilter;

    /// <summary>Create() 造完原生筛选按钮之后，插一个贵族的进去。</summary>
    [HarmonyPatch(typeof(NCardLibrary), "Create")]
    [HarmonyPostfix]
    private static void CreatePostfix(NCardLibrary __result)
    {
        try
        {
            AddFilter(__result);
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 卡牌总览：添加贵族筛选按钮失败：{ex}");
        }
    }

    /// <summary>原版会按"角色是否解锁"来显示筛选按钮，这里把贵族那栏强制显示出来。</summary>
    [HarmonyPatch(typeof(NCardLibrary), "_Ready")]
    [HarmonyPostfix]
    private static void ReadyPostfix(NCardLibrary __instance)
    {
        if (_ourFilter != null && GodotObject.IsInstanceValid(_ourFilter))
        {
            _ourFilter.Visible = true;
        }
    }

    private static void AddFilter(NCardLibrary library)
    {
        // 已交给 BaseLib：改成继承 CustomCharacterModel 之后，BaseLib 会像对待观者那样
        // 自动给贵族生成筛选按钮。这里自己复制节点反而会和它的插入逻辑打架（观者那栏就是这么坏的），
        // 所以关掉，只保留下面那个"查表兜底"，防止万一没注册上时黑屏。
        if (true)
        {
            return;
        }

        if (IroncladFilterField?.GetValue(library) is not NCardPoolFilter template)
        {
            Log.Error("[Aristocrat] 卡牌总览：找不到本体筛选按钮，跳过。");
            return;
        }

        if (PoolFiltersField?.GetValue(library) is not Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters)
        {
            Log.Error("[Aristocrat] 卡牌总览：拿不到卡池筛选表，跳过。");
            return;
        }

        if (FiltersField?.GetValue(library) is not IDictionary cardPoolFilters)
        {
            Log.Error("[Aristocrat] 卡牌总览：拿不到角色筛选表，跳过。");
            return;
        }

        var node = template.Duplicate() as NCardPoolFilter;
        if (node == null)
        {
            Log.Error("[Aristocrat] 卡牌总览：复制筛选按钮失败，跳过。");
            return;
        }

        node.Name = "AristocratPool";
        node.Loc = new LocString("characters", "ARISTOCRAT.title");
        ReplaceIcon(node);

        Node? parent = template.GetParent();
        if (parent is Container)
        {
            // 是容器的话自动排布，直接加进去就行
            parent.AddChild(node);
        }
        else
        {
            // 绝对定位的布局：贴着铁甲战士那个按钮往下放
            parent?.AddChild(node);
            node.Position = template.Position + new Vector2(0f, template.Size.Y + 6f);
        }

        // 点它的时候要跟本体按钮一样切卡池
        if (UpdateFilterMethod is MethodInfo method)
        {
            node.Toggled += filter => method.Invoke(library, [filter]);
        }

        poolFilters[node] = card => card.Pool is AristocratCardPool;
        cardPoolFilters[ModelDb.Character<Aristocrat.Character.Aristocrat>()] = node;
        _ourFilter = node;

        Log.Info($"[Aristocrat] 卡牌总览：已添加贵族筛选按钮（父节点 {parent?.GetType().Name}）。");
    }

    /// <summary>把按钮里的图标换成贵族自己的角色头像。</summary>
    private static void ReplaceIcon(NCardPoolFilter node)
    {
        try
        {
            Texture2D? texture = GD.Load<Texture2D>("res://images/ui/top_panel/character_icon_aristocrat.png");
            if (texture == null)
            {
                return;
            }

            foreach (Node child in node.FindChildren("*", "TextureRect", true, false))
            {
                if (child is TextureRect rect)
                {
                    rect.Texture = texture;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Aristocrat] 卡牌总览：换图标失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 兜底：万一贵族还是没在表里（例如添加按钮那步失败），
    /// 就临时借一个现有筛选器顶上，至少不会因为查表抛异常而黑屏。
    /// 只对贵族自己生效，不去动别的 mod 角色（之前那样会干扰观者等 mod 的自己的注册逻辑）。
    /// </summary>
    [HarmonyPatch(typeof(NCardLibrary), "OnSubmenuOpened")]
    internal static class Fallback
    {
        [HarmonyPrefix]
        private static void Prefix(NCardLibrary __instance)
        {
            try
            {
                if (FiltersField?.GetValue(__instance) is not IDictionary filters || filters.Count == 0)
                {
                    return;
                }

                var runState = RunManager.Instance?.DebugOnlyGetState();
                Player? me = runState?.Players.FirstOrDefault();
                CharacterModel? character = me?.Character;

                if (character is not Aristocrat.Character.Aristocrat || filters.Contains(character))
                {
                    return;
                }

                object fallback = filters.Values.Cast<object>().First();
                filters[character] = fallback;
                Log.Info("[Aristocrat] 卡牌总览：贵族没有自己的筛选按钮，先借用现有筛选器。");
            }
            catch (Exception ex)
            {
                Log.Error($"[Aristocrat] 卡牌总览兜底失败：{ex}");
            }
        }
    }
}
