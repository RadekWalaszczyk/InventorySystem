using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item")]
public class DatabaseItem : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public GameObject Prefab;
    public Mesh Mesh;
    public Material Material;

    [SerializeReference] public List<ItemUsable> OnUse;

    public void UseItem()
    {
        foreach (var useEffect in OnUse)
        {
            useEffect.Use();
        }
    }
}

[Serializable]
public abstract class ItemUsable
{
    public abstract void Use();

    [HideInInspector]
    public string name;
}

/*
 * Normally I'd separate these classes into separate scripts, but when I only have two of them, I'll keep them here
 */

[Serializable]
public class Heal : ItemUsable
{
    public override void Use()
    {
        Debug.Log("Heal Player: + " + HealAmount + " HP");
    }

    public int HealAmount;
}

[Serializable]
public class Drunk : ItemUsable
{
    public override void Use()
    {
        PostProcessManager.Instance.SetDrunkness(DrunkValue, DrunkTime);
    }

    public float DrunkTime;
    [Range(0f, 1f)]
    public float DrunkValue;
}
