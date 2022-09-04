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

        public BaseUnit currentUnit;
        GameObject currentUnitObject;

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

        MoveCanvas moveCanvas;

        MenuPrefabManager mpm;

        // Start is called before the first frame update
        void Start()
        {
            cineCam = FindObjectOfType<Cinemachine.CinemachineFreeLook>();

            InstantiateCombatants();
            SortUnitOrder();

            TileMenuScripts.SetTileCoordinates();

            bm = FindObjectOfType<BattleMenu>();
            ts = GetComponent<TileSelection>();

            mpm = FindObjectOfType<MenuPrefabManager>();
            sm = FindObjectOfType<StatusMenu>();

            batCam = FindObjectOfType<BattleCamera>();
            moveCanvas = FindObjectOfType<MoveCanvas>();

            cameraFocused = false;
            runSelectModeStuffOnce = false;
            selectModeReady = false;
            onLastTile = false;
            selectableTilesFound = false;
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
                        unit.GetUnitObject().GetComponent<EnemyTacticsMove>().enemyUnit = (BaseEnemyUnit)unit;
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
                        unit.GetUnitObject().GetComponent<TacticsMove>().unit = unit;
                        break;
                    }
                }

                newObject.GetComponent<PlayerTacticsMove>().Init();
            }
        }

        void SortUnitOrder()
        {
            activeUnits = activeUnits.OrderBy(x => x.agility).ToList(); // better formula will be put in place later

            activeUnits.Reverse(); // Sets highest speed first

            foreach (BaseUnit bu in activeUnits)
            {
                Debug.Log(bu.name + " agility: " + bu.agility);
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

                    // Focus camera on currentUnit
                    SetCameraFocus();

                    IfCameraIsFocused();

                    if (cameraFocused)
                    {
                        Debug.Log("Turn start: " + currentUnit.name);

                        // if player
                        if (currentUnit is BasePlayerUnit)
                        {
                            workingPTM = currentUnit.GetUnitObject().GetComponent<PlayerTacticsMove>();

                            workingPTM.GetCurrentTile();
                            workingPTM.lastTile = workingPTM.currentTile;
                            lastTile = workingPTM.lastTile;

                            if (!selectableTilesFound)
                            {
                                workingPTM.FindSelectableTiles();
                                workingPTM.SetLastTile();

                                selectableTilesFound = true;
                            }                            

                            turnState = turnStates.MOVE;

                            FindObjectOfType<PlayerMovement>().canMove = true;
                        }
                        // if enemy
                        if (currentUnit is BaseEnemyUnit)
                        {
                            workingETM = currentUnit.GetUnitObject().GetComponent<EnemyTacticsMove>();
                            // just simulate enemy taking a turn, wait a few seconds then proceed to player turn

                            workingETM.GetCurrentTile();

                            workingETM.lastTile = workingETM.currentTile;
                            lastTile = workingETM.lastTile;

                            workingETM.SetLastTile();
                            workingETM.Init();
                            workingETM.SetActualTargetTile();
                        }

                        battleState = battleStates.DURINGTURN;
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
                    // if player
                    if (currentUnit is BasePlayerUnit)
                    {
                        //currentUnit.GetUnitObject().GetComponent<Invector.vCharacterController.vThirdPersonInput>().enabled = false;

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

                    break;
                case battleStates.IDLE:

                    break;
            }
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
                            ts.selectMode = selectModes.START;
                            ts.selectionOn = true;

                            StartCoroutine(workingPTM.MovePlayerUnit(workingPTM.lastTile));
                        }

                        if (workingPTM.transform.position.x == workingPTM.lastTile.transform.position.x &&
                                workingPTM.transform.position.z == workingPTM.lastTile.transform.position.z && !onLastTile && !selectModeReady)
                        {
                            selectModeReady = true;

                            // turn off selectable tiles

                            currentUnit.GetUnitObject().GetComponent<PlayerTacticsMove>().RemoveSelectableTiles();
                            onLastTile = true;
                        }

                        break;
                    case turnStates.MOVE:

                        if (Input.GetKeyDown("e"))
                        {
                            SetUnitForMenu();

                            foreach (Tile t in workingPTM.tempSelectableTiles)
                            {
                                t.selectable = false;
                            }

                            batCam.cameraMode = CameraModes.PLAYERMENU;
                        }

                        if (Input.GetKeyDown("c"))
                        {
                            turnState = turnStates.SELECT;
                        }

                        if (runSelectModeStuffOnce)
                        {
                            runSelectModeStuffOnce = false;
                        }

                        if (!moveCanvas.canvasDrawn)
                        {
                            TileStandingOn(currentUnitObject).GetComponent<LandEffect>().SetMultipliers(currentUnit);
                            moveCanvas.DrawCanvas((BasePlayerUnit)currentUnit, TileStandingOn(currentUnitObject));
                        } else
                        {
                            moveCanvas.UpdateLandEffect(TileStandingOn(currentUnitObject));
                        }

                        if (!currentUnit.GetUnitObject().GetComponent<PlayerMovement>().canMove)
                        {
                            currentUnit.GetUnitObject().GetComponent<PlayerMovement>().ToggleCanMove();
                        }

                        if (!selectableTilesFound)
                        {
                            Debug.Log("Getting selectable tiles back");

                            foreach (Tile t in workingPTM.tempSelectableTiles)
                            {
                                t.selectable = true;
                            }

                            selectableTilesFound = true;
                        }                        

                        break;
                    case turnStates.MENU:

                        mpm.canOpenMap = false;

                        if (Input.GetKeyDown("w"))
                        {
                            bm.SetHovered(bm.topButton);
                        }

                        if (Input.GetKeyDown("d"))
                        {
                            bm.SetHovered(bm.rightButton);
                        }

                        if (Input.GetKeyDown("s"))
                        {
                            bm.SetHovered(bm.bottomButton);
                        }

                        if (Input.GetKeyDown("a"))
                        {
                            bm.SetHovered(bm.leftButton);
                        }

                        if (Input.GetKeyDown("c"))
                        {
                            turnState = turnStates.MOVE;
                            batCam.cameraMode = CameraModes.PLAYERMOVE;

                            selectableTilesFound = false;

                            mpm.canOpenMap = true;
                        }

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
            Debug.Log("Opening menu: " + currentUnit.name);
            turnState = turnStates.MENU;

            // turn off movement
            FindObjectOfType<PlayerMovement>().ToggleCanMove();

            // move unit to center of tile
            StartCoroutine(currentUnit.ResetAnimator());
            Debug.Log("move " + currentUnitObject.gameObject.name + " to: " + TileStandingOn(currentUnitObject).transform.position);
            currentUnitObject.transform.position = new Vector3(TileStandingOn(currentUnitObject).transform.position.x,
                currentUnitObject.transform.position.y, TileStandingOn(currentUnitObject).transform.position.z);

            // set tile's land effect
            TileStandingOn(currentUnitObject).GetComponent<LandEffect>().SetMultipliers(currentUnit);
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