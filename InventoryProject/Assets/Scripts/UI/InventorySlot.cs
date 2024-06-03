using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] Image SlotBackground;

    [Header("--- Selected Colors")]
    [SerializeField] Color SelectedColor = Color.grey;
    [SerializeField] Color DefaultColor = Color.white;

    InventoryItem itemInSlot;

    public static event Action<InventorySlot> OnItemPut;

    public void PutItemToSlot(InventoryItem newItem)
    {
        itemInSlot = newItem;
        itemInSlot.transform.SetParent(transform);

        itemInSlot.PutInNewSlot(this);

        OnItemPut?.Invoke(this);
    }

    public void RemoveItemFromSlot(InventoryItem item)
    {
        if (itemInSlot == item)
        {
            itemInSlot = null;
        }
        OnItemPut?.Invoke(this);
    }

    public bool IsSlotEmpty()
    {
        return itemInSlot == null;
    }

    public InventoryItem GetItemInSlot()
    {
        return itemInSlot;
    }

    public DatabaseItem GetDatabaseItem()
    {
        if (IsSlotEmpty())
            return null;

        return itemInSlot.Item;
    }

    public void SetSelected(bool isSelected)
    {
        SlotBackground.color = isSelected ? SelectedColor : DefaultColor;
    }
}
