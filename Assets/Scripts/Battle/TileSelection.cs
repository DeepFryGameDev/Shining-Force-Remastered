using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum selectModes
{
    PREPARATION,
    SELECTTILE,
    MENU,
    UNITSELECTED,
    PREPAREIDLE,
    IDLE
}

public enum targetModes
{
    IDLE,
    PREPARATION,
    TILESELECTION
}

namespace DeepFry
{
    public class TileSelection : MonoBehaviour
    {
        [Range(0, 5f)]
        public float camYOffset = 2.5f;
        [Range(0, 5f)]
        public float camZOffset = 4.3f;
        [Range(0, 50f)]
        public float camRotXOffset = 30.0f;
        [Range(0, 1f)]
        public float camMovementTime = .45f;
        [Range(0, 5f)]
        public float camRotationSpeed = 3.5f;
        [Range(0, 1f)]
        public float camDistanceThreshold = .1f;
        [Range(0, 1f)]
        public float camTargettingDistanceThreshold = .015f;
        [Range(0, 1f)]
        public float camRotThreshold = .3f;
        [Range(0, 5f)]
        public float camMoveSpeed = 3f;

        public bool selectionOn, selectionOff, inSelection, fromMenu;

        public selectModes selectMode;
        public targetModes targetMode;

        int tempTargetRange;

        public GameObject mainCam, selectionCam, orbitalCam, tileSelectCursor;

        BattleStateMachine bsm;
        MenuPrefabManager mpm;

        TileTargetProcessing ttp;

        public BaseUnit targetUnit;

        bool camMoveReady, rotationComplete, positionComplete;
        Vector3 toPos, toRot, refPos;

        MapMenu mm;
        StatusMenu sm;
        MagicItemMenu mim;
        MagicProcessing mp;
        ItemProcessing ip;
        

        void Start()
        {
            selectMode = selectModes.IDLE;
            targetMode = targetModes.IDLE;

            bsm = GetComponent<BattleStateMachine>();

            mpm = FindObjectOfType<MenuPrefabManager>();

            ttp = FindObjectOfType<TileTargetProcessing>();

            mp = FindObjectOfType<MagicProcessing>();

            ip = FindObjectOfType<ItemProcessing>();

            camMoveReady = false;

            toRot = new Vector3(camRotXOffset, 0.0f, 0.0f);

            mm = FindObjectOfType<MapMenu>();
            sm = FindObjectOfType<StatusMenu>();
            mim = FindObjectOfType<MagicItemMenu>();

            fromMenu = false;
        }


        void Update()
        {
            if (selectMode != selectModes.IDLE)
            {
                switch (selectMode)
                {
                    case selectModes.PREPARATION:
                        Vector3 pos = new Vector3(bsm.currentUnit.GetUnitObject().GetComponent<TacticsMove>().lastTile.transform.position.x,
                    camYOffset,
                    bsm.currentUnit.GetUnitObject().GetComponent<TacticsMove>().lastTile.transform.position.z - camZOffset);
                        PrepareAndRunCamera(pos, mainCam.transform);

                        inSelection = true;

                        break;
                    case selectModes.SELECTTILE:

                        HandleCameraMovementInput();

                        if (Input.GetKeyDown("e"))
                        {
                            Debug.Log("Checking tile " + GetTileHit().gameObject.name);

                            if (ttp.GetUnitOnTile(GetTileHit()) != null)
                            {
                                if (ttp.GetUnitOnTile(GetTileHit()).unitType == unitTypes.PLAYER)
                                {
                                    sm.DisplayStatusWindow(ttp.GetUnitOnTile(GetTileHit()) as BasePlayerUnit);
                                    sm.statusWindowOpened = true;
                                    selectMode = selectModes.UNITSELECTED;
                                }
                                else if (ttp.GetUnitOnTile(GetTileHit()).unitType == unitTypes.ENEMY)
                                {
                                    sm.DisplayStatusWindow(ttp.GetUnitOnTile(GetTileHit()) as BaseEnemyUnit);
                                    sm.statusWindowOpened = true;
                                    selectMode = selectModes.UNITSELECTED;
                                }
                            }
                        }

                        if (Input.GetKeyDown("c"))
                        {
                            Debug.Log("Set inSelection to false");
                            selectionOff = true;
                            inSelection = false;
                            selectMode = selectModes.PREPAREIDLE;
                        }

                        SetTileSelectCursorPos(GetTileHit());

                        break;
                    case selectModes.MENU:

                        break;
                    case selectModes.UNITSELECTED:
                        if (Input.GetKeyDown("c"))
                        {
                            mpm.OpenCanvas(sm.gameObject, false);
                            sm.statusWindowOpened = false;
                            selectMode = selectModes.SELECTTILE;
                        }
                        break;
                    case selectModes.PREPAREIDLE:
                        foreach (Tile tile in FindObjectsOfType<Tile>())
                        {
                            tile.selectable = false;
                        }

                        if (selectionOff)
                        {
                            selectionOff = false;
                            positionComplete = false;
                            rotationComplete = false;

                            camMoveReady = true;

                            tileSelectCursor.SetActive(false);
                        }

                        if (camMoveReady)
                        {
                            MoveCamera();
                        }

                        if (positionComplete && rotationComplete)
                        {
                            if (fromMenu)
                            {
                                orbitalCam.SetActive(true);
                                mainCam.SetActive(true);
                                selectionCam.SetActive(false);
                                selectMode = selectModes.IDLE;
                                targetMode = targetModes.IDLE;

                                bsm.selectableTilesFound = false;
                            } else
                            {
                                Debug.Log("Not from menu");
                                mainCam.SetActive(true);
                                selectionCam.SetActive(false);
                                selectMode = selectModes.IDLE;
                                targetMode = targetModes.IDLE;

                                bsm.workingPTM.GetComponent<PlayerMovement>().controller.enabled = true;
                                bsm.turnState = turnStates.MOVE;

                                bsm.selectableTilesFound = false;
                            }

                            fromMenu = false;
                        }

                        break;
                }
            }          
            
            if (targetMode != targetModes.IDLE)
            {
                switch (targetMode)
                {
                    case targetModes.PREPARATION:

                        if (mim.menuOpen)
                        {
                            mim.HideMenu();
                        }                        

                        if (ttp.currentBTT != null)
                        {
                            orbitalCam.SetActive(false);

                            // Vector3 pos = // needs to be first available target's tile's position.
                            Vector3 tempPos;

                            if (ttp.currentMagic != null)
                            {
                                targetUnit = ttp.GetFirstAvailableTarget(ttp.currentMagic.targetType);
                                tempTargetRange = ttp.currentMagic.targetRange;
                            } else
                            {
                                targetUnit = ttp.GetFirstAvailableTarget(ttp.currentItem.targetType);
                                tempTargetRange = ttp.currentItem.targetRange;
                            }

                            tempPos = targetUnit.GetUnitObject().transform.position;

                            Vector3 pos = new Vector3(tempPos.x, camYOffset, tempPos.z - camZOffset);

                            PrepareAndRunCamera(pos, orbitalCam.transform);

                            inSelection = true;

                            if (positionComplete && rotationComplete)
                            {
                                // show tile range
                                
                                List<Tile> effectTiles = ttp.GetTileRange(tempTargetRange, ttp.GetTileAtPos(tempPos));
                                foreach (Tile tile in effectTiles)
                                {
                                    tile.selectable = true;
                                }

                                SetTileSelectCursorPos(ttp.GetTileAtPos(tempPos));
                                tileSelectCursor.SetActive(true);

                                targetMode = targetModes.TILESELECTION;
                            }
                        }

                        break;
                    case targetModes.TILESELECTION:
                        HandleTileSelectionInput();

                        KeepCameraOnCursor();
                        break;
                }
            }
        }

        void KeepCameraOnCursor()
        {
            //selectionCam.transform.position = Vector3.Lerp()
            Vector3 toPos = new Vector3(GetTileSelectCursorTile().transform.position.x, GetTileSelectCursorTile().transform.position.y + camYOffset, 
                GetTileSelectCursorTile().transform.position.z - camZOffset);

            selectionCam.transform.position = Vector3.Lerp(selectionCam.transform.position, toPos, Time.deltaTime * camMoveSpeed);

            if (Vector3.Distance(toPos, selectionCam.transform.position) <= camTargettingDistanceThreshold)
            {
                selectionCam.transform.position = toPos;
            }
        }

        void HandleTileSelectionInput()
        {
            if (Input.GetKeyDown("w"))
            {
                Tile tempTile = ttp.GetTileAtPos(new Vector3(GetTileSelectCursorTile().transform.position.x
    , GetTileSelectCursorTile().transform.position.y, GetTileSelectCursorTile().transform.position.z + 1));

                if (tempTile != null && tempTile.selectable && ttp.GetUnitOnTile(tempTile) != null)
                {
                    SetTileSelectCursorPos(tempTile);
                }                
            }

            if (Input.GetKeyDown("s"))
            {
                Tile tempTile = ttp.GetTileAtPos(new Vector3(GetTileSelectCursorTile().transform.position.x
, GetTileSelectCursorTile().transform.position.y, GetTileSelectCursorTile().transform.position.z - 1));

                if (tempTile != null && tempTile.selectable && ttp.GetUnitOnTile(tempTile) != null)
                {
                    SetTileSelectCursorPos(tempTile);
                }
            }

            if (Input.GetKeyDown("a"))
            {
                Tile tempTile = ttp.GetTileAtPos(new Vector3(GetTileSelectCursorTile().transform.position.x - 1
    , GetTileSelectCursorTile().transform.position.y, GetTileSelectCursorTile().transform.position.z));

                if (tempTile != null && tempTile.selectable && ttp.GetUnitOnTile(tempTile) != null)
                {
                    SetTileSelectCursorPos(tempTile);
                }
            }

            if (Input.GetKeyDown("d"))
            {
                Tile tempTile = ttp.GetTileAtPos(new Vector3(GetTileSelectCursorTile().transform.position.x + 1
, GetTileSelectCursorTile().transform.position.y, GetTileSelectCursorTile().transform.position.z));

                if (tempTile != null && tempTile.selectable && ttp.GetUnitOnTile(tempTile) != null)
                {
                    SetTileSelectCursorPos(tempTile);
                }
            }

            if (Input.GetKeyDown("e"))
            {
                targetUnit = ttp.GetUnitOnTile(GetTileSelectCursorTile());

                if (ttp.currentMagic != null)
                {
                    Debug.Log("---");
                    Debug.Log("Simulating magic - MP before: " + bsm.currentUnit.MP);
                    mp.ExecuteMagic();
                    Debug.Log("Simulating magic - MP after: " + bsm.currentUnit.MP);
                    Debug.Log("---");
                }

                if (ttp.currentItem != null)
                {
                    Debug.Log("---");
                    ip.ExecuteItem();
                    Debug.Log("---");
                }
            }

            if (Input.GetKeyDown("c"))
            {
                selectionOff = true;
                inSelection = false;
                selectMode = selectModes.PREPAREIDLE;
            }
        }

        Tile GetTileSelectCursorTile()
        {
            return ttp.GetTileAtPos(tileSelectCursor.transform.position);
        }

        private void PrepareAndRunCamera(Vector3 newPos, Transform homeTransform)
        {
            // start here
            if (selectionOn && !camMoveReady)
            {
                selectionOn = false;
                positionComplete = false;
                rotationComplete = false;

                selectionCam.transform.position = homeTransform.position;
                selectionCam.transform.rotation = homeTransform.rotation;

                selectionCam.SetActive(true);
                mainCam.SetActive(false);

                toPos = newPos;

                camMoveReady = true;
            }

            if (positionComplete && rotationComplete) // add check if camera is in place too
            {
                SetTileSelectCursorPos(GetTileHit());

                tileSelectCursor.SetActive(true);

                FindObjectOfType<PlayerMovement>().controller.enabled = true; // weird workaround for now.

                // enable camera movement with WASD

                selectMode = selectModes.SELECTTILE;
            }

            if (camMoveReady)
            {
                MoveCamera();
            }
        }

        private void HandleCameraMovementInput()
        {
            if (Input.GetKey("w"))
            {
                selectionCam.transform.Translate(Vector3.forward * (Input.GetAxis("Vertical") * camMoveSpeed * Time.deltaTime), Space.World);
            }

            if (Input.GetKey("d"))
            {
                selectionCam.transform.Translate(Vector3.right * (Input.GetAxis("Horizontal") * camMoveSpeed * Time.deltaTime), Space.World);
            }

            if (Input.GetKey("s"))
            {
                selectionCam.transform.Translate(Vector3.back * (-Input.GetAxis("Vertical") * camMoveSpeed * Time.deltaTime), Space.World);
            }

            if (Input.GetKey("a"))
            {
                selectionCam.transform.Translate(Vector3.left * (-Input.GetAxis("Horizontal") * camMoveSpeed * Time.deltaTime), Space.World);
            }
        }

        private void SetTileSelectCursorPos(Tile tile)
        {
            Vector3 newPos = new Vector3(tile.transform.position.x, tileSelectCursor.transform.position.y, tile.transform.position.z);
            tileSelectCursor.transform.position = newPos;
        }

        void MoveCamera()
        {
            if (!positionComplete)
            {
                selectionCam.transform.position = Vector3.SmoothDamp(selectionCam.transform.position, toPos, ref refPos, camMovementTime);

                if (Vector3.Distance(selectionCam.transform.position, toPos) <= camDistanceThreshold || fromMenu)
                {
                    positionComplete = true;

                    selectionCam.transform.position = toPos;
                }
            }

            if (!rotationComplete)
            {
                selectionCam.transform.rotation = Quaternion.Slerp(selectionCam.transform.rotation, Quaternion.Euler(toRot), camRotationSpeed * Time.deltaTime);

                if (toRot.x - selectionCam.transform.eulerAngles.x <= camRotThreshold)
                {
                    Debug.Log("setting rotation to complete");
                    rotationComplete = true;

                    selectionCam.transform.rotation = Quaternion.Euler(toRot);
                }
            }

            if (positionComplete && rotationComplete)
            {
                camMoveReady = false;
            }
        }

        Tile GetTileHit()
        {
            RaycastHit[] hits;
            hits = Physics.RaycastAll(selectionCam.transform.position, selectionCam.transform.TransformDirection(Vector3.forward), 100.0f);

            for (int i=0; i < hits.Length; i++)
            {
                if (hits[i].collider.transform.tag == "Tile")
                {
                    //Debug.Log("Hit " + hits[i].transform.gameObject.name);

                    Debug.DrawRay(selectionCam.transform.position, selectionCam.transform.TransformDirection(Vector3.forward) * hits[i].distance * 2, Color.yellow);
                    return hits[i].collider.transform.GetComponent<Tile>();
                }
            }

            Debug.LogError("No tile found");

            return null;
        }
    }
}
