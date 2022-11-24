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

    AudioManager am;

    // Start is called before the first frame update
    void Start()
    {
        ts = FindObjectOfType<TileSelection>();
        ttp = FindObjectOfType<TileTargetProcessing>();
        bm = FindObjectOfType<BattleMenu>();
        ci = FindObjectOfType<CombatInteraction>();

        am = FindObjectOfType<AudioManager>();
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

        Debug.Log(currentPlayerUnit.name + " equipped weapon: " + currentPlayerUnit.GetEquippedWeapon().name);

        // if any targets in range
        if (TargetsInRange(bpu, currentPlayerUnit.GetEquippedWeapon().attackRange))
        {
            bm.HideMenu(bm.mainMenu, true);

            ttp.BeginTileSelectForAction();
            ttp.currentBTT = ttp.GetBaseTileTarget(currentPlayerUnit);
        }
        else // else, play error SE and message 'no targets available'
        {
            Debug.Log("No targets in range");
            am.PlayUI(UISoundEffects.INVALIDACTION);
        }
    }

    private bool TargetsInRange(BaseUnit homeUnit, int range)
    {
        foreach (Tile tile in ttp.GetTargetTiles(homeUnit.GetTile(), range))
        {
            if (ttp.GetUnitOnTile(tile) != null && ttp.GetUnitOnTile(tile).unitType == unitTypes.ENEMY)
            {
                return true;
            }
        }

        return false;
    }
}
