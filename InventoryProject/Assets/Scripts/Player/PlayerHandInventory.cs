using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PlayerHandInventory : MonoBehaviour
{
    [SerializeField] Transform ItemSlot;
    [SerializeField] MeshFilter MeshInHand;

    [Space(10)]
    [SerializeField] Image Crosshair;

    DatabaseItem itemInHand;

    Animator anim;
    MeshRenderer meshRenderer;

    bool isUsing = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        meshRenderer = MeshInHand.GetComponent<MeshRenderer>();
    }

    public void SetItemToHand(DatabaseItem newItem)
    {
        if (newItem == null)
            MeshInHand.mesh = null;

        itemInHand = newItem;
        if (itemInHand != null)
        {
            anim.SetTrigger("Equip");
        }
        isUsing = false;
    }

    public void Equip()
    {
        if (itemInHand != null)
        {
            MeshInHand.mesh = itemInHand.Mesh;
            meshRenderer.material = itemInHand.Material;
        }
    }

    public void ItemAction()
    {
        if (itemInHand != null && !isUsing)
        {
            isUsing = true;
            anim.SetTrigger("Use");
        }
    }

    public void UseEffect()
    {
        if (itemInHand != null)
        {
            itemInHand.UseItem();
        }
        isUsing = false;
    }
}
