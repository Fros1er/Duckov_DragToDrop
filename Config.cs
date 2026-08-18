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
    public int anchoredPosX = 0;
    public int anchoredPosY = 60;
    public int fontSize = 24;
    public bool enableShiftLeftClick = true;
    public float alphaOnActive = 0.3f;
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
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load config from file: {e}, try load from ModConfig");
        }

        return new Config();
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
            case nameof(anchoredPosX):
            {
                ModBehaviour.Config.anchoredPosX =
                    ModConfigAPI.SafeLoad(ModName, nameof(anchoredPosX), ModBehaviour.Config.anchoredPosX);
                break;
            }
            case nameof(anchoredPosY):
            {
                ModBehaviour.Config.anchoredPosY =
                    ModConfigAPI.SafeLoad(ModName, nameof(anchoredPosY), ModBehaviour.Config.anchoredPosY);
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
            L.Get(L.Keys.ConfigSizeDeltaX),
            typeof(int),
            ModBehaviour.Config.sizeDeltaX,
            new Vector2(0, 2560)
        );
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            "sizeDeltaY",
            L.Get(L.Keys.ConfigSizeDeltaY),
            typeof(int),
            ModBehaviour.Config.sizeDeltaY,
            new Vector2(0, 1440)
        );
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            nameof(anchoredPosX),
            L.Get(L.Keys.ConfigAnchoredPosX),
            typeof(int),
            ModBehaviour.Config.anchoredPosX,
            new Vector2(-2560, 2560)
        );
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            nameof(anchoredPosY),
            L.Get(L.Keys.ConfigAnchoredPosY),
            typeof(int),
            ModBehaviour.Config.anchoredPosY,
            new Vector2(-1440, 1440)
        );
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            "fontSize",
            L.Get(L.Keys.ConfigFontSize),
            typeof(int),
            ModBehaviour.Config.fontSize,
            new Vector2(0, 128)
        );
        ModConfigAPI.SafeAddInputWithSlider(
            ModName,
            "alphaOnActive",
            L.Get(L.Keys.ConfigAlphaOnActive),
            typeof(float),
            ModBehaviour.Config.alphaOnActive,
            new Vector2(0, 1f)
        );
        ModConfigAPI.SafeAddBoolDropdownList(
            ModName,
            "enableShiftLeftClick",
            L.Get(L.Keys.ConfigEnableShiftLeftClick),
            ModBehaviour.Config.enableShiftLeftClick
        );
        var formatOptions = new SortedDictionary<string, object>
        {
            { L.Get(L.Keys.ConfigDropAtBaseDropUnconfigured), (int)DropAtBaseAction.DropUnconfigured },
            { L.Get(L.Keys.ConfigDropAtBaseDrop), (int)DropAtBaseAction.Drop },
            { L.Get(L.Keys.ConfigDropAtBaseSendToStorage), (int)DropAtBaseAction.SendToStorage },
            { L.Get(L.Keys.ConfigDropAtBaseSell), (int)DropAtBaseAction.Sell }
        };

        ModConfigAPI.SafeAddDropdownList(
            ModName,
            nameof(dropAtBaseAction),
            L.Get(L.Keys.ConfigDropAtBaseAction),
            formatOptions,
            typeof(int),
            (int)ModBehaviour.Config.dropAtBaseAction
        );
        _hasSetup = true;
    }
}