using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractableController))]
public class Item : MonoBehaviour
{
    [SerializeField] DatabaseItem ItemDB;
    InteractableController interactable;

    private void Awake()
    {
        interactable = GetComponent<InteractableController>();    
    }

    void PickUp()
    {
        if (PlayerInventory.Instance.CreateNewInventoryItem(ItemDB))
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        interactable.OnMouseDown += PickUp;
    }

    private void OnDisable()
    {
        interactable.OnMouseDown -= PickUp;
    }
}

