using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class DB : MonoBehaviour
    {
        public List<PlayerUnitSO> allPlayerUnits = new List<PlayerUnitSO>();
        public List<BasePlayerUnit> activePlayerUnits = new List<BasePlayerUnit>();

        public List<UsableItemSO> usableItems = new List<UsableItemSO>();
        public List<EquipmentItemSO> equipmentItems = new List<EquipmentItemSO>();

        public static DB GameDB { get; private set; }

        private void Awake()
        {
            if (GameDB != null && GameDB != this)
            {
                Destroy(this);
            }
            else
            {
                GameDB = this;
            }
        }

        public BaseEquipment GetEquipmentItem(int ID)
        {
            foreach (EquipmentItemSO eiSO in equipmentItems)
            {
                if (eiSO.ID == ID)
                {
                    return eiSO.GetBaseEquip();
                }
            }

            return null;
        }
    }
}