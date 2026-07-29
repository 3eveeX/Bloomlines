using InventorySystem;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    GameObject addItem;

    private void Start()
    {
        AddItem addItemComponent = gameObject.GetComponentInChildren<AddItem>(includeInactive: true);

        if (addItemComponent != null)
        {
            addItem = addItemComponent.gameObject;
        }
        else
        {
            Debug.LogError("AddItem component not found in children!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            addItem.SetActive(true);
            // Destroy the pickup object
            Destroy(gameObject);
        }
    }
}
