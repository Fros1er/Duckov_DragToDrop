using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragToDrop;

public class DropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    // public static void Log(object message)
    // {
    //     ModBehaviour.Log(message);
    // }

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
            // Log($"eventData.used {eventData.used} || eventData.button {eventData.button}");
            return;
        }

        IItemDragSource component = eventData.pointerDrag.gameObject.GetComponent<IItemDragSource>();
        if (component == null || !component.IsEditable())
        {
            // Log($"{component} || component.IsEditable() {component?.IsEditable()}");
            return;
        }

        Item item = component.GetItem();
        if (item == null || !item.CanDrop)
        {
            // Log($"{item} || CanDrop: {item?.CanDrop}");
            return;
        }

        ItemUIUtilities.NotifyPutItem(item);

        if (component is InventoryEntry ie && !ie.CanOperate)
        {
            // Log($"component is InventoryEntry, CanOperate={ie.CanOperate}");
            return;
        }

        if (component is SlotDisplay sd && !sd.IsEditable())
        {
            // Log($"component is SlotDisplay, Editable={sd.Editable}");
            return;
        }

        item.Drop(CharacterMainControl.Main, true);
    }
}