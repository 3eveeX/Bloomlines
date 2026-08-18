using InventorySystem;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Seeder : MonoBehaviour
{
    [SerializeField]
    GameObject UItoOpen;

    [SerializeField]
    PlantDatabase pD;

     Slot inputSlot;    
     Slot outputSlot;   
    [SerializeField] float secondsPerSeed = 3f;
    Coroutine seedRoutine;

    InputAction moveAction;

   


    bool interact = false;
    bool open = false;



    bool slotsLogged = false;

    // Remembers the last item type we complained about, so the warning fires once
    // instead of once per frame.
    string lastUnmappedType = null;

    bool ResolveSlots()
    {
        // Unity reports destroyed objects as null, so this also recovers
        // if the inventory UI rebuilds its slot objects.
        if (inputSlot != null && outputSlot != null) return true;

        Slot[] found = UItoOpen.GetComponentsInChildren<Slot>(true);
        if (found.Length < 2) return false;

        inputSlot = found[0];
        outputSlot = found[1];

        if (!slotsLogged)
        {
            slotsLogged = true;
            Debug.LogWarning($"[Seeder] resolved input={inputSlot.gameObject.name} " +
                             $"output={outputSlot.gameObject.name}");
        }
        return true;
    }

   

    void Start()
    {
        UItoOpen.SetActive(false);
        moveAction = InputSystem.actions.FindAction("Move");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interact = true;
            Debug.LogWarning("Player entered the seeder area.");
            Debug.LogWarning("Input Slot Item = " + inputSlot.GetItem());
            Debug.LogWarning("Input Slot Item Type = " + inputSlot.GetItem()?.GetItemType());
            Debug.LogWarning("Output Slot Item = " + outputSlot.GetItem());
            Debug.LogWarning("Output Slot Item Type = " + outputSlot.GetItem()?.GetItemType());

            Debug.LogWarning("Input = " + (inputSlot != null ? inputSlot.ToString() : "null") + ", Output = " + (outputSlot != null ? outputSlot.ToString() : "null"));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interact = false;
            Debug.LogWarning("Player exited the seeder area.");
            
        }
    }

    void Update()
    {
        if (interact && Input.GetKeyDown(KeyCode.E))
        {
            if (open)
            {
                UItoOpen.SetActive(false);
                moveAction.Enable();
                open = false;
            }
            else
            {
                UItoOpen.SetActive(true);
                moveAction.Disable();
                open = true;
            }

            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;
        }
        if (pD == null || !ResolveSlots()) return;



        if (pD == null || !ResolveSlots()) return;
        if (seedRoutine != null) return;

        InventoryItem item = inputSlot.GetItem();
        if (item == null || item.GetIsNull()) return;

        // Don't run into an occupied output slot. Adding onto an existing stack would
        // merge two different sets of genetics into one item and quietly lose one.
        InventoryItem existingOutput = outputSlot.GetItem();
        if (existingOutput != null && !existingOutput.GetIsNull()) return;

        string type = item.GetItemType();

        // The genetics riding on the produce that was fed in. 0 for plain wild produce.
        int ticket = item.GetSeedTicket();

        if (pD.TryGetSeedType(type, out string seedType))
        {
            lastUnmappedType = null;
            seedRoutine = StartCoroutine(SeedRoutine(type, seedType, ticket));
        }
        else if (type != lastUnmappedType)
        {
            // This runs in Update, so without the guard it would log every single frame
            // for as long as an unmapped item sits in the input slot. Log once per type.
            lastUnmappedType = type;
            Debug.LogWarning($"[Seeder] no seed mapping for '{type}'. " +
                             "TryGetSeedType matches on PlantData.plantName, so this item's " +
                             "type has to equal a plantName in the PlantDatabase.");
        }
    }



    IEnumerator SeedRoutine(string itemType, string seedType, int ticket)
    {
        float elapsed = 0f;
        while (elapsed < secondsPerSeed)
        {
            InventoryItem current = inputSlot.GetItem();
            if (current == null || current.GetIsNull() || current.GetItemType() != itemType)
            {
                seedRoutine = null;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // remove from slot 0, then hand seedType to your inventory system for slot 1
        InventoryController.instance.RemoveItemPos("Seeder", 0, 1);
        InventoryController.instance.AddItemPos("Seeder", seedType, 1, 1);

        // Pass the produce's genetics on to the seed it became. Fetched back out of the
        // slot because the inventory copy-constructs items when it stores them.
        InventoryItem madeSeed = outputSlot.GetItem();
        if (madeSeed != null && !madeSeed.GetIsNull())
        {
            madeSeed.SetSeedTicket(ticket);
            Debug.LogWarning($"[Seeder] made {seedType} carrying ticket {ticket} " +
                      $"[{SeedDesk.Instance.GradeOf(ticket)}]");
        }
        else
        {
            Debug.LogWarning($"[Seeder] could not place '{seedType}' into slot 1 of 'Seeder'. " +
                             "Genetics were not carried over.");
        }

        seedRoutine = null;
    }
}
