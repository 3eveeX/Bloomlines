using UnityEngine;

[System.Serializable]
public class PlantStats
{
    public float growthSpeedMultiplier = 1f;
    public float yieldMultiplier = 1f;
    public float qualityMultiplier = 1f;

    /// <summary>
    /// How fine-grained genetics are allowed to be.
    /// 0.05 means stats can only ever be 1.00, 1.05, 1.10, 1.15 ... and nothing in between.
    /// BIGGER number  = fewer possible seeds = more stacking = tidier inventory, coarser breeding.
    /// SMALLER number = more possible seeds  = less stacking = messier inventory, finer breeding.
    /// This is the one dial you tune if the inventory feels cluttered.
    /// </summary>
    public const float StatStep = 0.05f;

    /// <summary>
    /// Returns a copy with every stat snapped to the nearest StatStep.
    /// Two seeds that were 1.03847 and 1.03846 both become 1.05, so they
    /// count as the same genetics and are allowed to share an inventory slot.
    /// </summary>
    public PlantStats Snapped()
    {
        return new PlantStats
        {
            growthSpeedMultiplier = Snap(growthSpeedMultiplier),
            yieldMultiplier = Snap(yieldMultiplier),
            qualityMultiplier = Snap(qualityMultiplier),
        };
    }

    private static float Snap(float value)
    {
        return Mathf.Round(value / StatStep) * StatStep;
    }

    /// <summary>
    /// A short text fingerprint of these stats, e.g. "21_20_23".
    /// Identical genetics always produce an identical fingerprint,
    /// which is how SeedDesk knows two seeds are interchangeable.
    /// </summary>
    public string GenotypeKey()
    {
        int g = Mathf.RoundToInt(growthSpeedMultiplier / StatStep);
        int y = Mathf.RoundToInt(yieldMultiplier / StatStep);
        int q = Mathf.RoundToInt(qualityMultiplier / StatStep);
        return g + "_" + y + "_" + q;
    }

    public PlantStats Clone()
    {
        return new PlantStats
        {
            growthSpeedMultiplier = growthSpeedMultiplier,
            yieldMultiplier = yieldMultiplier,
            qualityMultiplier = qualityMultiplier,
        };
    }

    public override string ToString()
    {
        return $"growth x{growthSpeedMultiplier:0.00}, yield x{yieldMultiplier:0.00}, quality x{qualityMultiplier:0.00}";
    }
}
