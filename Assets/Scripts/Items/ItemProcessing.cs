using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class ItemProcessing : MonoBehaviour
    {
        public BaseItem currentItem;
        public BasePlayerUnit currentPlayerUnit;

        BattleMenu bm;
        TileSelection ts;
        TileTargetProcessing ttp;

        // Start is called before the first frame update
        void Start()
        {
            ts = FindObjectOfType<TileSelection>();
            ttp = FindObjectOfType<TileTargetProcessing>();
            bm = FindObjectOfType<BattleMenu>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ItemChosen(BaseItem item, BasePlayerUnit bpu)
        {
            currentPlayerUnit = bpu;
            currentItem = item;

            ttp.currentMagic = null;

            Debug.Log("Item chosen: " + item.name);
            ttp.BeginTileSelectForAction();

            ttp.currentBTT = ttp.GetBaseTileTarget(currentPlayerUnit, currentItem);
            ttp.currentItem = currentItem;
        }

        void MedicalHerb()
        {
            Debug.Log("Healing " + ts.targetUnit.name + " for " + currentItem.value);
        }

        public void ExecuteItem()
        {
            switch (bm.itemMenuMode)
            {
                case itemMenuModes.USE:
                    Debug.Log("Using " + currentItem.name);

                    Invoke(FormatName(currentItem.name), 0.0f);

                    PostItemProcessing();
                    break;
                case itemMenuModes.GIVE:

                    break;
            }
        }

        void PostItemProcessing()
        {
            // remove item from inventory
            RemoveFromInventory(currentItem, currentPlayerUnit);
        }

        public void RemoveFromInventory(BaseItem item, BasePlayerUnit bpu)
        {
            Debug.Log("Removing from " + bpu.name + "'s inventory: " + item.name);
        }

        string FormatName(string s)
        {
            return s.Replace(" ", "");
        }
    }
}