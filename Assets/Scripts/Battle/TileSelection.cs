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
        [Range(10, 40)]
        public float targetZoomFOV = 25;
        [Range(.05f, 1)]
        public float zoomStopThreshold = .1f;
        [Range(.1f, 2)]
        public float zoomSmoothTime = .3f;
        [Range(10, 35)]
        public float zoomInFOV = 25;
        [Range(35, 50)]
        public float zoomOutFOV = 40;

        float zoomVelocity = 0;

        public bool selectionOn, selectionOff, inSelection, fromMenu;

        public selectModes selectMode;
        public targetModes targetMode;

        int tempTargetRange;

        public GameObject mainCam, selectionCam, orbitalCam, tileSelectCursor;

        List<Tile> selectableTiles = new List<Tile>();

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
        AttackProcessing ap;
        

        void Start()
        {
            selectMode = selectModes.IDLE;
            targetMode = targetModes.IDLE;

            bsm = GetComponent<BattleStateMachine>();

            mpm = FindObjectOfType<MenuPrefabManager>();

            ttp = FindObjectOfType<TileTargetProcessing>();

            mp = FindObjectOfType<MagicProcessing>();

            ip = FindObjectOfType<ItemProcessing>();

            ap = FindObjectOfType<AttackProcessing>();

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
                            MoveCamera(selectionCam, toPos);
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

                            if (bsm.currentUnit.unitType == unitTypes.PLAYER)
                            {
                                if (ttp.currentMagic != null)
                                {
                                    targetUnit = ttp.GetFirstAvailableTarget(ttp.currentMagic.targetType);
                                    tempTargetRange = ttp.currentMagic.targetRange;
                                }
                                else if (ttp.currentItem != null)
                                {
                                    targetUnit = ttp.GetFirstAvailableTarget(ttp.currentItem.targetType);
                                    tempTargetRange = ttp.currentItem.targetRange;
                                }
                                else
                                {
                                    targetUnit = ttp.GetFirstAvailableTarget(TargetTypes.ENEMY);
                                    BasePlayerUnit tempBPU = (BasePlayerUnit)bsm.currentUnit;
                                    tempTargetRange = tempBPU.GetEquippedWeapon().attackRange;
                                }
                            }

                            if (bsm.currentUnit.unitType == unitTypes.ENEMY)
                            {
                                if (bsm.currentUnit.unitType == unitTypes.PLAYER)
                                {
                                    if (ttp.currentMagic != null)
                                    {
                                        // magic not developed yet
                                    }
                                    else if (ttp.currentItem != null)
                                    {
                                        // items not developed yet
                                    }
                                    else
                                    {
                                        BaseEnemyUnit tempBEU = (BaseEnemyUnit)bsm.currentUnit;
                                        tempTargetRange = tempBEU.attackRange;
                                    }
                                }
                            }

                            tempPos = targetUnit.GetUnitObject().transform.position;

                            Vector3 pos = new Vector3(tempPos.x, camYOffset, tempPos.z - camZOffset);

                            PrepareAndRunCamera(pos, orbitalCam.transform);

                            inSelection = true;

                            if (positionComplete && rotationComplete)
                            {
                                // show tile range

                                selectableTiles.Clear();
                                List<Tile> effectTiles = new List<Tile>();

                                if (ttp.currentItem == null && ttp.currentMagic == null) // attack
                                {
                                    effectTiles = ttp.GetTileRange(tempTargetRange, ttp.GetTileAtPos(bsm.currentUnit.GetUnitObject().transform.position));
                                } else // magic or item
                                {
                                    effectTiles = ttp.GetTileRange(tempTargetRange, ttp.GetTileAtPos(tempPos));
                                }
                                
                                foreach (Tile tile in effectTiles)
                                {
                                    selectableTiles.Add(tile);
                                }

                                SetSelectableTiles(true);

                                SetTileSelectCursorPos(ttp.GetTileAtPos(tempPos));
                                tileSelectCursor.SetActive(true);

                                Debug.Log("Continue here");

                                targetMode = targetModes.TILESELECTION;
                            }
                        }

                        break;
                    case targetModes.TILESELECTION:
                        if (bsm.currentUnit.unitType == unitTypes.PLAYER)
                        {
                            HandleTileSelectionInput();
                        }                        

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
                    mp.ExecuteMagic();
                    targetMode = targetModes.IDLE;
                    SetSelectableTiles(false);
                    inSelection = false;
                    Debug.Log("---");
                }

                if (ttp.currentItem != null)
                {
                    Debug.Log("---");
                    ip.ExecuteItem();
                    targetMode = targetModes.IDLE;
                    SetSelectableTiles(false);
                    inSelection = false;
                    Debug.Log("---");
                }

                if (ttp.currentItem == null && ttp.currentMagic == null)
                {
                    Debug.Log("---");
                    ap.ExecuteAttack();
                    targetMode = targetModes.IDLE;
                    SetSelectableTiles(false);
                    inSelection = false;
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

        public void SetSelectableTiles(bool selectable)
        {
            foreach (Tile tile in selectableTiles)
            {
                tile.selectable = selectable;
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
                MoveCamera(selectionCam, toPos);
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

        public void MoveCamera(GameObject camera, Vector3 toPosition)
        {
            if (!positionComplete)
            {
                camera.transform.position = Vector3.SmoothDamp(camera.transform.position, toPosition, ref refPos, camMovementTime);

                if (Vector3.Distance(camera.transform.position, toPosition) <= camDistanceThreshold || fromMenu)
                {
                    Debug.Log("Camera is moved");
                    positionComplete = true;

                    camera.transform.position = toPosition;
                }
            }

            if (!rotationComplete)
            {
                camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, Quaternion.Euler(toRot), camRotationSpeed * Time.deltaTime);

                if (toRot.x - camera.transform.eulerAngles.x <= camRotThreshold)
                {
                    rotationComplete = true;

                    camera.transform.rotation = Quaternion.Euler(toRot);
                }
            }

            if (positionComplete && rotationComplete)
            {
                camMoveReady = false;
            }
        }

        public IEnumerator MoveCamera(GameObject cam, BaseUnit unit)
        {
            Tile t = ttp.GetTileAtPos(unit.GetUnitObject().transform.position);

            Debug.Log("Moving camera to " + unit.name);

            bool posComplete = false, rotComplete = false, moveComplete = false;
            Vector3 toPos = new Vector3(), toRot = new Vector3(), refPos = new Vector3();

            toPos = new Vector3(t.transform.position.x, camYOffset, t.transform.position.z - camZOffset);
            toRot = new Vector3(camRotXOffset, 0.0f, 0.0f);

            while (!moveComplete)
            {
                cam.transform.position = Vector3.SmoothDamp(cam.transform.position, toPos, ref refPos, camMovementTime);

                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.Euler(toRot), camRotationSpeed * Time.deltaTime);

                if (Vector3.Distance(cam.transform.position, toPos) <= camDistanceThreshold)
                {
                    posComplete = true;

                    cam.transform.position = toPos;
                }

                if (toRot.x - cam.transform.eulerAngles.x <= camRotThreshold)
                {
                    rotComplete = true;

                    cam.transform.rotation = Quaternion.Euler(toRot);
                }

                if (posComplete && rotComplete)
                {
                    moveComplete = true;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator ZoomToUnit(GameObject camera, bool zoomingIn)
        {
            Camera cam = camera.GetComponent<Camera>();

            bool zoomComplete = false;
            float newFOV, targetFOV;

            if (zoomingIn)
                targetFOV = zoomInFOV;
            else
                targetFOV = zoomOutFOV;

            while (!zoomComplete)
            {
                cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref zoomVelocity, zoomSmoothTime);

                newFOV = cam.fieldOfView;

                if (Mathf.Abs(newFOV - targetFOV) <= zoomStopThreshold)
                {
                    zoomComplete = true;
                }

                yield return new WaitForEndOfFrame();
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

        public void ToggleTileSelectCursor(bool enabled)
        {
            tileSelectCursor.SetActive(enabled);
        }
    }
}
