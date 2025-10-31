using Cysharp.Threading.Tasks;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using static DragToDrop.ModBehaviour;

namespace DragToDrop;

public class DropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.used || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        IItemDragSource component = eventData.pointerDrag.gameObject.GetComponent<IItemDragSource>();
        if (component == null || !component.IsEditable())
        {
            return;
        }

        Item item = component.GetItem();
        if (item == null || !item.CanDrop)
        {
            return;
        }

        ItemUIUtilities.NotifyPutItem(item);
        if (component is InventoryEntry ie && !ie.CanOperate)
        {
            return;
        }

        if (component is SlotDisplay sd && !sd.IsEditable())
        {
            return;
        }

        LevelManager levelManager = LevelManager.Instance;
        if (levelManager.IsBaseLevel)
        {
            switch (ModBehaviour.Config.dropAtBaseAction)
            {
                case Config.DropAtBaseAction.SendToStorage:
                {
                    if (PlayerStorage.IsAccessableAndNotFull())
                    {
                        ItemUtilities.SendToPlayerStorage(item);
                    }
                    break;
                }
                case Config.DropAtBaseAction.Sell:
                {
                    if (shops.Count > 0 && shops.Min != null)
                    {
                        Log($"Sell to {shops.Min.MerchantID}");
                        ((UniTask)Util.CallMethod(shops.Min, "Sell", new object[] { item })).Forget();
                    }
                    break;
                }
                case Config.DropAtBaseAction.Drop:
                case Config.DropAtBaseAction.DropUnconfigured:
                default:
                {
                    item.Drop(CharacterMainControl.Main, true);
                    break;
                }
            }
        }
        else
        {
            item.Drop(CharacterMainControl.Main, true);
        }
    }
}