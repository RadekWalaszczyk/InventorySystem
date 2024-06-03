using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] Image Crosshair;

    [Space(10)]
    [SerializeField] Sprite DefaultCrosshair;
    [SerializeField] Sprite InteractCrosshair;

    CrosshairState currentState;

    public void SetCrosshair(CrosshairState state)
    {
        if (currentState == state) return;

        currentState = state;

        if (state == CrosshairState.Default)
            Crosshair.sprite = DefaultCrosshair;
        else if (state == CrosshairState.Interactable)
            Crosshair.sprite = InteractCrosshair;
    }

    public enum CrosshairState
    {
        Default,
        Interactable
    }
}
