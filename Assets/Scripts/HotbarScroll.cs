using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Reflection;
using InventorySystem; // wherever InventoryUIManager and Slot live

public class HotbarScroll : MonoBehaviour
{
    [SerializeField]
    private InventoryUIManager inventoryUI;

    Mouse mouse;
    private Dictionary<int, GameObject> positionToSlotDict;
    private FieldInfo previouslyHighlightedField;
    private int currentSlot = 0;

    public int CurrentSlotIndex => currentSlot;

    void Start()
    {
        mouse = Mouse.current;

        FieldInfo dictField = typeof(InventoryUIManager).GetField(
            "positionToSlotDict",
            BindingFlags.NonPublic | BindingFlags.Instance);

        previouslyHighlightedField = typeof(InventoryUIManager).GetField(
            "previouslyHighlighted",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (dictField == null || previouslyHighlightedField == null)
        {
            Debug.LogError("HotbarScroll: couldn't find expected private fields on InventoryUIManager. " +
                           "The package may have changed its internals.");
            return;
        }

        positionToSlotDict = (Dictionary<int, GameObject>)dictField.GetValue(inventoryUI);
    }

    void Update()
    {
        if (positionToSlotDict == null || positionToSlotDict.Count == 0) return;

        SyncCurrentSlot();

        float scroll = mouse.scroll.ReadValue().y;

        if (scroll > 0f)
        {
            ScrollSelect(-1);
        }
        else if (scroll < 0f)
        {
            ScrollSelect(1);
        }
    }

    /// <summary>
    /// Reads whatever slot the package currently has highlighted (from a number key, click, etc.)
    /// so scrolling continues from the right spot instead of its own stale index.
    /// </summary>
    private void SyncCurrentSlot()
    {
        GameObject highlighted = (GameObject)previouslyHighlightedField.GetValue(inventoryUI);
        if (highlighted != null)
        {
            Slot slotComponent = highlighted.GetComponent<Slot>();
            if (slotComponent != null)
            {
                currentSlot = slotComponent.GetPosition(); // public method, no reflection needed
            }
        }
    }

    private void ScrollSelect(int direction)
    {
        int slotCount = positionToSlotDict.Count;
        currentSlot = ((currentSlot + direction) % slotCount + slotCount) % slotCount;

        if (positionToSlotDict.ContainsKey(currentSlot))
        {
            inventoryUI.SetPressed(positionToSlotDict[currentSlot], true);
        }
    }
}