using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Duckov.Economy;
using Duckov.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace DragToDrop;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    public const string ModName = "DragToDrop";
    public static Config Config = new Config();

    private static GameObject _discardArea;
    private static Image _discardAreaImage;
    private static GameObject _discardAreaTextObject;
    private static Text _discardAreaText;
    private static Canvas _canvas;

    public static void Log(object message)
    {
        Debug.Log($"[DragToDrop] {message}");
    }

    public static void SetDiscardAreaStyle()
    {
        if (_canvas == null || _discardAreaTextObject == null || _discardAreaText == null)
        {
            return;
        }

        float scale = _canvas.scaleFactor;
        var sizeDelta = new Vector2(Config.sizeDeltaX, Config.sizeDeltaY) * scale;
        var anchoredPosition = new Vector2(Config.anchoredPosX, Config.anchoredPosY);
        _discardArea.GetComponent<RectTransform>().sizeDelta = sizeDelta;
        _discardArea.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        _discardAreaText.fontSize = Config.fontSize;

        string text = "丢弃物品";
        if (LevelManager.Instance.IsBaseLevel)
        {
            switch (Config.dropAtBaseAction)
            {
                case Config.DropAtBaseAction.DropUnconfigured:
                {
                    text = "丢弃物品\n设置中可以调整在仓库中丢弃物品时的行为";
                    break;
                }
                case Config.DropAtBaseAction.SendToStorage:
                {
                    text = "放回仓库";
                    break;
                }
                case Config.DropAtBaseAction.Sell:
                {
                    text = "出售";
                    break;
                }
            }
        }

        _discardAreaText.text = text;
        Log($"SetDiscardAreaStyle: sizeDelta {sizeDelta}, fontSize {_discardAreaText.fontSize}");
    }

    private static void CreateDiscardArea(GameObject lootView)
    {
        if (_discardArea != null)
        {
            _discardArea.transform.SetParent(lootView.transform);
            _discardArea.transform.SetSiblingIndex(0);
            SetDiscardAreaStyle();
            return;
        }

        GameObject discardArea = new GameObject("DiscardArea");
        discardArea.transform.SetParent(lootView.transform);
        discardArea.transform.SetSiblingIndex(0);
        discardArea.AddComponent<DropArea>();

        RectTransform rectTransform = discardArea.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Image image = discardArea.AddComponent<Image>();
        image.color = new Color(21 / 255f, 41 / 255f, 66 / 255f, 0.0f);

        GameObject textObject = new GameObject("DiscardAreaText");
        textObject.transform.SetParent(discardArea.transform);
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = new Color(1, 1, 1, 0.0f);
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        textRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        textRectTransform.pivot = new Vector2(0.5f, 0.5f);
        textRectTransform.anchoredPosition = new Vector2(0, 50);

        _discardArea = discardArea;
        _discardAreaTextObject = textObject;
        _discardAreaImage = image;
        _discardAreaText = text;

        SetDiscardAreaStyle();
    }

    void Awake()
    {
        var harmony = new Harmony("com.froster.mod");
        harmony.PatchAll();
        Config = Config.LoadConfig();
        Config.SetupModConfig();
        Log("Loaded!!!");
    }

    // private void Update()
    // {
    //     Test.Update();
    // }

    private static bool _registered;


    [HarmonyPatch(typeof(LootView), "Awake")]
    public static class Patch_LootView_Awake
    {
        static void Postfix(LootView __instance)
        {
            Canvas canvas = __instance.GetComponent<Canvas>();
            if (canvas == null)
            {
                Transform parent = __instance.transform.parent;
                while (parent != null)
                {
                    canvas = parent.GetComponent<Canvas>();
                    if (canvas != null)
                        break;

                    parent = parent.parent;
                }
            }

            CanvasScaler? scaler = canvas?.GetComponent<CanvasScaler>();
            if (canvas == null || scaler == null)
            {
                Log("Canvas not found, can't load.");
                return;
            }

            _canvas = canvas;

            CreateDiscardArea(__instance.gameObject);

            if (!_registered)
            {
                _registered = true;
                IItemDragSource.OnStartDragItem += item =>
                {
                    // Log($"OnStartDragItem, item is {item}");
                    _discardAreaImage.color = new Color(21 / 255f, 41 / 255f, 66 / 255f, Config.alphaOnActive);
                    _discardAreaText.color = new Color(1, 1, 1, ModBehaviour.Config.alphaOnActive * 1.6f);
                };
                IItemDragSource.OnEndDragItem += item =>
                {
                    // Log($"OnEndDragItem, item is {item}");
                    _discardAreaImage.color = new Color(21 / 255f, 41 / 255f, 66 / 255f, 0.0f);
                    _discardAreaText.color = new Color(1, 1, 1, 0.0f);
                };
            }

            Log($"UI Add to {__instance.transform.name}");
        }
    }

    [HarmonyPatch(typeof(ItemDisplay), nameof(ItemDisplay.OnPointerClick))]
    public static class Patch_ItemDisplay_OnPointerClick
    {
        static bool patchedJudge(float delta)
        {
            // delta is eventData.clickTime - this.lastClickTime
            // Log($"Judge: {delta} {Config.enableShiftLeftClick} {Keyboard.current.shiftKey.wasPressedThisFrame} {Keyboard.current.shiftKey.isPressed}");
            return delta <= 0.3f || (Config.enableShiftLeftClick &&
                                     (Keyboard.current.shiftKey.isPressed ||
                                      Keyboard.current.shiftKey.wasPressedThisFrame));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool failed = false;
            string reason = "";
            if (codes.Count != 78)
            {
                failed = true;
                reason = $"codes.Count: {codes.Count} != 78";
            }
            else if (codes[16].opcode != OpCodes.Callvirt)
            {
                failed = true;
                reason = $"codes[16] opcode: {codes[16].opcode} != OpCodes.Callvirt";
            }
            else if (codes[16].operand is not MethodInfo info)
            {
                failed = true;
                reason = $"operand type: {codes[16].operand?.GetType().FullName ?? "null"} is not MethodInfo";
            }
            else if (info.Name != "get_clickTime")
            {
                failed = true;
                reason = $"method name: {info.Name} != get_clickTime";
            }

            if (failed)
            {
                Log($"Failed to patch ItemDisplay.OnPointerClick. {reason}");
                return codes.AsEnumerable();
            }

            // Log($"len of ItemDisplay.OnPointerClick: {codes.Count}");

            /*
            codes[19]:
                IL_0032: sub
                IL_0033: ldc.r4       0.3
                IL_0038: bgt.un.s     IL_005c
            stack after sub:
                eventData.clickTime - this.lastClickTime
             */
            var callMethod = AccessTools.Method(typeof(Patch_ItemDisplay_OnPointerClick), nameof(patchedJudge));
            codes[20] = new CodeInstruction(OpCodes.Call, callMethod);
            codes[21].opcode = OpCodes.Brfalse_S;

            Log("Successfully patched ItemDisplay.OnPointerClick. Now we have shift+left click!");

            // for (int i = 0; i < codes.Count; i++)
            // {
            //     var c = codes[i];
            //     string operandStr = c.operand switch
            //     {
            //         null => "",
            //         MethodBase m => m.DeclaringType + "::" + m.Name,
            //         FieldInfo f => f.DeclaringType + "::" + f.Name,
            //         _ => c.operand.ToString()
            //     };
            //     Log($"{i:D3}: {c.opcode} {operandStr}");
            // }

            return codes.AsEnumerable();
        }
    }

    public static SortedSet<StockShop> shops =
        new(Comparer<StockShop>.Create((a, b) => b.sellFactor.CompareTo(a.sellFactor)));

    [HarmonyPatch(typeof(StockShop), "Awake")]
    public static class Patch_StockShop_Awake
    {
        static void Postfix(StockShop __instance)
        {
            shops.Add(__instance);
        }
    }

    [HarmonyPatch(typeof(StockShop), "OnDestroy")]
    public static class Patch_StockShop_OnDestory
    {
        static void Prefix(StockShop __instance)
        {
            shops.Remove(__instance);
        }
    }
}