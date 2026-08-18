
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
        if (heldSeed == null || heldSeed.GetIsNull()) return;

        string itemType = heldSeed.GetItemType();
        if (itemType == null || !itemType.Contains("Seed")) return;

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

        // The prefab's own stats are only a default. Overwrite them with this
        // particular seed's genetics, fetched from the desk using its ticket.
        // A wild seed has ticket 0 and correctly gets plain 1.0x stats back.
        currentPlant.stats = SeedDesk.Instance.Lookup(seedItem.GetSeedTicket());
    }

    private void Harvest()
    {
        // Mathf.Max keeps a badly bred plant from yielding literally nothing.
        int yieldAmount = Mathf.Max(1, Mathf.RoundToInt(currentPlant.data.baseYieldAmount * currentPlant.stats.yieldMultiplier));

        // Carry this plant's genetics into the produce it drops, so the seeder can pass
        // them on to the next generation instead of resetting everything to 1.0x.
        int ticket = SeedDesk.Instance.CheckIn(currentPlant.stats);
        SeedDesk.AddTicketedItem("Hotbar", currentPlant.data.harvestedItemType, ticket, yieldAmount);

        Destroy(currentPlant.gameObject);
        currentPlant = null;
        isPotPlanted = false;
        seedItem = null;
    }

}