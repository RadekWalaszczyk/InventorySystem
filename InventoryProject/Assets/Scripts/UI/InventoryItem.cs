using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class InventoryItem : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image ItemIcon;

    public DatabaseItem Item { get; private set; }

    RectTransform rectTrans;
    Canvas canvas;

    InventorySlot lastSlot;

    private void Start()
    {
        rectTrans = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void InitializeItem(DatabaseItem item, InventorySlot slot)
    {
        Item = item;
        ItemIcon.sprite = item.Icon;
        lastSlot = slot;
    }

    #region Drag n drop item

    public void OnDrag(PointerEventData eventData)
    {
        rectTrans.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ItemIcon.raycastTarget = false;
        rectTrans.SetParent(PlayerInventory.Instance.transform);

        lastSlot.RemoveItemFromSlot(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        var slot = GetSlot();
        if (slot != null && slot.IsSlotEmpty())
        {
            // Put item to empty slot

            slot.PutItemToSlot(this);
        }
        else if (slot != null && !slot.IsSlotEmpty())
        {
            // Swap items in slots

            var itemInSlot = slot.GetItemInSlot();
            lastSlot.PutItemToSlot(itemInSlot);

            slot.PutItemToSlot(this);
        }
        else
        {
            // Drop item from inventory

            DropItem();
            return;
        }
        ItemIcon.raycastTarget = true;
    }

    #endregion

    public void PutInNewSlot(InventorySlot slot)
    {
        lastSlot = slot;
        transform.localPosition = Vector3.zero;
    }

    public void DropItem()
    {
        Camera _camera = Camera.main;
        Vector3 dropPos = _camera.transform.position + _camera.transform.forward;
        var dropedItem = Instantiate(Item.Prefab, dropPos, Quaternion.identity);
        lastSlot = null;
        Destroy(gameObject);
    }

    //Gets all event system raycast results of current mouse position.
    InventorySlot GetSlot()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);

        for (int index = 0; index < raysastResults.Count; index++)
        {
            RaycastResult curRaycastResult = raysastResults[index];
            var slot = curRaycastResult.gameObject.GetComponentInParent<InventorySlot>();
            if (slot != null)
            {
                return slot;
            }
        }

        return null;
    }
}
