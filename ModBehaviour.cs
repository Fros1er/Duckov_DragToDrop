using Duckov.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DragToDrop
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static GameObject _discardArea;
        private static Image _discardAreaImage;
        private static GameObject _discardAreaTextObject;
        private static Text _discardAreaText;

        public static void Log(object message)
        {
            Debug.Log($"[DragToDrop] {message}");
        }

        private static void CreateDiscardArea(GameObject lootView, float scale)
        {
            if (_discardArea != null)
            {
                _discardArea.transform.SetParent(lootView.transform);
                _discardArea.transform.SetSiblingIndex(0);
                _discardAreaTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(927, 896) * scale;
                _discardAreaText.fontSize = (158 * scale / 5 < 24) ? (int)(158 * scale / 5) : 24;
                return;
            }

            GameObject discardArea = new GameObject("DiscardArea");
            discardArea.transform.SetParent(lootView.transform);
            discardArea.transform.SetSiblingIndex(0);
            discardArea.AddComponent<DropArea>();

            RectTransform rectTransform = discardArea.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(927, 896) * scale;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-17, 98);
            Log(
                $"Scale = {scale} SizeDelta: {rectTransform.sizeDelta} anchoredPosition: {rectTransform.anchoredPosition}");

            Image image = discardArea.AddComponent<Image>();
            image.color = new Color(21 / 255f, 41 / 255f, 66 / 255f, 0.0f);

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(discardArea.transform);
            Text text = textObject.AddComponent<Text>();
            text.text = "丢弃物品";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 24;
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
        }

        void Awake()
        {
            var harmony = new Harmony("com.froster.mod");
            harmony.PatchAll();
            Log($"Resolution: {Screen.width}x{Screen.height}");
            Log("Loaded!!!");
        }

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

                // Log(
                //     $"uiScaleMode: {scaler.uiScaleMode} referencePixelsPerUnit: {scaler.referencePixelsPerUnit} scaleFactor: {scaler.scaleFactor} referenceResolution: {scaler.referenceResolution} screenMatchMode: {scaler.screenMatchMode}");


                CreateDiscardArea(__instance.gameObject, canvas.scaleFactor);

                if (!_registered)
                {
                    _registered = true;
                    IItemDragSource.OnStartDragItem += item =>
                    {
                        Log($"OnStartDragItem, item is {item}");
                        _discardAreaImage.color = new Color(21 / 255f, 41 / 255f, 66 / 255f, 0.3f);
                        _discardAreaText.color = new Color(1, 1, 1, 0.5f);
                    };
                    IItemDragSource.OnEndDragItem += item =>
                    {
                        Log($"OnEndDragItem, item is {item}");
                        _discardAreaImage.color = new Color(21 / 255f, 41 / 255f, 66 / 255f, 0.0f);
                        _discardAreaText.color = new Color(1, 1, 1, 0.0f);
                    };
                }

                Log($"UI Add to {__instance.transform.name}");
            }
        }
    }
}