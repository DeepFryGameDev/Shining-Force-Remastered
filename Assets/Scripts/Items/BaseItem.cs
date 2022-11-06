using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemTypes
{
    USABLE,
    EQUIPMENT,
    KEY
}
public enum UsableItemTypes
{
    HEAL,
    EFFECT,
    ATTACK
}

public enum EquipmentTypes
{
    WEAPON,
    ACCESSORY
}

namespace DeepFry
{
    public class BaseItem
    {
        public int ID;
        public string name;

        public Sprite icon;

        public int gilValue;

        public ItemTypes itemType;

        public BaseUsableItem GetBaseUsableItem()
        {
            BaseUsableItem item = new BaseUsableItem
            {
                ID = ID,
                name = name,
                icon = icon,
                gilValue = gilValue,
                itemType = itemType
            };

            return item;
        }
    }
}