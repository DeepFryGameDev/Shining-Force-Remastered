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

        public bool dbInitialized;

        private void Awake()
        {
            if (GameDB != null && GameDB != this)
            {
                Destroy(this);
            }
            else
            {
                GameDB = this;
                DontDestroyOnLoad(this);
            }
        }

        private void Update()
        {
            // in a build, this will not be needed - it is only for editor use. (We will need to update somewhere to set cursor to hidden)
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
            }
        }

        public void SetInitialPlayerUnits(List<PlayerUnitSO> unitSOs)
        {
            foreach (PlayerUnitSO puSO in unitSOs)
            {
                Debug.Log("-~-~-~- " + puSO.name + " added to active player units. -~-~-~-");
                DB.GameDB.activePlayerUnits.Add(puSO.GetPlayerUnit());
            }

            dbInitialized = true;
        }

        public void SetNewPlayerUnits(List<BasePlayerUnit> newUnits)
        {
            activePlayerUnits = newUnits;
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