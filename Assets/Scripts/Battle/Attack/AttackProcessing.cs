using DeepFry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackProcessing : MonoBehaviour
{
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

    public void ExecuteAttack()
    {
        Debug.Log("Attacking " + ts.targetUnit.name);

        List<BaseUnit> newUnitList = new List<BaseUnit>();
        newUnitList.Add(ts.targetUnit);

        ci.SetNewBaseCombatInteraction(CombatInteractionTypes.ATTACK, currentPlayerUnit, newUnitList);
        ci.BuildNewCombatInteraction();

        ts.mainCam.SetActive(true);
        ts.selectionCam.SetActive(false);
    }

    public void AttackChosen(BasePlayerUnit bpu)
    {
        currentPlayerUnit = bpu;

        ttp.currentItem = null;
        ttp.currentMagic = null;

        // if any targets in range
        if (TargetsInRange(currentPlayerUnit.GetEquippedWeapon().attackRange))
        {
            bm.HideMenu(bm.mainMenu);

            ttp.BeginTileSelectForAction();
            ttp.currentBTT = ttp.GetBaseTileTarget(currentPlayerUnit);
        }
        else // else, play error SE and message 'no targets available'
        {
            Debug.Log("No targets in range");
        }
    }

    private bool TargetsInRange(int range)
    {
        foreach (Tile tile in ttp.GetTargetTiles(range))
        {
            if (ttp.GetUnitOnTile(tile) != null && ttp.GetUnitOnTile(tile).unitType == unitTypes.ENEMY)
            {
                return true;
            }
        }

        return false;
    }
}
