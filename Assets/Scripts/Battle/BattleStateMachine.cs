using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.SceneManagement;

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

    public enum TargetTypes
    {
        SELF,
        PLAYER,
        ENEMY,
        ANY
    }

    struct UnitTurnCalc
    {
        public BaseUnit unit;
        public float turnValue;
    }

    public class BattleStateMachine : MonoBehaviour
    {
        public List<BaseUnit> activeUnits = new List<BaseUnit>();

        public BaseUnit currentUnit;

        Cinemachine.CinemachineFreeLook cineCam;

        public battleStates battleState = battleStates.BEGINTURN;

        BattleMenu bm;

        public turnStates turnState;

        int tileLayer = 1 << 6;

        float cameraFollowDistance = 5f;

        public bool selectModeReady, selectMenuOpen;

        StatusMenu sm;

        bool cameraFocused, runSelectModeStuffOnce, onLastTile;
        public bool selectableTilesFound;

        public PlayerTacticsMove workingPTM;
        EnemyTacticsMove workingETM;
        public Tile lastTile;

        TileSelection ts;
        BattleCamera batCam;

        MoveCanvas mc;

        MenuPrefabManager mpm;

        Queue<BaseUnit> turnQueue = new Queue<BaseUnit>();

        bool bossEncounter;

        // Start is called before the first frame update
        void Start()
        {
            cineCam = FindObjectOfType<Cinemachine.CinemachineFreeLook>();

            TileMenuScripts.SetTileCoordinates();

            bm = FindObjectOfType<BattleMenu>();
            ts = GetComponent<TileSelection>();

            mpm = FindObjectOfType<MenuPrefabManager>();
            sm = FindObjectOfType<StatusMenu>();

            batCam = FindObjectOfType<BattleCamera>();
            mc = FindObjectOfType<MoveCanvas>();

            cameraFocused = false;
            runSelectModeStuffOnce = false;
            selectModeReady = false;
            onLastTile = false;
            selectableTilesFound = false;

            Init();
        }

        void Init()
        {
            InstantiateCombatants();
        }

        // Update is called once per frame
        void Update()
        {
            RunCombat();
        }

        void InstantiateCombatants()
        {
            activeUnits = BattleInit.combatants;

            foreach (BaseEnemyEncounter bee in BattleInit.enemyCombatants)
            {
                //Instantiate them
                Debug.Log(ts.GetTileAtCoords(bee.spawnCoordinates.x, bee.spawnCoordinates.y).name);
                GameObject newObject = GameObject.Instantiate(bee.enemy.unitPrefab, 
                    new Vector3(bee.spawnCoordinates.y,
                    ts.GetTileAtCoords(bee.spawnCoordinates.x, bee.spawnCoordinates.y).transform.position.y + 0.2f, // used to be 0
                    bee.spawnCoordinates.x), 
                    bee.enemy.unitPrefab.transform.rotation, GameObject.Find("[Enemy Units]").transform); 

                foreach (BaseUnit unit in activeUnits)
                {
                    if (unit.unitType == unitTypes.ENEMY && unit.battleID == bee.battleID)
                    {
                        if (bee.enemy.boss)
                        {
                            bossEncounter = true;
                        }

                        unit.SetUnitObject(newObject);
                        unit.GetUnitObject().GetComponent<TacticsMove>().unit = unit;
                        unit.GetUnitObject().GetComponent<EnemyTacticsMove>().enemyUnit = (BaseEnemyUnit)unit;
                        break;
                    }
                    
                }

                newObject.GetComponent<EnemyTacticsMove>().Init();
            }

            foreach (BasePlayerUnit bpu in BattleInit.playerCombatants)
            {
                bpu.Init();

                GameObject newObject = null;

                //Instantiate them - this will need an overhaul later, but as there is only 1 fight, they can be instantiated in the same place
                switch (bpu.ID)
                {
                    case 0: // bowie
                        newObject = GameObject.Instantiate(bpu.unitPrefab, new Vector3(7, 0, 0), bpu.unitPrefab.transform.rotation, GameObject.Find("[Player Units]").transform);
                        break;
                    case 1: // sarah
                        newObject = GameObject.Instantiate(bpu.unitPrefab, new Vector3(8, 0, 0), bpu.unitPrefab.transform.rotation, GameObject.Find("[Player Units]").transform);
                        break;
                    case 2: // chester
                        newObject = GameObject.Instantiate(bpu.unitPrefab, new Vector3(6, 0, 0), bpu.unitPrefab.transform.rotation, GameObject.Find("[Player Units]").transform);
                        break;
                }
                

                //newObject.GetComponent<Invector.vCharacterController.vThirdPersonController>().enabled = false;
                //newObject.GetComponent<Invector.vCharacterController.vThirdPersonInput>().enabled = false;

                newObject.GetComponent<PlayerMovement>().cam = Camera.main.transform;

                foreach (BaseUnit unit in activeUnits)
                {
                    if (unit.unitType == unitTypes.PLAYER && bpu.ID == unit.ID)
                    {
                        unit.SetUnitObject(newObject);
                        unit.GetUnitObject().GetComponent<TacticsMove>().unit = unit;

                        Vector3 newRot = new Vector3(unit.GetUnitObject().transform.rotation.x, 0, unit.GetUnitObject().transform.rotation.z);
                        unit.GetUnitObject().transform.eulerAngles = newRot;

                        foreach (BasePlayerUnit playerUnit in BattleInit.playerCombatants)
                        {
                            if (playerUnit.battleID == unit.battleID)
                            {
                                unit.GetUnitObject().GetComponent<PlayerUnitObject>().playerUnit = playerUnit;
                                Debug.Log(playerUnit.name + " playerUnit set");
                                break;
                            }
                        }

                        ToggleMovement(unit, false);
                        break;
                    }
                }

                newObject.GetComponent<PlayerTacticsMove>().Init();

                // reset animator workaround:
                newObject.SetActive(false);
                newObject.SetActive(true);
            }

            SetTurnOrder();
        }

        float GetRandomBetweenRange(float min, float max)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            return UnityEngine.Random.Range(min, max + 1);
        }

        void RunCombat()
        {
            //Debug.Log(battleState);
            switch (battleState)
            {
                case battleStates.BEGINTURN:
                    // Focus camera on currentUnit
                    SetCameraFocus();

                    IfCameraIsFocused();

                    if (cameraFocused)
                    {
                        Debug.Log("Turn start: " + currentUnit.name);                                              

                        // if player
                        if (currentUnit is BasePlayerUnit)
                        {
                            batCam.EnableCameraFreeLook(true);
                            workingPTM = currentUnit.GetUnitObject().GetComponent<PlayerTacticsMove>();

                            workingPTM.GetCurrentTile();
                            //Debug.Log("Current tile: " + workingPTM.currentTile.name);
                            //Debug.Log("Last tile: " + workingPTM.currentTile.name);
                            workingPTM.lastTile = workingPTM.currentTile;
                            lastTile = workingPTM.lastTile;
                            //Debug.Log("Current tile after: " + workingPTM.currentTile.name);
                            //Debug.Log("Last tile after: " + workingPTM.currentTile.name);

                            mc.DrawCanvas((BasePlayerUnit)currentUnit, lastTile);
                            mc.ToggleMenu(true);

                            if (!selectableTilesFound)
                            {
                                workingPTM.FindSelectableTiles();
                                workingPTM.SetLastTile();

                                selectableTilesFound = true;
                            }
                            turnState = turnStates.MOVE;

                            StartCoroutine(currentUnit.ResetAnimator());

                            Debug.Log("Setting movement to true: " + currentUnit.GetUnitObject().gameObject.name);
                            ToggleMovement(currentUnit, true);

                            battleState = battleStates.DURINGTURN;
                        }
                        // if enemy
                        if (currentUnit is BaseEnemyUnit)
                        {
                            battleState = battleStates.DURINGTURN;

                            batCam.EnableCameraFreeLook(false);
                            workingETM = currentUnit.GetUnitObject().GetComponent<EnemyTacticsMove>();
                            // just simulate enemy taking a turn, wait a few seconds then proceed to player turn

                            workingETM.GetCurrentTile();
                            workingETM.lastTile = workingETM.currentTile;
                            lastTile = workingETM.lastTile;
                            workingETM.SetLastTile();
                            workingETM.Init();
                            workingETM.SetActualTargetTile();

                            //Debug.Log("Setting to during turn");                            
                        }
                    }
                    
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
                    // check if game over
                    Debug.Log("Checking if game over");
                    bool bossFound = false;
                    bool enemyFound = false;

                    if (bossEncounter)
                    {                        
                        foreach (BaseUnit unit in activeUnits)
                        {
                            if (unit.unitType == unitTypes.ENEMY && !bossFound)
                            {
                                foreach (BaseEnemyEncounter bee in BattleInit.enemyCombatants)
                                {
                                    if (bee.battleID == unit.battleID && bee.enemy.boss)
                                    {
                                        Debug.Log("Boss found. Moving on");
                                        bossFound = true;
                                        break;
                                    } 
                                }
                            }
                        }
                    } else
                    {
                        foreach (BaseUnit unit in activeUnits)
                        {
                            if (unit.unitType == unitTypes.ENEMY)
                            {
                                Debug.Log("Enemies still exist. Moving on");
                                enemyFound = true;
                                break;
                            }
                        }
                    }

                    if ((!bossEncounter && enemyFound) || (bossEncounter && bossFound)) // continue on
                    {
                        // if player
                        if (currentUnit is BasePlayerUnit)
                        {
                            //currentUnit.GetUnitObject().GetComponent<Invector.vCharacterController.vThirdPersonInput>().enabled = false;
                            bm.ResetForNewTurn();

                            mc.ToggleMenu(false);

                            workingPTM.lastTile = null;

                            workingPTM.tempSelectableTiles.Clear();
                            selectableTilesFound = false;
                        }
                        // if enemy
                        if (currentUnit is BaseEnemyUnit)
                        {
                            workingETM.tempSelectableTiles.Clear();
                        }

                        SetNextUnit();
                        battleState = battleStates.BEGINTURN;
                    } else
                    {
                        Debug.Log("Battle over!");
                        DoPostBattleThings(0);
                    }

                    //SetNextUnit();
                    break;
                case battleStates.IDLE:

                    break;
            }
        }

        public void DoPostBattleThings(int sceneToLoad)
        {
            // save stats updated from the battle onto the DB units
            List<BasePlayerUnit> tempUnits = new List<BasePlayerUnit>();
            foreach (BaseUnit unit in activeUnits)
            {
                if (unit.unitType == unitTypes.PLAYER)
                {
                    if (unit.GetUnitObject() == null) // they died in battle
                    {
                        foreach (BasePlayerUnit bpu in DB.GameDB.activePlayerUnits)
                        {
                            if (bpu.battleID == unit.battleID)
                            {
                                BasePlayerUnit tempUnit = bpu;

                                tempUnit.HP = 0;
                                tempUnit.MP = tempUnit.maxMP;
                                tempUnit.dead = true;

                                tempUnits.Add(tempUnit);
                                break;
                            }
                        }
                    } else // they survived, fully heal them and save them to DB
                    {
                        unit.GetUnitObject().GetComponent<PlayerUnitObject>().playerUnit.HP = unit.GetUnitObject().GetComponent<PlayerUnitObject>().playerUnit.maxMP;
                        unit.GetUnitObject().GetComponent<PlayerUnitObject>().playerUnit.MP = unit.GetUnitObject().GetComponent<PlayerUnitObject>().playerUnit.maxMP;
                        tempUnits.Add(unit.GetUnitObject().GetComponent<PlayerUnitObject>().playerUnit);
                    }                    
                }
            }

            DB.GameDB.SetNewPlayerUnits(tempUnits);

            // Load scene
            SceneManager.LoadScene(sceneToLoad);
        }

        void ToggleMovement(BaseUnit unit, bool enable)
        {
            unit.GetUnitObject().GetComponent<PlayerMovement>().canMove = enable;
        }

        void ProcessPlayerTurn()
        {
            if (!mpm.mapOpen && !mpm.optionsOpen)
            {
                switch (turnState)
                {
                    case turnStates.SELECT:

                        //-- run once
                        if (!runSelectModeStuffOnce)
                        {
                            // run back to original tile
                            runSelectModeStuffOnce = true;
                            selectModeReady = false;

                            onLastTile = false;

                            // turn selection on
                            ts.selectMode = selectModes.PREPARATION;
                            ts.tileSelectMode = tileSelectMode.CHECK;
                            ts.selectionOn = true;

                            mc.ToggleMenu(false);

                            workingPTM.RemoveSelectableTiles();

                            StartCoroutine(workingPTM.MovePlayerUnit(workingPTM.lastTile));
                        }

                        if (workingPTM.transform.position.x == workingPTM.lastTile.transform.position.x &&
                                workingPTM.transform.position.z == workingPTM.lastTile.transform.position.z && !onLastTile && !selectModeReady)
                        {
                            selectModeReady = true;
                            Debug.Log("Select mode is ready");

                            // turn off selectable tiles

                            currentUnit.GetUnitObject().GetComponent<PlayerTacticsMove>().RemoveSelectableTiles();
                            onLastTile = true;
                        }

                        break;
                    case turnStates.MOVE:

                        if (Input.GetKeyDown("e"))
                        {
                            Debug.Log("In here now");
                            SetUnitForMenu();

                            foreach (Tile t in workingPTM.tempSelectableTiles)
                            {
                                t.selectable = false;
                            }
                            bm.menuState = menuStates.MAIN;

                            batCam.cameraMode = CameraModes.PLAYERMENU;
                        }

                        if (runSelectModeStuffOnce)
                        {
                            runSelectModeStuffOnce = false;
                        }

                        if (!mc.canvasDrawn)
                        {
                            TileStandingOn(currentUnit.GetUnitObject()).GetComponent<LandEffect>().SetMultipliers(currentUnit);
                            mc.DrawCanvas((BasePlayerUnit)currentUnit, TileStandingOn(currentUnit.GetUnitObject()));
                        } else
                        {
                            mc.UpdateLandEffect(TileStandingOn(currentUnit.GetUnitObject()));
                        }

                        if (!currentUnit.GetUnitObject().GetComponent<PlayerMovement>().canMove)
                        {
                            currentUnit.GetUnitObject().GetComponent<PlayerMovement>().ToggleCanMove();
                        }

                        if (!selectableTilesFound)
                        {
                            foreach (Tile t in workingPTM.tempSelectableTiles)
                            {
                                t.selectable = true;
                            }

                            selectableTilesFound = true;
                        }                        

                        break;
                    case turnStates.MENU:

                        mpm.canOpenMap = false;

                        if (currentUnit.GetUnitObject().GetComponent<PlayerMovement>().canMove)
                        {
                            currentUnit.GetUnitObject().GetComponent<PlayerMovement>().ToggleCanMove();
                        }

                        currentUnit.GetUnitObject().GetComponent<PlayerTacticsMove>().RemoveSelectableTiles();

                        break;
                }
            }            
        }

        private void SetUnitForMenu()
        {
            turnState = turnStates.MENU;
            bm.ShowMenu(bm.mainMenu, 2, true);

            // turn off movement
            currentUnit.GetUnitObject().GetComponent<PlayerMovement>().ToggleCanMove();

            // move unit to center of tile
            StartCoroutine(currentUnit.ResetAnimator());
            currentUnit.GetUnitObject().transform.position = new Vector3(TileStandingOn(currentUnit.GetUnitObject()).transform.position.x,
                currentUnit.GetUnitObject().transform.position.y, TileStandingOn(currentUnit.GetUnitObject()).transform.position.z);

            // set tile's land effect
            TileStandingOn(currentUnit.GetUnitObject()).GetComponent<LandEffect>().SetMultipliers(currentUnit);
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
            cineCam.enabled = true;

            //Debug.Log(currentUnit.name + " is current unit");

            if (cineCam.Follow != currentUnit.GetUnitObject().transform)
            {
                cineCam.Follow = currentUnit.GetUnitObject().transform;
                cineCam.LookAt = currentUnit.GetUnitObject().transform;
            }
        }

        void IfCameraIsFocused()
        {
            cameraFocused = false;
            if (Vector3.Distance(cineCam.transform.position, currentUnit.GetUnitObject().transform.position) <= cameraFollowDistance)
            {
                cameraFocused = true;
            }
        }

        public void SetBattleState(battleStates newBattleState)
        {
            battleState = newBattleState;
        }

        void SetTurnOrder()
        {
            Debug.Log("------------ SETTING NEW TURN ORDER ------------");
            turnQueue.Clear();

            // bosses always get a free first turn every turn
            foreach (BaseUnit unit in activeUnits)
            {
                Debug.Log(unit.name + " getting turn order set ");
                if (unit.unitType == unitTypes.ENEMY && unit.GetBaseEnemyUnit().boss)
                {
                    turnQueue.Enqueue(unit);
                    Debug.Log("------------ TURN ORDER BOSS QUEUED ------------");
                }
            }
            // ----------------------------------------------

            // build temp list of active units
            List<BaseUnit> tempList = activeUnits;

            List<UnitTurnCalc> turnCalcs = new List<UnitTurnCalc>();

            // agility should be multiplied by a value between 50% and 150%
            // agility sets variability (1/2 of agility). so agility of 6 means its a value between 97% and 103%
            // agility of 50 would mean value between 75% and 125%
            foreach (BaseUnit unit in tempList)
            {
                if (unit.unitType == unitTypes.PLAYER)
                {
                    if (unit.GetUnitObject() == null)
                    {
                        continue; // skips over dead player units
                    }
                }

                // for each unit, perform a "roll" to get the above values and round that to nearest whole value
                float tempRoll = 0;
                float randMin = 100 - (unit.agility / 2); // 100 - agi/2 is min
                float randMax = 100 + (unit.agility / 2); // 100 + agi/2 is max

                tempRoll = Mathf.RoundToInt(GetRandomBetweenRange(randMin, randMax));

                float roll = unit.agility * (tempRoll * .01f);

                UnitTurnCalc utc = new UnitTurnCalc
                {
                    unit = unit,
                    turnValue = roll
                };

                turnCalcs.Add(utc);
            }

            // at the end, sort them into a list by highest value first
            turnCalcs = turnCalcs.OrderBy(x => x.turnValue).ToList();
            turnCalcs.Reverse();

            // for each unit in the list, enqueue them into the turn queue.

            Debug.Log("------------ NEW TURN ORDER ------------");
            for (int i = 0; i < turnCalcs.Count; i++)
            {
                Debug.Log(i + ") " + turnCalcs[i].unit.name + " with a roll of " + turnCalcs[i].turnValue.ToString());
                turnQueue.Enqueue(turnCalcs[i].unit);
            }

            Debug.Log("------------------------------------");
            Debug.Log("--------------*-DONE-*--------------");
            Debug.Log("------------------------------------");

            Debug.Log("Setting first unit to " + turnQueue.Peek().name);
            currentUnit = turnQueue.Peek();
        }

        void SetNextUnit()
        {
            Debug.Log("Setting next unit");
            if (turnQueue.Count > 0)
            {
                turnQueue.Dequeue();

                if (turnQueue.Count == 0)
                {
                    SetTurnOrder();
                } else
                {
                    currentUnit = turnQueue.Peek();
                }                
            } else
            {
                SetTurnOrder();
            }
        }
        
        public void RemoveFromTurnQueue(BaseUnit unit)
        {
            Queue<BaseUnit> tempQueue = turnQueue;

            tempQueue = new Queue<BaseUnit>(tempQueue.Where(x => x.battleID != unit.battleID));

            turnQueue = tempQueue;
        }
    }    
}