using System.Collections;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public PlantData data;
    public PlantStats stats = new PlantStats();
    public int growthStage = 0;

    public bool IsFullyGrown => growthStage >= data.maxGrowthStage;

    [SerializeField]
    private GameObject[] stageModels; 

    private void Start()
    {
        UpdateVisual();

        if (growthStage < data.maxGrowthStage)
            StartCoroutine(GrowNextStage());
    }

    private IEnumerator GrowNextStage()
    {
        float waitTime = CalculateStageTime();
        yield return new WaitForSeconds(waitTime);

        growthStage++;
        UpdateVisual();
        Debug.LogWarning($"Plant {data.plantName} has grown to stage {growthStage}.");

        if (growthStage < data.maxGrowthStage)
            StartCoroutine(GrowNextStage());
        else
            OnFullyGrown();
    }

    private float CalculateStageTime()
    {
        float time = data.baseStageTime / stats.growthSpeedMultiplier;
        float variance = Random.Range(-0.2f, 0.2f);
        return Mathf.Max(1f, time * (1f + variance));
    }

    private void UpdateVisual()
    {
        if (stageModels == null || stageModels.Length == 0) return;

        for (int i = 0; i < stageModels.Length; i++)
        {
            if (stageModels[i] != null)
                stageModels[i].SetActive(i == growthStage);
        }
    }

    private void OnFullyGrown()
    {
        Debug.Log($"{data.plantName} is fully grown!");
    }
}