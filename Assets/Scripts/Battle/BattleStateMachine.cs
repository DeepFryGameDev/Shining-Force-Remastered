using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum battleStates
{
    BEGINTURN,
    DURINGTURN,
    ENDTURN,
    IDLE
}

namespace DeepFry
{
    public class BattleStateMachine : MonoBehaviour
    {
        public List<BaseUnit> activeUnits = new List<BaseUnit>();

        BaseUnit currentUnit;

        battleStates battleState = battleStates.BEGINTURN;

        // Start is called before the first frame update
        void Start()
        {
            InstantiateCombatants();
            SortUnitOrder();
        }

        // Update is called once per frame
        void Update()
        {
            RunCombat();
        }

        void InstantiateCombatants()
        {
            activeUnits = BattleInit.combatants;

            foreach (EnemyUnitSO euSO in BattleInit.enemyCombatants)
            {
                //Instantiate them
                GameObject newObject = GameObject.Instantiate(euSO.unitPrefab, new Vector3(6, 0, 10), euSO.unitPrefab.transform.rotation, GameObject.Find("[Enemy Units]").transform);

                foreach (BaseUnit unit in activeUnits)
                {
                    if (unit.unitType == unitTypes.ENEMY && euSO.ID == unit.ID)
                    {
                        unit.SetUnitObject(newObject);
                        break;
                    }
                }

                newObject.GetComponent<EnemyTacticsMove>().Init();
            }

            foreach (PlayerUnitSO puSO in BattleInit.playerCombatants)
            {
                //Instantiate them
                GameObject newObject = GameObject.Instantiate(puSO.unitPrefab, new Vector3(6, 0, 1), puSO.unitPrefab.transform.rotation, GameObject.Find("[Player Units]").transform);

                newObject.GetComponent<Invector.vCharacterController.vThirdPersonController>().enabled = false;
                newObject.GetComponent<Invector.vCharacterController.vThirdPersonInput>().enabled = false;

                foreach (BaseUnit unit in activeUnits)
                {
                    if (unit.unitType == unitTypes.PLAYER && puSO.ID == unit.ID)
                    {
                        unit.SetUnitObject(newObject);
                        break;
                    }
                }

                newObject.GetComponent<PlayerTacticsMove>().Init();
            }
        }

        void SortUnitOrder()
        {
            activeUnits = activeUnits.OrderBy(x => x.speed).ToList(); // better formula will be put in place later

            activeUnits.Reverse(); // Sets highest speed first

            foreach (BaseUnit bu in activeUnits)
            {
                Debug.Log(bu.name + " speed: " + bu.speed);
            }

            Debug.Log("------");
            currentUnit = activeUnits[0];
        }

        void RunCombat()
        {
            switch (battleState)
            {
                case battleStates.BEGINTURN:

                    Debug.Log("Turn start: " + currentUnit.name);

                    // Focus camera on currentUnit

                    // if player allow to move via WASD
                    if (currentUnit is BasePlayerUnit)
                    {
                        // allow unit to be moved via WASD
                        currentUnit.GetUnitObject().GetComponent<Invector.vCharacterController.vThirdPersonController>().enabled = true;
                        currentUnit.GetUnitObject().GetComponent<Invector.vCharacterController.vThirdPersonInput>().enabled = true;

                        

                        currentUnit.GetUnitObject().GetComponent<PlayerTacticsMove>().FindSelectableTiles();
                    }
                    // if enemy
                    if (currentUnit is BaseEnemyUnit)
                    {
                        // just simulate enemy taking a turn, wait a few seconds then proceed to player turn

                        currentUnit.GetUnitObject().GetComponent<EnemyTacticsMove>().Init();

                        currentUnit.GetUnitObject().GetComponent<EnemyTacticsMove>().SetActualTargetTile();
                    }

                    battleState = battleStates.DURINGTURN;
                    
                    break;
                case battleStates.DURINGTURN:

                    // if player
                    if (currentUnit is BasePlayerUnit)
                    {
                        if (Input.GetKeyDown("x"))
                        {
                            Debug.Log("Passing turn: " + currentUnit.name);
                            battleState = battleStates.ENDTURN;
                        }
                    }
                    // if enemy
                    if (currentUnit is BaseEnemyUnit)
                    {
                        if (Input.GetKeyDown("x"))
                        {
                            Debug.Log("Passing turn: " + currentUnit.name);
                            battleState = battleStates.ENDTURN;
                        }                        
                    }



                    break;
                case battleStates.ENDTURN:

                    // if player
                    if (currentUnit is BasePlayerUnit)
                    {
                        currentUnit.GetUnitObject().GetComponent<Invector.vCharacterController.vThirdPersonInput>().enabled = false;
                    }
                    // if enemy
                    if (currentUnit is BaseEnemyUnit)
                    {
                        
                    }

                    SetNextUnit();
                    battleState = battleStates.BEGINTURN;

                    break;
                case battleStates.IDLE:

                    break;
            }
        }

        void SetNextUnit()
        {
            activeUnits.Add(currentUnit);
            activeUnits.RemoveAt(0);

            // <--- can re-evaluate here if we want to add something to verify the unit order before setting next unit.
            currentUnit = activeUnits[0];
        }
    }    
}