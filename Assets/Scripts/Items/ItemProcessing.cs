using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class ItemProcessing : MonoBehaviour
    {
        public BaseUsableItem currentItem;
        public BasePlayerUnit currentPlayerUnit;

        BattleMenu bm;
        TileSelection ts;
        TileTargetProcessing ttp;
        CombatInteraction ci;

        // Start is called before the first frame update
        void Start()
        {
            ts = FindObjectOfType<TileSelection>();
            ttp = FindObjectOfType<TileTargetProcessing>();
            bm = FindObjectOfType<BattleMenu>();
            ci = FindObjectOfType<CombatInteraction>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ItemChosen(BaseUsableItem item, BasePlayerUnit bpu)
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

                    List<BaseUnit> newUnitList = new List<BaseUnit>();
                    newUnitList.Add(ts.targetUnit);

                    ci.SetNewBaseCombatInteraction(CombatInteractionTypes.ITEM, currentPlayerUnit, newUnitList, currentItem);
                    ci.BuildNewCombatInteraction();

                    ts.mainCam.SetActive(true);
                    ts.selectionCam.SetActive(false);

                    // PostItemProcessing(); // - needs to be done after combat interaction is finished.. add a check here to verify combat interaction is complete before proceeding
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