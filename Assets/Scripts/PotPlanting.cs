
using InventorySystem;

using UnityEngine;
using UnityEngine.InputSystem;



public class PotPlanting : MonoBehaviour
{
    [SerializeField]
    private HotbarScroll hotbarScroll;

    [SerializeField]
    private PlantDatabase plantDatabase;

    bool isPlayerInRange = false;
    public bool isPotPlanted = false;
    InputAction interactAction;
    public InventoryItem seedItem;
    private Plant currentPlant;

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Player entered pot range");
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.LogWarning("Player exited pot range");
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }

    private void Update()
    {
        if (!isPlayerInRange) return;
        if (!interactAction.WasPressedThisFrame()) return;

        if (!isPotPlanted)
        {
            TryPlantSeed();
        }
        else if (currentPlant != null && currentPlant.IsFullyGrown)
        {
            Harvest();
        }
    }

    private void TryPlantSeed()
    {
        int slotIndex = hotbarScroll.CurrentSlotIndex;
        Inventory hotbar = InventoryController.instance.GetInventory("Hotbar");
        InventoryItem heldSeed = hotbar.InventoryGetItem(slotIndex);
        string itemType = heldSeed.GetItemType();

        if (itemType != "Seed") return;

        PlantData data = plantDatabase.GetPlantData(heldSeed);
        if (data == null)
        {
            Debug.LogWarning("No PlantData found for this seed - did you add it to the PlantDatabase?");
            return;
        }

        isPotPlanted = true;
        seedItem = heldSeed;
        InventoryController.instance.RemoveItemPos("Hotbar", slotIndex, 1);

        GameObject plantObj = Instantiate(data.grownPlantPrefab, transform.position, UnityEngine.Quaternion.identity);
        currentPlant = plantObj.GetComponent<Plant>();
        currentPlant.data = data;
    }

    private void Harvest()
    {
        int yieldAmount = Mathf.RoundToInt(currentPlant.data.baseYieldAmount * currentPlant.stats.yieldMultiplier);
        InventoryController.instance.AddItem("Hotbar", currentPlant.data.harvestedItemType, yieldAmount);

        Destroy(currentPlant.gameObject);
        currentPlant = null;
        isPotPlanted = false;
        seedItem = null;
    }

}