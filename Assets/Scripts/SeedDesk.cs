using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The coat-check desk for seed genetics.
///
/// Seeds in the inventory are plain data and have nowhere to store stats, so instead
/// each seed carries a small ticket number and this desk holds the actual stats.
///
/// SETUP: make an empty GameObject in your scene, name it "SeedDesk", and put this
/// script on it. That is the whole setup. Nothing else to wire up.
///
/// Ticket 0 always means "no genetics" and returns default 1.0x stats, so wild seeds,
/// shop seeds, and every non-seed item in the game keep working untouched.
/// </summary>
public class SeedDesk : MonoBehaviour
{
    private static SeedDesk instance;

    /// <summary>
    /// The one desk in the scene. If you forgot to add the GameObject, one is created
    /// automatically the first time something asks for it, so this can never be null
    /// and you will never get a mystery NullReferenceException from here.
    /// </summary>
    public static SeedDesk Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject holder = new GameObject("SeedDesk (auto-created)");
                instance = holder.AddComponent<SeedDesk>();
            }
            return instance;
        }
    }

    // ticket number -> the stats it stands for
    private readonly Dictionary<int, PlantStats> statsByTicket = new Dictionary<int, PlantStats>();

    // genotype fingerprint -> ticket number, so identical genetics reuse one ticket
    private readonly Dictionary<string, int> ticketByGenotype = new Dictionary<string, int>();

    private int nextTicket = 1;

    [SerializeField]
    [Tooltip("Log every check-in. Useful while building the breeder, noisy afterwards.")]
    private bool verbose = true;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    /// <summary>
    /// Hand in a set of stats, get a ticket number back.
    ///
    /// The stats get snapped to the nearest step first, and if some earlier seed
    /// already had these exact genetics you get that same ticket back instead of a
    /// new one. That reuse is what lets similar seeds share an inventory slot.
    /// </summary>
    public int CheckIn(PlantStats rawStats)
    {
        if (rawStats == null) return 0;

        PlantStats stats = rawStats.Snapped();
        string genotype = stats.GenotypeKey();

        if (ticketByGenotype.TryGetValue(genotype, out int existingTicket))
        {
            if (verbose)
                Debug.LogWarning($"[SeedDesk] reused ticket {existingTicket} for genotype {genotype} ({stats})");
            return existingTicket;
        }

        int ticket = nextTicket;
        nextTicket++;

        statsByTicket[ticket] = stats;
        ticketByGenotype[genotype] = ticket;

        if (verbose)
            Debug.LogWarning($"[SeedDesk] new ticket {ticket} for genotype {genotype} ({stats})");

        return ticket;
    }

    /// <summary>
    /// Hand in a ticket, get the stats back.
    /// Returns a COPY on purpose: a growing plant that modifies its own stats
    /// must not be able to reach back and corrupt every other seed's genetics.
    /// Unknown or 0 tickets return fresh default stats rather than blowing up.
    /// </summary>
    public PlantStats Lookup(int ticket)
    {
        if (ticket != 0 && statsByTicket.TryGetValue(ticket, out PlantStats stats))
            return stats.Clone();

        return new PlantStats();
    }

    /// <summary>True if this ticket actually has genetics on file.</summary>
    public bool HasGenetics(int ticket)
    {
        return ticket != 0 && statsByTicket.ContainsKey(ticket);
    }

    /// <summary>
    /// A rough player-facing quality word, from the average of the three stats.
    /// Handy for tooltips and for naming bred seeds later on.
    /// </summary>
    public string GradeOf(int ticket)
    {
        if (!HasGenetics(ticket)) return "Common";

        PlantStats s = statsByTicket[ticket];
        float average = (s.growthSpeedMultiplier + s.yieldMultiplier + s.qualityMultiplier) / 3f;

        if (average < 0.9f) return "Withered";
        if (average < 1.1f) return "Common";
        if (average < 1.4f) return "Hardy";
        if (average < 1.8f) return "Fine";
        if (average < 2.3f) return "Superb";
        return "Pristine";
    }

    /// <summary>How many distinct genotypes exist so far. Useful sanity check while testing.</summary>
    public int KnownGenotypeCount()
    {
        return statsByTicket.Count;
    }

    /// <summary>
    /// Adds items to an inventory and stamps each one with a seed ticket, so genetics
    /// survive the trip.
    ///
    /// It places one unit per empty slot rather than one big stack, because a stack is a
    /// single InventoryItem and can therefore only hold one set of genetics. If the
    /// inventory runs out of empty slots, the remainder is added the normal way with no
    /// ticket, and you get a warning saying so.
    ///
    /// Returns how many units actually got stamped.
    /// </summary>
    public static int AddTicketedItem(string inventoryName, string itemType, int ticket, int amount = 1)
    {
        if (amount <= 0) return 0;

        Inventory inv = InventoryController.instance.GetInventory(inventoryName);
        if (inv == null)
        {
            Debug.LogWarning($"[SeedDesk] no inventory named '{inventoryName}'. Nothing added.");
            return 0;
        }

        List<InventoryItem> slots = inv.GetList();
        if (slots == null)
        {
            Debug.LogWarning($"[SeedDesk] inventory '{inventoryName}' has no slot list. Nothing added.");
            return 0;
        }

        int stamped = 0;

        for (int n = 0; n < amount; n++)
        {
            int freeSlot = -1;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null || slots[i].GetIsNull())
                {
                    freeSlot = i;
                    break;
                }
            }

            if (freeSlot < 0) break; // inventory full, handled below

            InventoryController.instance.AddItemPos(inventoryName, itemType, freeSlot, 1);

            InventoryItem placed = inv.InventoryGetItem(freeSlot);
            if (placed != null && !placed.GetIsNull())
            {
                placed.SetSeedTicket(ticket);
                stamped++;
            }
        }

        int leftover = amount - stamped;
        if (leftover > 0)
        {
            InventoryController.instance.AddItem(inventoryName, itemType, leftover);
            Debug.LogWarning($"[SeedDesk] '{inventoryName}' ran out of empty slots. " +
                             $"{leftover}x {itemType} added without genetics (ticket 0).");
        }

        return stamped;
    }
}
