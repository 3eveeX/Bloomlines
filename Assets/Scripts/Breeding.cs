using UnityEngine;

public static class Breeding
{
    const float Cap = 3f;
    const float Floor = 0.1f;


    

    

    public static PlantStats Combine(PlantStats a, PlantStats b) => new PlantStats
    {
        growthSpeedMultiplier = Mix(a.growthSpeedMultiplier, b.growthSpeedMultiplier),
        yieldMultiplier = Mix(a.yieldMultiplier, b.yieldMultiplier),
        qualityMultiplier = Mix(a.qualityMultiplier, b.qualityMultiplier),
    };

    static float Mix(float x, float y)
    {
        float lo = Mathf.Min(x, y), hi = Mathf.Max(x, y);
        float roll = Random.value;
        float result;

        if (roll < 0.40f) result = (x + y) * 0.5f;                      
        else if (roll < 0.70f) result = Random.value < 0.5f ? x : y;         
        else if (roll < 0.95f) result = Random.Range(lo * 0.95f, hi * 1.08f);
        else result = hi * 1.15f;                          

        return Mathf.Clamp(result, Floor, Cap);
    }

    public static PlantData ResolveSpecies(PlantData a, PlantData b)
    {
        if (a == b) return a;

        if (a.mutations != null)
            foreach (var m in a.mutations)
                if (m.partner == b && Random.value < m.chance) return m.result;

        if (b.mutations != null)
            foreach (var m in b.mutations)
                if (m.partner == a && Random.value < m.chance) return m.result;

        return Random.value < 0.5f ? a : b;
    }
}
