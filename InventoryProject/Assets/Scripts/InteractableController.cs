using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableController : MonoBehaviour
{
    public event Action OnMouseDown;

    public void MouseDown()
    {
        OnMouseDown?.Invoke();
    }
}
