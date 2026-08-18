using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Plants/PlantData")]
public class PlantData : ScriptableObject
{
    public string plantName;
    public Mesh[] growthStageMeshes; 
    public int maxGrowthStage = 4;
    public float baseStageTime = 20f;
    public GameObject grownPlantPrefab; 
    public string harvestedItemType; 
    public int baseYieldAmount = 1;
    public List<MutationResult> mutations;

    [System.Serializable]
    public class MutationResult
    {
        public PlantData partner;
        public PlantData result;
        [Range(0f, 1f)] public float chance = 0.1f;
    }
}