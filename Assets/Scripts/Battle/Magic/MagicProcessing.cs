using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class MagicProcessing : MonoBehaviour
    {
        public BaseMagic currentMagic;
        public BasePlayerUnit currentPlayerUnit;

        BattleMenu bm;
        CombatInteraction ci;

        TileSelection ts;
        TileTargetProcessing ttp;

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

        public void MagicChosen(BaseMagic magic, BasePlayerUnit bpu)
        {
            currentPlayerUnit = bpu;
            currentMagic = magic;

            ttp.currentItem = null;

            if (currentPlayerUnit.MP >= currentMagic.mpCost)
            {
                // can cast
                ttp.currentMagic = currentMagic;

                ttp.BeginTileSelectForAction();

                ttp.currentBTT = ttp.GetBaseTileTarget(currentPlayerUnit, currentMagic);
                ttp.currentMagic = currentMagic;
            } else
            {
                // cannot cast. play a SE or something
            }
        }

        void Egress()
        {
            Debug.Log(ts.targetUnit.name + " - Returning to last save point.");
        }

        void Heal()
        {
            Debug.Log(ts.targetUnit.name + " recovers " + currentMagic.value + " HP.");
        }

        public void ExecuteMagic()
        {
            Debug.Log("Casting " + currentMagic.name + " on " + ts.targetUnit.name);

            Invoke(FormatName(currentMagic.name), 0.0f);

            List<BaseUnit> newUnitList = new List<BaseUnit>();
            newUnitList.Add(ts.targetUnit);

            ci.SetNewBaseCombatInteraction(CombatInteractionTypes.MAGIC, currentPlayerUnit, newUnitList, currentMagic);
            ci.BuildNewCombatInteraction();

            ts.mainCam.SetActive(true);
            ts.selectionCam.SetActive(false);

            PostMagicProcessing();
        }

        void PostMagicProcessing()
        {
            currentPlayerUnit.MP -= currentMagic.mpCost;
        }

        string FormatName(string s)
        {
            return s.Replace(" ", "");
        }
    }
}