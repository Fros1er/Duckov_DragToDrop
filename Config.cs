using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static DragToDrop.ModBehaviour;

namespace DragToDrop;

[Serializable]
public class Config
{
    public enum DropAtBaseAction
    {
        DropUnconfigured = 0,
        Drop = 1,
        SendToStorage = 2,
        Sell = 3
    }

    public int sizeDeltaX = 927;
    public int sizeDeltaY = 896;
    public int fontSize = 24;
    public bool enableShiftLeftClick = true;
    public float alphaOnActive = 0.5f;
    public DropAtBaseAction dropAtBaseAction = DropAtBaseAction.DropUnconfigured;

    private static bool _hasSetup;

    private static string PersistentConfigPath => Path.Combine(Application.streamingAssetsPath, "DragToDropConfig.txt");

    public static Config LoadConfig()
    {
        try
        {
            if (File.Exists(PersistentConfigPath))
            {
                string json = File.ReadAllText(PersistentConfigPath);
                var config = JsonUtility.FromJson<Config>(json);
                return config;
            }

            Log("Config file not found, try load from ModConfig");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load config from file: {e}, try load from ModConfig");
        }

        return new Config
        {
            sizeDeltaX = ModConfigAPI.SafeLoad(ModName, "sizeDeltaX", ModBehaviour.Config.sizeDeltaX),
            sizeDeltaY = ModConfigAPI.SafeLoad(ModName, "sizeDeltaY", ModBehaviour.Config.sizeDeltaY),
            fontSize = ModConfigAPI.SafeLoad(ModName, "fontSize", ModBehaviour.Config.fontSize),
            enableShiftLeftClick = ModConfigAPI.SafeLoad(ModName, "enableShiftLeftClick",
                ModBehaviour.Config.enableShiftLeftClick)
        };
    }

    private static void SaveConfig(Config config)
    {
        try
        {
            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(PersistentConfigPath, json);
            // Log("Config saved");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save config: {e}");
        }
    }

    private static void LoadConfigFromModConfig(string key)
    {
        // Log($"Key: {key}");
        switch (key)
        {
            case "sizeDeltaX":
            {
                ModBehaviour.Config.sizeDeltaX =
                    ModConfigAPI.SafeLoad(ModName, "sizeDeltaX", ModBehaviour.Config.sizeDeltaX);
                break;
            }
            case "sizeDeltaY":
            {
                ModBehaviour.Config.sizeDeltaY =
                    ModConfigAPI.SafeLoad(ModName, "sizeDeltaY", ModBehaviour.Config.sizeDeltaY);
                break;
            }
            case "fontSize":
            {
                ModBehaviour.Config.fontSize = ModConfigAPI.SafeLoad(ModName, "fontSize", ModBehaviour.Config.fontSize);
                break;
            }
            case "enableShiftLeftClick":
            {
                ModBehaviour.Config.enableShiftLeftClick = ModConfigAPI.SafeLoad(ModName, "enableShiftLeftClick",
                    ModBehaviour.Config.enableShiftLeftClick);
                break;
            }
            case "alphaOnActive":
            {
                ModBehaviour.Config.alphaOnActive =
                    ModConfigAPI.SafeLoad(ModName, "alphaOnActive", ModBehaviour.Config.alphaOnActive);
                break;
            }
            case nameof(dropAtBaseAction):
            {
                ModBehaviour.Config.dropAtBaseAction =
                    (DropAtBaseAction)ModConfigAPI.SafeLoad(ModName, nameof(dropAtBaseAction),
                        (int)ModBehaviour.Config.dropAtBaseAction);
                break;
            }
        }
    }

    private static void OnModConfigOptionsChanged(string key)
    {
        if (!key.StartsWith(ModName + "_"))
            return;
        LoadConfigFromModConfig(key.Substring(ModName.Length + 1));
        SaveConfig(ModBehaviour.Config);
        SetDiscardAreaStyle();
        // Log($"ModConfig updated - {key}, {JsonUtility.ToJson(ModBehaviour.Config, true)}");
    }

    public static void SetupModConfig()
    {
        if (_hasSetup)
        {
            return;
        }

        if (!ModConfigAPI.IsAvailable())
        {
            Log("ModConfig not available");
            return;
        }

        ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);

        // 2560×1440
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            "sizeDeltaX",
            "丢弃区域宽度（初始为927）",
            typeof(int),
            ModBehaviour.Config.fontSize,
            new Vector2(0, 2560)
        );
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            "sizeDeltaY",
            "丢弃区域高度（初始为896）",
            typeof(int),
            ModBehaviour.Config.fontSize,
            new Vector2(0, 1440)
        );
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            "fontSize",
            "字体大小（初始为24）",
            typeof(int),
            ModBehaviour.Config.fontSize,
            new Vector2(0, 128)
        );
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            "alphaOnActive",
            "丢弃区域透明度（初始为0.3）",
            typeof(float),
            ModBehaviour.Config.alphaOnActive,
            new Vector2(0, 1f)
        );
        ModConfigAPI.SafeAddBoolDropdownList(
            ModName,
            "enableShiftLeftClick",
            "启用Shift+左键=双击",
            ModBehaviour.Config.enableShiftLeftClick
        );
        var formatOptions = new SortedDictionary<string, object>
        {
            { "丢弃到脚下", (int)DropAtBaseAction.DropUnconfigured },
            { "丢弃到脚下（关闭区域中的设置提示）", (int)DropAtBaseAction.Drop },
            { "放入仓库", (int)DropAtBaseAction.SendToStorage },
            { "以所有商人里价格最好的出售（广告：为避免误操作，建议订阅物品回购mod）", (int)DropAtBaseAction.Sell }
        };

        ModConfigAPI.SafeAddDropdownList(
            ModName,
            nameof(dropAtBaseAction),
            "在基地丢弃物品时的行为",
            formatOptions,
            typeof(int),
            (int)ModBehaviour.Config.dropAtBaseAction
        );
        _hasSetup = true;
    }
}