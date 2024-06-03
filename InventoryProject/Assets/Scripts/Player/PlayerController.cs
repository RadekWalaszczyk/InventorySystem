using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float PickableDistance;
    [SerializeField] LayerMask PickableMask;

    [Space(10)]
    [SerializeField] PlayerHandInventory PlayerHands;
    [SerializeField] PlayerMovement PlayerMovement;
    [SerializeField] CrosshairController Crosshair;

    Camera _mainCamera;
    PlayerInventory _inventory;

    int lastHandNumber = 1;
    int currentHandNumber = 0;

    PlayerInputState playerInputState;

    const string openInventory = "OpenInventory";
    const string interaction = "Interaction";
    const string mouseScroll = "Mouse ScrollWheel";

    private void Start()
    {
        _mainCamera = Camera.main;
        _inventory = PlayerInventory.Instance;

        playerInputState = PlayerInputState.Default;
    }

    private void Update()
    {
        PlayerUIInput();
        
        if (playerInputState != PlayerInputState.OnlyUI)
        {
            PlayerInput();
            PlayerMovement.UpdateMovement();
        }
    }

    void PlayerUIInput()
    {
        if (Input.GetButtonDown(openInventory))
        {
            var isOpen = _inventory.ToggleInventory();
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;

            playerInputState = isOpen ? PlayerInputState.OnlyUI : PlayerInputState.Default;
        }
    }

    void PlayerInput()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray.origin, ray.direction, out var hit, PickableDistance, PickableMask, QueryTriggerInteraction.Ignore))
        {
            Crosshair.SetCrosshair(CrosshairController.CrosshairState.Interactable);

            if (Input.GetButtonDown(interaction))
            {
                var pickable = hit.collider.GetComponentInParent<InteractableController>();
                if (pickable != null)
                {
                    pickable.MouseDown();
                }
            }
        }
        else
        {
            Crosshair.SetCrosshair(CrosshairController.CrosshairState.Default);
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlayerHands.ItemAction();
        }

        #region Hand Pick

        int selectedSlot = CheckSlotInput();

        if (selectedSlot >= 0 && selectedSlot < _inventory.handInventory.Count)
            currentHandNumber = selectedSlot;

        if (Input.GetAxis(mouseScroll) < 0)
        {
            currentHandNumber += 1;
            if (currentHandNumber > _inventory.handInventory.Count - 1)
                currentHandNumber = 0;
        }
        if (Input.GetAxis(mouseScroll) > 0)
        {
            currentHandNumber -= 1;
            if (currentHandNumber < 0)
                currentHandNumber = _inventory.handInventory.Count - 1;
        }

        if (lastHandNumber != currentHandNumber)
        {
            _inventory.handInventory[lastHandNumber].SetSelected(false);
            _inventory.handInventory[currentHandNumber].SetSelected(true);

            RefreshSlot(_inventory.handInventory[currentHandNumber]);
            lastHandNumber = currentHandNumber;
        }

        #endregion

    }

    #region Check Slot Input

    readonly Array acceptableKeycodes = new[]
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
    };

    private int CheckSlotInput()
    {
        var inputDetected = false;
        var input = KeyCode.Escape;

        foreach (KeyCode keycode in acceptableKeycodes)
        {
            if (Input.GetKeyDown(keycode))
            {
                inputDetected = true;
                input = keycode;
                break;
            }
        }

        if (inputDetected)
        {
            return input - KeyCode.Alpha1;
        }

        return -1;
    }

    #endregion

    void RefreshSlot(InventorySlot slot)
    {
        if (slot == _inventory.handInventory[currentHandNumber])
        {
            var itemToHand = _inventory.handInventory[currentHandNumber].GetDatabaseItem();
            PlayerHands.SetItemToHand(itemToHand);
        }
    }

    private void OnEnable()
    {
        InventorySlot.OnItemPut += RefreshSlot;
    }

    private void OnDisable()
    {
        InventorySlot.OnItemPut -= RefreshSlot;
    }

    enum PlayerInputState
    {
        Default,
        OnlyUI
    }
}
