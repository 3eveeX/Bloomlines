using System;
using UnityEngine;

namespace InventorySystem
{
    //Author: Jaxon Schauer
    /// <summary>
    /// Contains information about an item to be passed through code
    /// </summary>

    [System.Serializable]
    public class InventoryItem
    {
        private int amount;

        string itemType;
        private Sprite itemImage;//Holds image of item
        private int maxStackAmount;
        private bool draggable;
        private bool pressable;
        private InventoryItemEvent itemEvent;
        private GameObject relatedGameObject;
        private bool isNull = false;//Checks if item exists
        private bool displayAmount;
        private int position;
        private string inventory;
        private string previousInventory;

        // --- BLOOMLINES ADDITION (not part of the original asset) ---
        // Coat-check ticket into SeedDesk. 0 means "no genetics attached",
        // which is every non-seed item and every wild/shop seed.
        private int seedTicket = 0;
        // ------------------------------------------------------------
        public InventoryItem(ItemInitializer init)
        {
            this.amount = 1;
            this.itemType = init.GetItemType();
            this.itemImage = init.GetItemImage();
            this.maxStackAmount = init.GetItemStackAmount();
            this.draggable = init.GetDraggable();
            this.pressable = init.GetPressable();
            this.itemEvent = init.GetEvent();
            this.isNull = init.GetIsNull();
            this.relatedGameObject = init.GetRelatedGameObject();
            this.displayAmount = init.GetDisplayAmount();
        }
        public InventoryItem(InventoryItem other, int amount = 1)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Passed argument is null");
            }

            this.amount = amount;
            this.itemType = other.itemType != null ? string.Copy(other.itemType) : null;
            this.itemImage = other.itemImage;

            this.maxStackAmount = other.maxStackAmount;
            this.draggable = other.draggable;
            this.pressable = other.pressable;
            this.itemEvent = other.itemEvent;
            this.isNull = other.isNull;
            this.relatedGameObject = other.GetRelatedGameObject();
            this.displayAmount = other.GetDisplayAmount();
            this.inventory = other.inventory;
            this.position = other.position;
            this.previousInventory= other.previousInventory;
            this.seedTicket = other.seedTicket; // BLOOMLINES: carry genetics through copies
        }

        public InventoryItem(bool isNull)
        {
            amount = 1;
            this.isNull = isNull;
        }
        public void SetIsNull(bool isNull)
        {
            this.isNull = isNull;
        }
        public bool GetIsNull()
        {
            return isNull;
        }
        public string GetItemType()
        {
            return itemType;
        }
        public Sprite GetItemImage()
        {
            return itemImage;
        }
        public int GetItemStackAmount()
        {
            return maxStackAmount;
        }
        public void Selected()
        {
            if (itemEvent != null)
                itemEvent.Invoke(this);
        }
        public bool GetPressable()
        {
            return pressable;
        }
        public bool GetDraggable()
        {
            return draggable;
        }
        public int GetAmount()
        {
            return amount;
        }
        public void SetAmount(int amount)
        {
            this.amount = amount;
        }
        public bool GetDisplayAmount()
        {
            return displayAmount;
        }
        public GameObject GetRelatedGameObject()
        {
            return relatedGameObject;
        }
        public void SetPressable(bool pressable)
        {
            this.pressable = pressable;
        }
        public void SetPosition(int position)
        {
            this.position = position;
        }
        public int GetPosition()
        {
            return position;
        }
        public void SetInventory(string inventory)
        {
            previousInventory = this.inventory;
            this.inventory = inventory;
        }
        public string GetInventory()
        {
            return inventory;
        }
        // --- BLOOMLINES ADDITION (not part of the original asset) ---
        public int GetSeedTicket()
        {
            return seedTicket;
        }
        public void SetSeedTicket(int ticket)
        {
            this.seedTicket = ticket;
        }
        // ------------------------------------------------------------

        public override string ToString()
        {
            string result = $@"
            ItemType: {itemType}
            Inventory: {inventory}
            previousInventory: {previousInventory}
            Item Position: {position}
            Item Amount: {amount}
            Max Item Amount: {maxStackAmount}";
            return result;



        }
    }
}
