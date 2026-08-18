using InventorySystem;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Breeder : MonoBehaviour
{
    [SerializeField]
    GameObject InputUItoOpen;
    [SerializeField]
    GameObject OutputUItoOpen;

    InputAction moveAction;

    [SerializeField]
    PlantDatabase pD;

    [Tooltip("Inventory name of the two-slot input inventory, spelled as in the InventoryController.")]
    [SerializeField]
    string inputInventoryName = "BreederInput";

    [Tooltip("Inventory name of the output inventory, spelled as in the InventoryController.")]
    [SerializeField]
    string outputInventoryName = "BreederOutput";

    [SerializeField]
    float secondsPerBreed = 5f;

    Coroutine breedRoutine;

    Slot inputSlot1;
    Slot inputSlot2;
    Slot outputSlot;

    bool interact = false;
    bool open = false;

    bool slotsLogged = false;


    bool ResolveSlots()
    {
        // Unity reports destroyed objects as null, so this also recovers
        // if the inventory UI rebuilds its slot objects.
        if (inputSlot1 != null && inputSlot2 != null && outputSlot != null) return true;

        Slot[] found = InputUItoOpen.GetComponentsInChildren<Slot>(true);
        Slot[] outputFound = OutputUItoOpen.GetComponentsInChildren<Slot>(true);
        if (found.Length < 2) return false;
        if (outputFound.Length < 1) return false;

        inputSlot1 = found[0];
        inputSlot2 = found[1];
        outputSlot = outputFound[0];

        if (!slotsLogged)
        {
            slotsLogged = true;
            Debug.LogWarning($"[Breeder] resolved input 1={inputSlot1.gameObject.name} " + $"input 2 ={inputSlot2.gameObject.name} " +
                             $"output={outputSlot.gameObject.name}");
        }
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interact = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interact = false;


        }
    }

    void Start()
    {
        InputUItoOpen.SetActive(false);
        OutputUItoOpen.SetActive(false);
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        //when the user presses e next to the breeder, the breeder will open its inventory

        if (interact && Input.GetKeyDown(KeyCode.E))
        {
            if (open)
            {
                InputUItoOpen.SetActive(false);
                OutputUItoOpen.SetActive(false);
                moveAction.Enable();
                open = false;
            }
            else
            {
                InputUItoOpen.SetActive(true);
                OutputUItoOpen.SetActive(true);
                moveAction.Disable();
                open = true;
            }

        }


        //when the user puts a seed in each of the breeder's input slots, it starts the breeding process

        /*
         ( The breeding process:
            - The breeder will check each seed's stats            -> SeedDesk.Lookup(ticket)
            - The breeder will decide which stats to combine and how based on random chance:
                - Take one stat from a category and just use that
                - Take the average of the two stats rounded up from a category and use that
                - Add the two stats from a category together and use that
                - Make up a completely new number between the two stats from a category and use that
                                                                  -> Breeding.Mix(), already written
            - The breeder will create a new seed with the combined stats
                                                                  -> SeedDesk.CheckIn() + AddItemPos()
            -If the breeder has 2 different seeds in the input slots, it will mutate the output seed
             to something new based on a hidden mutation chance)
                                                                  -> Breeding.ResolveSpecies()
         )
         */

        if (pD == null || !ResolveSlots()) return;
        if (breedRoutine != null) return;                   // already breeding, leave it alone

        InventoryItem seedA = inputSlot1.GetItem();
        InventoryItem seedB = inputSlot2.GetItem();

        if (IsEmpty(seedA) || IsEmpty(seedB)) return;       // need two parents
        if (!IsEmpty(outputSlot.GetItem())) return;         // output full, wait for the player to take it

        breedRoutine = StartCoroutine(BreedRoutine(seedA.GetItemType(), seedB.GetItemType()));
    }

    static bool IsEmpty(InventoryItem item)
    {
        return item == null || item.GetIsNull();
    }

    IEnumerator BreedRoutine(string typeA, string typeB)
    {
        // Wait out the timer, bailing if the player pulls either parent back out.
        float elapsed = 0f;
        while (elapsed < secondsPerBreed)
        {
            InventoryItem a = inputSlot1.GetItem();
            InventoryItem b = inputSlot2.GetItem();

            if (IsEmpty(a) || IsEmpty(b) || a.GetItemType() != typeA || b.GetItemType() != typeB || !typeA.Contains("Seed") || !typeB.Contains("Seed"))
            {
                breedRoutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        InventoryItem parentA = inputSlot1.GetItem();
        InventoryItem parentB = inputSlot2.GetItem();

        // Which species are these seeds? This works straight off the seed, with no planting
        // required, because PlantDatabase matches on the item's related GameObject.
        PlantData dataA = pD.GetPlantData(parentA);
        PlantData dataB = pD.GetPlantData(parentB);

        if (dataA == null || dataB == null)
        {
            Debug.LogWarning($"[Breeder] no PlantData for '{typeA}' or '{typeB}'. " +
                             "Check that both seed items have their Related GameObject set " +
                             "and that it matches a seedRelatedObject in the PlantDatabase.");
            breedRoutine = null;
            yield break;
        }

        // Pull each parent's genetics off the desk. A seed with ticket 0 is a plain
        // wild seed and correctly comes back as default 1.0x stats.
        PlantStats statsA = SeedDesk.Instance.Lookup(parentA.GetSeedTicket());
        PlantStats statsB = SeedDesk.Instance.Lookup(parentB.GetSeedTicket());

        PlantStats babyStats = Breeding.Combine(statsA, statsB);
        PlantData babySpecies = Breeding.ResolveSpecies(dataA, dataB);

        int babyTicket = SeedDesk.Instance.CheckIn(babyStats);

        // Consume both parents.
        InventoryController.instance.RemoveItemPos(inputInventoryName, 0, 1);
        InventoryController.instance.RemoveItemPos(inputInventoryName, 1, 1);

        // Create the child seed, then stamp its ticket on the item the inventory actually
        // stored. The inventory copy-constructs items, so we have to fetch it back out of
        // the slot rather than stamping a copy we made ourselves.
        string babySeedType = babySpecies.plantName + "Seed";
        InventoryController.instance.AddItemPos(outputInventoryName, babySeedType, 0, 1);

        InventoryItem child = outputSlot.GetItem();
        if (!IsEmpty(child))
        {
            child.SetSeedTicket(babyTicket);
            Debug.LogWarning($"[Breeder] bred {babySeedType} ticket {babyTicket} " +
                      $"[{SeedDesk.Instance.GradeOf(babyTicket)}] {babyStats}");
        }
        else
        {
            Debug.LogWarning($"[Breeder] could not place '{babySeedType}' into '{outputInventoryName}'. " +
                             "Check the item type name and that the inventory accepts it.");
        }

        breedRoutine = null;
    }
}
