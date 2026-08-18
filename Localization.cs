using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DragToDrop;

public static class L
{
    public static class Keys
    {
        public const string DiscardDrop = "DragToDrop.Discard.Drop";
        public const string DiscardDropUnconfigured = "DragToDrop.Discard.DropUnconfigured";
        public const string DiscardSendToStorage = "DragToDrop.Discard.SendToStorage";
        public const string DiscardSell = "DragToDrop.Discard.Sell";

        public const string ConfigSizeDeltaX = "DragToDrop.Config.SizeDeltaX";
        public const string ConfigSizeDeltaY = "DragToDrop.Config.SizeDeltaY";
        public const string ConfigAnchoredPosX = "DragToDrop.Config.AnchoredPosX";
        public const string ConfigAnchoredPosY = "DragToDrop.Config.AnchoredPosY";
        public const string ConfigFontSize = "DragToDrop.Config.FontSize";
        public const string ConfigAlphaOnActive = "DragToDrop.Config.AlphaOnActive";
        public const string ConfigEnableShiftLeftClick = "DragToDrop.Config.EnableShiftLeftClick";
        public const string ConfigDropAtBaseAction = "DragToDrop.Config.DropAtBaseAction";
        public const string ConfigDropAtBaseDropUnconfigured = "DragToDrop.Config.DropAtBase.DropUnconfigured";
        public const string ConfigDropAtBaseDrop = "DragToDrop.Config.DropAtBase.Drop";
        public const string ConfigDropAtBaseSendToStorage = "DragToDrop.Config.DropAtBase.SendToStorage";
        public const string ConfigDropAtBaseSell = "DragToDrop.Config.DropAtBase.Sell";
    }

    private static readonly Dictionary<SystemLanguage, Dictionary<string, string>> Tables =
        new()
        {
            [SystemLanguage.English] = new Dictionary<string, string>
            {
                [Keys.DiscardDrop] = "Drop Item",
                [Keys.DiscardDropUnconfigured] =
                    "Drop Item\nAdjust base drop behavior in settings",
                [Keys.DiscardSendToStorage] = "Return to Storage",
                [Keys.DiscardSell] = "Sell",
                [Keys.ConfigSizeDeltaX] = "Discard area width (default: 927)",
                [Keys.ConfigSizeDeltaY] = "Discard area height (default: 896)",
                [Keys.ConfigAnchoredPosX] = "Discard area horizontal offset (default: 0)",
                [Keys.ConfigAnchoredPosY] = "Discard area vertical offset (default: 60)",
                [Keys.ConfigFontSize] = "Font size (default: 24)",
                [Keys.ConfigAlphaOnActive] = "Discard area opacity (default: 0.3)",
                [Keys.ConfigEnableShiftLeftClick] = "Enable Shift+Left Click = Double Click",
                [Keys.ConfigDropAtBaseAction] = "Behavior when discarding items at base",
                [Keys.ConfigDropAtBaseDropUnconfigured] = "Drop at feet",
                [Keys.ConfigDropAtBaseDrop] = "Drop at feet (hide settings hint in area)",
                [Keys.ConfigDropAtBaseSendToStorage] = "Send to storage",
                [Keys.ConfigDropAtBaseSell] =
                    "Sell at best merchant price (tip: subscribe to buyback mod to avoid mistakes)"
            },
            [SystemLanguage.Japanese] = new Dictionary<string, string>
            {
                [Keys.DiscardDrop] = "アイテムを捨てる",
                [Keys.DiscardDropUnconfigured] =
                    "アイテムを捨てる\n設定で変更可",
                [Keys.DiscardSendToStorage] = "倉庫に戻す",
                [Keys.DiscardSell] = "売却",
                [Keys.ConfigSizeDeltaX] = "捨て区域の幅（初期値: 927）",
                [Keys.ConfigSizeDeltaY] = "捨て区域の高さ（初期値: 896）",
                [Keys.ConfigAnchoredPosX] = "捨て区域の横方向オフセット（初期値: 0）",
                [Keys.ConfigAnchoredPosY] = "捨て区域の縦方向オフセット（初期値: 60）",
                [Keys.ConfigFontSize] = "フォントサイズ（初期値: 24）",
                [Keys.ConfigAlphaOnActive] = "捨て区域の透明度（初期値: 0.3）",
                [Keys.ConfigEnableShiftLeftClick] = "Shift+左クリック = ダブルクリックを有効化",
                [Keys.ConfigDropAtBaseAction] = "基地でアイテムを捨てるときの動作",
                [Keys.ConfigDropAtBaseDropUnconfigured] = "足元に捨てる",
                [Keys.ConfigDropAtBaseDrop] = "足元に捨てる（区域の設定ヒントを非表示）",
                [Keys.ConfigDropAtBaseSendToStorage] = "倉庫に入れる",
                [Keys.ConfigDropAtBaseSell] =
                    "最も高い価格の商人に売却（誤操作防止のため買い戻しmodの導入を推奨）"
            },
            [SystemLanguage.ChineseSimplified] = new Dictionary<string, string>
            {
                [Keys.DiscardDrop] = "丢弃物品",
                [Keys.DiscardDropUnconfigured] = "丢弃物品\n设置中可以调整在仓库中丢弃物品时的行为",
                [Keys.DiscardSendToStorage] = "放回仓库",
                [Keys.DiscardSell] = "出售",
                [Keys.ConfigSizeDeltaX] = "丢弃区域宽度（初始为927）",
                [Keys.ConfigSizeDeltaY] = "丢弃区域高度（初始为896）",
                [Keys.ConfigAnchoredPosX] = "丢弃区域横向偏移（初始为0）",
                [Keys.ConfigAnchoredPosY] = "丢弃区域纵向偏移（初始为60）",
                [Keys.ConfigFontSize] = "字体大小（初始为24）",
                [Keys.ConfigAlphaOnActive] = "丢弃区域透明度（初始为0.3）",
                [Keys.ConfigEnableShiftLeftClick] = "启用Shift+左键=双击",
                [Keys.ConfigDropAtBaseAction] = "在基地丢弃物品时的行为",
                [Keys.ConfigDropAtBaseDropUnconfigured] = "丢弃到脚下",
                [Keys.ConfigDropAtBaseDrop] = "丢弃到脚下（关闭区域中的设置提示）",
                [Keys.ConfigDropAtBaseSendToStorage] = "放入仓库",
                [Keys.ConfigDropAtBaseSell] =
                    "以所有商人里价格最好的出售（广告：为避免误操作，建议订阅物品回购mod）"
            }
        };

    private static Action? _onLanguageChanged;
    private static bool _initialized;
    private static SystemLanguage _currentLanguage = SystemLanguage.English;

    public static SystemLanguage CurrentLanguage => _currentLanguage;

    public static void Initialize(Action onLanguageChanged)
    {
        if (_initialized)
        {
            return;
        }

        _onLanguageChanged = onLanguageChanged;
        _currentLanguage = DetectLanguage();
        SubscribeLanguageChanged();
        _initialized = true;
    }

    public static string Get(string key)
    {
        if (Tables.TryGetValue(_currentLanguage, out var table) &&
            table.TryGetValue(key, out var value))
        {
            return value;
        }

        if (_currentLanguage != SystemLanguage.English &&
            Tables.TryGetValue(SystemLanguage.English, out var englishTable) &&
            englishTable.TryGetValue(key, out var englishValue))
        {
            return englishValue;
        }

        return key;
    }

    private static SystemLanguage DetectLanguage()
    {
        SystemLanguage? gameLanguage = TryGetGameLanguage();
        if (gameLanguage.HasValue)
        {
            return NormalizeLanguage(gameLanguage.Value);
        }

        return NormalizeLanguage(Application.systemLanguage);
    }

    private static SystemLanguage NormalizeLanguage(SystemLanguage language)
    {
        return language switch
        {
            SystemLanguage.Japanese => SystemLanguage.Japanese,
            SystemLanguage.Chinese or SystemLanguage.ChineseSimplified or SystemLanguage.ChineseTraditional =>
                SystemLanguage.ChineseSimplified,
            _ => SystemLanguage.English
        };
    }

    private static SystemLanguage? TryGetGameLanguage()
    {
        try
        {
            Type? managerType = Type.GetType("SodaCraft.Localizations.LocalizationManager, TeamSoda.Localization");
            if (managerType == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    managerType = assembly.GetType("SodaCraft.Localizations.LocalizationManager");
                    if (managerType != null)
                    {
                        break;
                    }
                }
            }

            if (managerType == null)
            {
                return null;
            }

            foreach (var memberName in new[] { "CurrentLanguage", "Language", "currentLanguage" })
            {
                PropertyInfo? property = managerType.GetProperty(memberName,
                    BindingFlags.Public | BindingFlags.Static);
                if (property?.PropertyType == typeof(SystemLanguage))
                {
                    return (SystemLanguage)property.GetValue(null)!;
                }

                FieldInfo? field = managerType.GetField(memberName,
                    BindingFlags.Public | BindingFlags.Static);
                if (field?.FieldType == typeof(SystemLanguage))
                {
                    return (SystemLanguage)field.GetValue(null)!;
                }
            }
        }
        catch (Exception e)
        {
            ModBehaviour.Log($"Failed to detect game language: {e.Message}");
        }

        return null;
    }

    private static void SubscribeLanguageChanged()
    {
        try
        {
            Type? managerType = Type.GetType("SodaCraft.Localizations.LocalizationManager, TeamSoda.Localization");
            if (managerType == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    managerType = assembly.GetType("SodaCraft.Localizations.LocalizationManager");
                    if (managerType != null)
                    {
                        break;
                    }
                }
            }

            if (managerType == null)
            {
                return;
            }

            EventInfo? languageChangedEvent = managerType.GetEvent("OnSetLanguage",
                BindingFlags.Public | BindingFlags.Static);
            MethodInfo? addMethod = languageChangedEvent?.GetAddMethod(false);
            if (addMethod == null)
            {
                return;
            }

            Type actionType = typeof(Action<>).MakeGenericType(typeof(SystemLanguage));
            Delegate handler = Delegate.CreateDelegate(actionType, typeof(L), nameof(OnGameLanguageChanged));
            addMethod.Invoke(null, new object[] { handler });
        }
        catch (Exception e)
        {
            ModBehaviour.Log($"Failed to subscribe language change: {e.Message}");
        }
    }

    private static void OnGameLanguageChanged(SystemLanguage language)
    {
        _currentLanguage = NormalizeLanguage(language);
        _onLanguageChanged?.Invoke();
    }
}
