using System.Collections.Generic;
using UnityEngine;

public enum PlayerUnitClasses
{
    SDMN,
    PRST,
    KNTE
}

namespace DeepFry 
{
    public class BasePlayerUnit : BaseUnit
    {
        public PlayerUnitClasses unitClass;

        public int exp, maxExp;

        public MagicSO[] learnedMagicSOs;

        public List<BaseItem> inventory = new List<BaseItem>();

        BaseEquipment equippedWeapon;
        BaseEquipment equippedArmor;

        public void Init()
        {
            InitInventory();
        }

        private void InitInventory()
        {
            Debug.Log("Initializing inventory for " + name);

            inventory.Clear();

            foreach (BaseItemSO itemSO in items)
            {
                Debug.Log("Adding " + itemSO.name + " to inventory for " + name);
                inventory.Add(itemSO.GetItem());
            }

            SetInitialEquip();
        }

        void SetInitialEquip()
        {
            switch (ID)
            {
                case 0: // Bowie
                    EquipWeapon(DB.GameDB.GetEquipmentItem(1));
                    break;
                case 1: // Sarah
                    EquipWeapon(DB.GameDB.GetEquipmentItem(2));
                    break;
                case 2:  //Chester

                    break;
            }
        }

        public void EquipWeapon(BaseEquipment equip)
        {
            UnequipWeapon();
            equippedWeapon = equip;

            // update stats
        }

        public void UnequipWeapon()
        {
            equippedWeapon = null;

            // update stats
        }

        public BaseEquipment GetEquippedWeapon()
        {
            return equippedWeapon;
        }

        public void AddToInventory(BaseItem item)
        {
            inventory.Add(item);
        }

        public void RemoveFromInventory(BaseItem item)
        {            
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].ID == item.ID)
                {
                    inventory.RemoveAt(i);
                }
            }
        }

        public bool HasInventorySpace()
        {
            if (inventory.Count < 4)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool HasItemsInInventory()
        {
            Debug.Log("Inventory count: " + inventory.Count + " for " + name);
            if (inventory.Count > 0 && inventory.Count <= 4) { return true; } else { return false; };
        }
    }
}