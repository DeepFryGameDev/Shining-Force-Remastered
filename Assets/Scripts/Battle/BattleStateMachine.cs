using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace DeepFry
{
    public enum battleStates
    {
        BEGINTURN,
        DURINGTURN,
        ENDTURN,
        IDLE
    }

    public enum turnStates
    {
        IDLE,
        SELECT,
        MOVE,
        MENU,
        ENEMY
    }

    public class BattleStateMachine : MonoBehaviour
    {
        public List<BaseUnit> activeUnits = new List<BaseUnit>();

        BaseUnit currentUnit;
        GameObject currentUnitObject;

        Cinemachine.CinemachineFreeLook cineCam;

        public battleStates battleState = battleStates.BEGINTURN;

        BattleMenu bm;

        turnStates turnState;

        int tileLayer = 1 << 6;

        // Start is called before the first frame update
        void Start()
        {
            cineCam = FindObjectOfType<Cinemachine.CinemachineFreeLook>();

            InstantiateCombatants();
            SortUnitOrder();

            TileMenuScripts.SetTileCoordinates();

            bm = FindObjectOfType<BattleMenu>();
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
                GameObject newObject = GameObject.Instantiate(euSO.unitPrefab, new Vector3(8, 0, 13), euSO.unitPrefab.transform.rotation, GameObject.Find("[Enemy Units]").transform);

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
                GameObject newObject = GameObject.Instantiate(puSO.unitPrefab, new Vector3(7, 0, 0), puSO.unitPrefab.transform.rotation, GameObject.Find("[Player Units]").transform);

                //newObject.GetComponent<Invector.vCharacterController.vThirdPersonController>().enabled = false;
                //newObject.GetComponent<Invector.vCharacterController.vThirdPersonInput>().enabled = false;

                newObject.GetComponent<PlayerMovement>().cam = Camera.main.transform;

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
            currentUnitObject = currentUnit.unitObject;
        }

        void RunCombat()
        {
            switch (battleState)
            {
                case battleStates.BEGINTURN:

                    Debug.Log("Turn start: " + currentUnit.name);

                    // Focus camera on currentUnit
                    SetCameraFocus();

                    // if player
                    if (currentUnit is BasePlayerUnit)
                    {                      
                        currentUnit.GetUnitObject().GetComponent<PlayerTacticsMove>().FindSelectableTiles();
                        turnState = turnStates.MOVE;

                        FindObjectOfType<PlayerMovement>().canMove = true;
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
                        ProcessPlayerTurn();
                    }
                    // if enemy
                    if (currentUnit is BaseEnemyUnit)
                    {

                    }



                    break;
                case battleStates.ENDTURN:

                    // if player
                    if (currentUnit is BasePlayerUnit)
                    {
                        //currentUnit.GetUnitObject().GetComponent<Invector.vCharacterController.vThirdPersonInput>().enabled = false;
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

        void ProcessPlayerTurn()
        {
            if (Input.GetKeyDown("e"))
            {
                Debug.Log("current turnState: " + turnState);

                switch (turnState)
                {
                    case turnStates.SELECT:

                        break;
                    case turnStates.MOVE:

                        Debug.Log("Opening menu: " + currentUnit.name);
                        bm.turnState = turnStates.MENU;
                        turnState = turnStates.MENU;

                        // turn off movement
                        FindObjectOfType<PlayerMovement>().ToggleCanMove();

                        // move unit to center of tile
                        StartCoroutine(currentUnit.ResetAnimator());
                        Debug.Log("move " + currentUnitObject.gameObject.name + " to: " + TileStandingOn(currentUnitObject).transform.position);
                        currentUnitObject.transform.position = new Vector3(TileStandingOn(currentUnitObject).transform.position.x,
                            currentUnitObject.transform.position.y, TileStandingOn(currentUnitObject).transform.position.z);

                        break;

                    case turnStates.MENU:

                        break;
                }

            }

            if (Input.GetKeyDown("w"))
            {
                switch (turnState)
                {
                    case turnStates.SELECT:
                        Debug.Log("w in select"); // maybe dont need
                        break;
                    case turnStates.MENU:
                        bm.SetHovered(bm.topButton);
                        break;
                }
            }

            if (Input.GetKeyDown("d"))
            {
                switch (turnState)
                {
                    case turnStates.SELECT:
                        Debug.Log("d in select"); // maybe dont need
                        break;
                    case turnStates.MENU:
                        bm.SetHovered(bm.rightButton);
                        break;
                }
            }

            if (Input.GetKeyDown("s"))
            {
                switch (turnState)
                {
                    case turnStates.SELECT:
                        Debug.Log("s in select"); // maybe dont need
                        break;
                    case turnStates.MENU:
                        bm.SetHovered(bm.bottomButton);
                        break;
                }
            }

            if (Input.GetKeyDown("a"))
            {
                switch (turnState)
                {
                    case turnStates.SELECT:
                        Debug.Log("a in select"); // maybe dont need
                        break;
                    case turnStates.MENU:
                        bm.SetHovered(bm.leftButton);
                        break;
                }
            }
        }

        public Tile TileStandingOn(GameObject unit)
        {
            RaycastHit hit;
            Tile tile = null;

            if (Physics.Raycast(unit.transform.position, -Vector3.up, out hit, Mathf.Infinity, tileLayer))
            {
                //Debug.Log("Hit collider: " + hit.collider.gameObject.name);
                tile = hit.collider.GetComponent<Tile>();
            }

            return tile;
        }

        void SetCameraFocus()
        {
            cineCam.Follow = currentUnit.GetUnitObject().transform;
            cineCam.LookAt = currentUnit.GetUnitObject().transform;
        }

        void SetNextUnit()
        {
            activeUnits.Add(currentUnit);
            activeUnits.RemoveAt(0);

            // <--- can re-evaluate here if we want to add something to verify the unit order before setting next unit.
            currentUnit = activeUnits[0];
        }

        public void SetBattleState(battleStates newBattleState)
        {
            battleState = newBattleState;
        }
    }    
}