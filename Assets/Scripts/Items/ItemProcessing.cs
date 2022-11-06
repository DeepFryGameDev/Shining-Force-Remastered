using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class ItemProcessing : MonoBehaviour
    {
        public BaseItem currentItem;
        public BaseUsableItem currentUsableItem;
        public BaseEquipment currentEquipmentItem;
        public BasePlayerUnit currentPlayerUnit;

        BattleMenu bm;
        TileSelection ts;
        TileTargetProcessing ttp;
        CombatInteraction ci;

        MagicItemMenu mim;

        // Start is called before the first frame update
        void Start()
        {
            ts = FindObjectOfType<TileSelection>();
            ttp = FindObjectOfType<TileTargetProcessing>();
            bm = FindObjectOfType<BattleMenu>();
            ci = FindObjectOfType<CombatInteraction>();
            mim = FindObjectOfType<MagicItemMenu>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PrepareItemTileSelection(BaseUsableItem item, BasePlayerUnit bpu)
        {
            currentPlayerUnit = bpu;
            currentItem = item;
            currentUsableItem = item;
            currentEquipmentItem = null;

            ttp.currentMagic = null;

            Debug.Log("Item chosen: " + item.name);
            ttp.BeginTileSelectForAction();

            ttp.currentBTT = ttp.GetBaseTileTarget(currentPlayerUnit, item); // <--- here need to update i think

            ttp.currentItem = currentItem;
            ttp.currentUsableItem = item;
            ttp.currentEquipment = null;
        }

        public void PrepareItemTileSelection(BaseEquipment equip, BasePlayerUnit bpu)
        {
            currentPlayerUnit = bpu;
            currentItem = equip;
            currentUsableItem = null;
            currentEquipmentItem = equip;

            ttp.currentMagic = null;

            Debug.Log("Item chosen: " + equip.name);
            ttp.BeginTileSelectForAction();

            ttp.currentBTT = ttp.GetBaseTileTarget(currentPlayerUnit, equip); // <--- here need to update i think

            ttp.currentItem = currentItem;
            ttp.currentUsableItem = null;
            ttp.currentEquipment = equip;
        }

        void MedicalHerb()
        {
            BaseUsableItem tempItem = (BaseUsableItem)currentItem;
            Debug.Log("Healing " + ts.targetUnit.name + " for " + tempItem.value);
        }

        public void ExecuteItem()
        {
            switch (bm.itemMenuMode)
            {
                case itemMenuModes.USE:
                    Debug.Log("Using " + currentItem.name);

                    Invoke(FormatName(currentItem.name), 0.0f);

                    List<BaseUnit> newUnitList = new List<BaseUnit>
                    {
                        ts.targetUnit
                    };

                    BaseUsableItem tempItem = (BaseUsableItem)currentItem;

                    ci.SetNewBaseCombatInteraction(CombatInteractionTypes.ITEM, currentPlayerUnit, newUnitList, tempItem);
                    ci.BuildNewCombatInteraction();

                    ts.mainCam.SetActive(true);
                    ts.selectionCam.SetActive(false);

                    PostItemProcessing(); // - needs to be done after combat interaction is finished.. add a check here to verify combat interaction is complete before proceeding
                    break;
                case itemMenuModes.GIVE:
                    BasePlayerUnit tempBPU = (BasePlayerUnit)ts.targetUnit;
                    StartCoroutine(GiveItem(tempBPU));

                    break;
            }
        }

        IEnumerator GiveItem(BasePlayerUnit targetUnit)
        {           
            if (targetUnit.HasInventorySpace())
            {
                targetUnit.AddToInventory(currentItem); // add item to target unit
                currentPlayerUnit.RemoveFromInventory(currentItem); // remove item from current unit

                Debug.Log("Giving " + currentItem.name + " to " + targetUnit.name);

                yield return ci.DisplayMessage(currentItem.name + " has been given to " + targetUnit.name + ".", ci.messageDelay);
                ci.ClearMessageText();

                // prepare for next turn
                ts.mainCam.SetActive(true);
                ts.selectionCam.SetActive(false);

                // move to next turn
                bm.EndTurn();
            }
        }

        public IEnumerator DropItem(BasePlayerUnit unit, BaseItem item)
        {
            unit.RemoveFromInventory(item); // remove item from current unit

            Debug.Log("Dropping " + item.name + " from " + unit.name);

            yield return ci.DisplayMessage(item.name + " has been dropped.", ci.messageDelay);
            ci.ClearMessageText();

            // prepare for next turn
            ts.mainCam.SetActive(true);
            ts.selectionCam.SetActive(false);

            // move to next turn
            bm.EndTurn();
        }


        public IEnumerator EquipItem(BasePlayerUnit unit, BaseItem item)
        {
            // if already equipped, just present a message that its been equipped and open the mim back up to show items
            if (unit.GetEquippedWeapon().ID == item.ID)
            {
                yield return ci.DisplayMessage(item.name + " is already equipped.", ci.messageDelay);
                ci.ClearMessageText();

                mim.SetAndShowUnitEquipment(unit);
                mim.ShowMenu();
            } else
            {
                // if not, say that it's been equipped and end the turn
                unit.EquipWeapon(DB.GameDB.GetEquipmentItem(item.ID));

                yield return ci.DisplayMessage(item.name + " has been equipped.", ci.messageDelay);
                ci.ClearMessageText();

                // prepare for next turn
                ts.mainCam.SetActive(true);
                ts.selectionCam.SetActive(false);

                // move to next turn
                bm.EndTurn();
            }            
        }

        void PostItemProcessing()
        {
            // remove item from inventory
            currentPlayerUnit.RemoveFromInventory(currentItem);
        }

        string FormatName(string s)
        {
            return s.Replace(" ", "");
        }
    }
}