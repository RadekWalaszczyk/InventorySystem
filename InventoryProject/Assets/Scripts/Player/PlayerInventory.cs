using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] Transform HandInventoryParent;
    [SerializeField] Transform MainInventoryParent;

    [Space(10)]
    [SerializeField] GameObject Inventory;
    [SerializeField] InventoryItem InventoryItemPrefab;

    public List<InventorySlot> handInventory { get; private set; }
    List<InventorySlot> mainInventory;

    public static PlayerInventory Instance;
    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        handInventory = HandInventoryParent.GetComponentsInChildren<InventorySlot>().ToList();
        mainInventory = MainInventoryParent.GetComponentsInChildren<InventorySlot>().ToList();
    }

    public bool ToggleInventory()
    {
        Inventory.SetActive(!Inventory.activeSelf);
        return Inventory.activeSelf;
    }

    #region Slots And Inventory

    /// <summary>
    /// Create new item and try to put it into inventory
    /// </summary>
    /// <param name="newItem">Item to create and put into inventory</param>
    /// <returns></returns>
    public bool CreateNewInventoryItem(DatabaseItem newItem)
    {
        var emptySlot = FindEmptySlot();
        if (emptySlot != null)
        {
            InventoryItem newInventoryItem = Instantiate(InventoryItemPrefab, transform);
            newInventoryItem.InitializeItem(newItem, emptySlot);

            emptySlot.PutItemToSlot(newInventoryItem);
            return true;
        }

        Debug.Log("There is no space in inventory");
        return false;
    }


    InventorySlot FindEmptySlot()
    {
        foreach (var slot in handInventory.Where(x => x.IsSlotEmpty()))
        {
            return slot;
        }

        foreach (var slot in mainInventory.Where(x => x.IsSlotEmpty()))
        {
            return slot;
        }

        return null;
    }

    #endregion
}
