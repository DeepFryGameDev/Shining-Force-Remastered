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
        BattleStateMachine bsm;
        CombatInteraction ci;

        AudioManager am;

        TileSelection ts;
        TileTargetProcessing ttp;

        // Start is called before the first frame update
        void Start()
        {
            ts = FindObjectOfType<TileSelection>();
            ttp = FindObjectOfType<TileTargetProcessing>();
            bm = FindObjectOfType<BattleMenu>();
            ci = FindObjectOfType<CombatInteraction>();
            bsm = FindObjectOfType<BattleStateMachine>();

            am = FindObjectOfType<AudioManager>();
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
                // cannot cast. play a SE
                am.PlayUI(UISoundEffects.INVALIDACTION);
            }
        }

        void Egress()
        {
            Debug.Log(ts.targetUnit.name + " - Returning to last save point.");
            bsm.DoPostBattleThings(0);
        }

        void Heal()
        {
            Debug.Log(ts.targetUnit.name + " recovers " + currentMagic.value + " HP.");
        }

        public void PrepareMagic()
        {
            Debug.Log("Casting " + currentMagic.name + " on " + ts.targetUnit.name);

            List<BaseUnit> newUnitList = new List<BaseUnit>();
            newUnitList.Add(ts.targetUnit);

            ci.SetNewBaseCombatInteraction(CombatInteractionTypes.MAGIC, currentPlayerUnit, newUnitList, currentMagic);
            ci.BuildNewCombatInteraction();

            ts.mainCam.SetActive(true);
            ts.selectionCam.SetActive(false);

            PostMagicProcessing();
        }

        public void ExecuteMagic(string name)
        {
            Invoke(FormatName(name), 0.0f);
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