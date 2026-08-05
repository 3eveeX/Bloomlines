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
}