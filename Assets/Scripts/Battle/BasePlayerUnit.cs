using UnityEngine;

public enum PlayerUnitClasses
{
    SDMN,
    PRST
}

namespace DeepFry 
{
    public class BasePlayerUnit : BaseUnit
    {
        public PlayerUnitClasses unitClass;

        public int exp, maxExp;

        public MagicSO[] learnedMagic;

        public BaseItemSO[] inventory;

        public BaseEquipment GetEquippedWeapon()
        {
            foreach (BaseItemSO item in inventory)
            {
                if (item.itemType == ItemTypes.EQUIPMENT)
                {
                    EquipmentItemSO tempEISO = (EquipmentItemSO)item;

                    if (tempEISO != null)
                    {
                        EquipmentItemSO tempEquip = (EquipmentItemSO)item;
                        if (tempEquip.GetBaseEquip().equipped && tempEquip.equipType == EquipmentTypes.WEAPON)
                        {
                            return tempEquip.GetBaseEquip();
                        }
                    }
                }                
            }
            return null;
        }
    }
}