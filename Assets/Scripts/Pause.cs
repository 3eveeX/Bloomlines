using UnityEngine;
using UnityEngine.InputSystem;

public class OpenInventory : MonoBehaviour
{
    public bool isInventoryOpen = false;

    InputAction toggleInventoryAction;

    private void Start()
    {
        toggleInventoryAction = InputSystem.actions.FindAction("Inventory");
    }

    private void Update()
    {
        if (toggleInventoryAction != null && toggleInventoryAction.triggered)
        {
            isInventoryOpen = !isInventoryOpen;
            Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isInventoryOpen;
            Debug.Log("Inventory toggled. Now open: " + isInventoryOpen);
        }
    }
}
