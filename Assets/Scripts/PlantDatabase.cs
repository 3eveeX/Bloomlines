using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Plants/PlantDatabase")]
public class PlantDatabase : ScriptableObject
{
    [System.Serializable]
    public struct SeedEntry
    {
        public GameObject seedRelatedObject; 
        public PlantData plantData;
    }

    public List<SeedEntry> entries;

    public PlantData GetPlantData(InventoryItem seed)
    {
        foreach (var entry in entries)
        {
            if (entry.seedRelatedObject == seed.GetRelatedGameObject())
                return entry.plantData;
        }
        return null;
    }

    // PlantDatabase
    public bool TryGetSeedType(string itemType, out string seedType)
    {
        foreach (var entry in entries)
        {
            if (entry.plantData != null && entry.plantData.plantName == itemType)
            {
                seedType = entry.plantData.plantName + "Seed";
                return true;
            }
        }
        seedType = null;
        return false;
    }

}