using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

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

public enum tileSelectMode
{
    ACTION,
    CHECK,
    IDLE
}

namespace DeepFry
{
    public class TileSelection : MonoBehaviour
    {
        [Range(0, 5f)]
        public float camYOffset = 2.5f;
        [Range(0, 180f)]
        public float camRotXOffset = 90.0f;
        [Range(0, 180f)]
        public float camRotXCombatOffset = 65.0f;
        [Range(0.1f, 3f)]
        public float camZCombatOffset = 1f;
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
        public float targetZoomFOV = 60;
        [Range(.05f, 1)]
        public float zoomStopThreshold = .1f;
        [Range(.1f, 2)]
        public float zoomSmoothTime = .3f;
        [Range(10, 60)]
        public float zoomInFOV = 45;
        [Range(35, 70)]
        public float zoomOutFOV = 60;

        float zoomVelocity = 0;

        public bool selectionOn, selectionOff, inSelection, fromMenu;

        public selectModes selectMode;
        public targetModes targetMode;

        int tempTargetRange, tempEffectRange;

        public GameObject mainCam, selectionCam, orbitalCam, gameCam, tempCam, tileSelectCursor;
        CinemachineBrain brain;

        List<Tile> selectableTiles = new List<Tile>();

        BattleStateMachine bsm;
        public MenuPrefabManager mpm;

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
        BattleMenu bm;

        public tileSelectMode tileSelectMode;

        bool cameraMoving;
        

        void Start()
        {
            selectMode = selectModes.IDLE;
            targetMode = targetModes.IDLE;
            tileSelectMode = tileSelectMode.IDLE;

            bsm = GetComponent<BattleStateMachine>();

            mpm = FindObjectOfType<MenuPrefabManager>();

            ttp = FindObjectOfType<TileTargetProcessing>();

            mp = FindObjectOfType<MagicProcessing>();

            ip = FindObjectOfType<ItemProcessing>();

            ap = FindObjectOfType<AttackProcessing>();

            bm = FindObjectOfType<BattleMenu>();

            brain = FindObjectOfType<CinemachineBrain>();

            camMoveReady = false;

            toRot = new Vector3(camRotXOffset, 0.0f, 0.0f);

            mm = FindObjectOfType<MapMenu>();
            sm = FindObjectOfType<StatusMenu>();
            mim = FindObjectOfType<MagicItemMenu>();

            fromMenu = false;
        }


        void Update()
        {
            switch (tileSelectMode)
            {
                case tileSelectMode.CHECK:
                    switch (selectMode)
                    {
                        case selectModes.PREPARATION:
                            Vector3 pos = new Vector3(bsm.currentUnit.GetUnitObject().GetComponent<TacticsMove>().lastTile.transform.position.x,
                        camYOffset,
                        bsm.currentUnit.GetUnitObject().GetComponent<TacticsMove>().lastTile.transform.position.z); // this is directly above the current unit

                            // turn off cinemachine brain
                            brain.enabled = false;

                            selectionCam.transform.position = mainCam.transform.position;
                            selectionCam.transform.rotation = mainCam.transform.rotation;

                            selectionCam.SetActive(true);
                            gameCam.SetActive(false);
                            mainCam.SetActive(false);

                            StartCoroutine(MoveCameraForSelection(bsm.currentUnit.GetUnitObject().GetComponent<TacticsMove>().lastTile, tileSelectMode.CHECK));
                            //MoveCameraForSelection(pos, selectionCam.transform);

                            //inSelection = true;

                            //tempCam.transform.position = selectionCam.transform.position;
                            //tempCam.transform.rotation = selectionCam.transform.rotation;

                            break;
                        case selectModes.SELECTTILE:
                            /*
                            if (Input.GetKeyDown("c"))
                            {
                                Debug.Log("Set inSelection to false");
                                selectionOff = true;
                                inSelection = false;
                                selectMode = selectModes.PREPAREIDLE;
                            }*/

                            break;
                        case selectModes.MENU:

                            break;
                        case selectModes.UNITSELECTED:
                            /*if (Input.GetKeyDown("c"))
                            {
                                mpm.OpenCanvas(sm.gameObject, false);
                                sm.statusWindowOpened = false;
                                selectMode = selectModes.SELECTTILE;
                            }*/
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

                            // turn on cinemachine brain
                            brain.enabled = true;

                            if (fromMenu)
                            {
                                orbitalCam.SetActive(true);
                                mainCam.SetActive(true);
                                selectionCam.SetActive(false);
                                selectMode = selectModes.IDLE;
                                targetMode = targetModes.IDLE;
                                tileSelectMode = tileSelectMode.IDLE;

                                bsm.selectableTilesFound = false;
                            }
                            else
                            {
                                Debug.Log("Not from menu");
                                mainCam.SetActive(true);
                                gameCam.SetActive(true);
                                selectionCam.SetActive(false);
                                selectMode = selectModes.IDLE;
                                targetMode = targetModes.IDLE;
                                tileSelectMode = tileSelectMode.IDLE;

                                bsm.workingPTM.GetComponent<PlayerMovement>().controller.enabled = true;
                                bsm.turnState = turnStates.MOVE;

                                bsm.selectableTilesFound = false;
                            }

                            fromMenu = false;

                            break;
                    }
                    break;
                case tileSelectMode.ACTION:
                    switch (targetMode)
                    {
                        case targetModes.PREPARATION:
                            if (mim.menuOpen)
                            {
                                mim.HideMenu();
                            }

                            if (ttp.currentBTT != null)
                            {                               
                                // Vector3 pos = // needs to be first available target's tile's position.
                                Vector3 tempPos;

                                if (bsm.currentUnit.unitType == unitTypes.PLAYER)
                                {
                                    orbitalCam.SetActive(false);

                                    if (ttp.currentMagic != null)
                                    {
                                        targetUnit = ttp.GetFirstAvailableTarget(ttp.currentMagic.targetType);
                                        tempTargetRange = ttp.currentMagic.targetRange;
                                    }
                                    else if (ttp.currentItem != null)
                                    {
                                        //Debug.Log("Menu state: " + bm.itemMenuMode); // something else

                                        switch (bm.itemMenuMode)
                                        {
                                            case itemMenuModes.USE:
                                                targetUnit = ttp.GetFirstAvailableTarget(ttp.currentUsableItem.targetType);
                                                tempTargetRange = ttp.currentUsableItem.targetRange;
                                                tempEffectRange = ttp.currentUsableItem.effectRange;
                                                break;

                                            case itemMenuModes.GIVE:
                                                targetUnit = ttp.GetFirstAvailableTarget(bm.itemMenuMode, TargetTypes.PLAYER);
                                                tempTargetRange = 1;
                                                tempEffectRange = 0;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        targetUnit = ttp.GetFirstAvailableTarget(TargetTypes.ENEMY);
                                        BasePlayerUnit tempBPU = (BasePlayerUnit)bsm.currentUnit;
                                        tempTargetRange = tempBPU.GetEquippedWeapon().attackRange;
                                    }
                                }

                                if (bsm.currentUnit.unitType == unitTypes.ENEMY) // <-- idk what this is lol
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

                                Vector3 pos = new Vector3(targetUnit.GetUnitObject().transform.position.x, camYOffset, targetUnit.GetUnitObject().transform.position.z);

                                brain.enabled = false;

                                selectionCam.transform.position = mainCam.transform.position;
                                selectionCam.transform.rotation = mainCam.transform.rotation;

                                selectionCam.SetActive(true);
                                mainCam.SetActive(false);
                                gameCam.SetActive(false);    

                                StartCoroutine(MoveCameraForSelection(targetUnit.GetTile(), tileSelectMode.ACTION));

                                /*inSelection = true;

                                if (positionComplete && rotationComplete)
                                {
                                    // show tile range

                                    selectableTiles.Clear();
                                    List<Tile> targetTiles = new List<Tile>();

                                    if (ttp.currentItem == null && ttp.currentMagic == null) // attack
                                    {
                                        targetTiles = ttp.GetTargetTiles(bsm.currentUnit.GetTile(), tempTargetRange);
                                    }
                                    else // magic or item
                                    {
                                        targetTiles = ttp.GetTargetTiles(bsm.currentUnit.GetTile(), tempTargetRange);
                                    }

                                    foreach (Tile tile in targetTiles)
                                    {
                                        selectableTiles.Add(tile);
                                    }

                                    SetSelectableTiles(true);

                                    SetTileSelectCursorPos(ttp.GetTileAtPos(tempPos));
                                    tileSelectCursor.SetActive(true);

                                    Debug.Log("Continue here");

                                    targetMode = targetModes.TILESELECTION;
                                }*/
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
                    break;
            }    
        }

        IEnumerator MoveCameraForSelection(Tile tile, tileSelectMode mode)
        {
            if (!cameraMoving)
            {
                cameraMoving = true;

                selectionCam.transform.position = mainCam.transform.position;
                selectionCam.transform.rotation = mainCam.transform.rotation;

                selectionCam.SetActive(true);
                gameCam.SetActive(false);
                mainCam.SetActive(false);

                tileSelectMode = tileSelectMode.IDLE;

                yield return StartCoroutine(MoveCamera(selectionCam, tile));

                Debug.Log("Movement complete");
                //selectionCam.transform.position = toPos;
                //selectionCam.transform.rotation = Quaternion.Euler(toRot);

                //selectionCam.SetActive(true);
                //gameCam.SetActive(false);
                //mainCam.SetActive(false);

                tileSelectMode = mode;

                //selectionCam.SetActive(true);
                //gameCam.SetActive(false);

                SetTileSelectCursorPos(GetTileHit());

                tileSelectCursor.SetActive(true);

                //FindObjectOfType<PlayerMovement>().controller.enabled = true; // weird workaround for now.

                // enable camera movement with WASD

                if (mode == tileSelectMode.CHECK)
                {
                    selectMode = selectModes.SELECTTILE;
                } else if (mode == tileSelectMode.ACTION)
                {
                    inSelection = true;
                    // show tile range

                    selectableTiles.Clear();
                    List<Tile> targetTiles = new List<Tile>();

                    if (ttp.currentItem == null && ttp.currentMagic == null) // attack
                    {
                        targetTiles = ttp.GetTargetTiles(bsm.currentUnit.GetTile(), tempTargetRange);
                    }
                    else // magic or item
                    {
                        targetTiles = ttp.GetTargetTiles(bsm.currentUnit.GetTile(), tempTargetRange);
                    }

                    foreach (Tile t in targetTiles)
                    {
                        selectableTiles.Add(t);
                    }

                    SetSelectableTiles(true);

                    SetTileSelectCursorPos(ttp.GetTileAtPos(targetUnit.GetUnitObject().transform.position));
                    tileSelectCursor.SetActive(true);

                    Debug.Log("Continue here");

                    targetMode = targetModes.TILESELECTION;
                }
                cameraMoving = false;
            }  
        }

        void KeepCameraOnCursor()
        {
            //selectionCam.transform.position = Vector3.Lerp()
            Vector3 toPos = new Vector3(GetTileSelectCursorTile().transform.position.x, GetTileSelectCursorTile().transform.position.y + camYOffset, 
                GetTileSelectCursorTile().transform.position.z);

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
                    //StartCoroutine(MoveCamera(selectionCam, ttp.GetUnitOnTile(tempTile)));
                }                
            }

            if (Input.GetKeyDown("s"))
            {
                Tile tempTile = ttp.GetTileAtPos(new Vector3(GetTileSelectCursorTile().transform.position.x
, GetTileSelectCursorTile().transform.position.y, GetTileSelectCursorTile().transform.position.z - 1));

                if (tempTile != null && tempTile.selectable && ttp.GetUnitOnTile(tempTile) != null)
                {
                    SetTileSelectCursorPos(tempTile);
                    //StartCoroutine(MoveCamera(selectionCam, ttp.GetUnitOnTile(tempTile)));
                }
            }

            if (Input.GetKeyDown("a"))
            {
                Tile tempTile = ttp.GetTileAtPos(new Vector3(GetTileSelectCursorTile().transform.position.x - 1
    , GetTileSelectCursorTile().transform.position.y, GetTileSelectCursorTile().transform.position.z));

                if (tempTile != null && tempTile.selectable && ttp.GetUnitOnTile(tempTile) != null)
                {
                    SetTileSelectCursorPos(tempTile);
                    //StartCoroutine(MoveCamera(selectionCam, ttp.GetUnitOnTile(tempTile)));
                }
            }

            if (Input.GetKeyDown("d"))
            {
                Tile tempTile = ttp.GetTileAtPos(new Vector3(GetTileSelectCursorTile().transform.position.x + 1
, GetTileSelectCursorTile().transform.position.y, GetTileSelectCursorTile().transform.position.z));

                if (tempTile != null && tempTile.selectable && ttp.GetUnitOnTile(tempTile) != null)
                {
                    SetTileSelectCursorPos(tempTile);
                    //StartCoroutine(MoveCamera(selectionCam, ttp.GetUnitOnTile(tempTile)));
                }
            }

            if (Input.GetKeyDown("e"))
            {
                Debug.Log("Doing it in here");

                targetUnit = ttp.GetUnitOnTile(GetTileSelectCursorTile());

                if (ttp.currentMagic != null)
                {
                    Debug.Log("---");
                    mp.PrepareMagic();
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

            /*if (Input.GetKeyDown("c"))
            {
                selectionOff = true;
                inSelection = false;
                selectMode = selectModes.PREPAREIDLE;
            }*/
        }

        public void ToggleBrain(bool set)
        {
            brain.enabled = set;
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

        private void MoveCameraForSelections(Vector3 newPos, Transform homeTransform)
        {

            // start here
            if (selectionOn && !camMoveReady)
            {
                selectionOn = false;
                positionComplete = false;
                rotationComplete = false;

                brain.enabled = false;
                /*selectionCam.transform.position = gameCam.transform.position;
                selectionCam.transform.rotation = gameCam.transform.rotation;

                selectionCam.SetActive(true);
                gameCam.SetActive(false);       */         

                toPos = newPos;
                Debug.Log("Going to position: " + toPos);

                camMoveReady = true;
            }

            if (positionComplete && rotationComplete) // add check if camera is in place too
            {
                Debug.Log("Movement complete");
                selectionCam.transform.position = toPos;
                selectionCam.transform.rotation = Quaternion.Euler(toRot);

                selectionCam.SetActive(true);
                gameCam.SetActive(false);
                mainCam.SetActive(false);

                //selectionCam.SetActive(true);
                //gameCam.SetActive(false);

                SetTileSelectCursorPos(GetTileHit());

                tileSelectCursor.SetActive(true);

                //FindObjectOfType<PlayerMovement>().controller.enabled = true; // weird workaround for now.

                // enable camera movement with WASD

                selectMode = selectModes.SELECTTILE;
            }

            if (camMoveReady)
            {
                MoveCamera(selectionCam, toPos);
            }
        }

        public void SetTileSelectCursorPos(Tile tile)
        {
            Vector3 newPos = new Vector3(tile.transform.position.x, tileSelectCursor.transform.position.y, tile.transform.position.z);
            tileSelectCursor.transform.position = newPos;
        }

        public void MoveCamera(GameObject camera, Vector3 toPosition)
        {
            if (!positionComplete)
            {
                camera.transform.position = Vector3.SmoothDamp(camera.transform.position, toPosition, ref refPos, camMovementTime);
                //camera.transform.position = Vector3.Lerp(camera.transform.position, toPosition, camMoveSpeed * Time.deltaTime);
                //camera.transform.position = Vector3.MoveTowards(camera.transform.position, toPos, camMoveSpeed * Time.deltaTime);


                if (Vector3.Distance(camera.transform.position, toPosition) <= camDistanceThreshold || fromMenu)
                {
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
                selectionCam.transform.position = mainCam.transform.position;
                selectionCam.transform.rotation = mainCam.transform.rotation;

                selectionCam.SetActive(true);
                mainCam.SetActive(false);

                camMoveReady = false;
            }
        }

        public IEnumerator MoveCamera(GameObject cam, Tile t)
        {
            Debug.Log("Moving camera to " + t.name);

            bool posComplete = false, rotComplete = false, moveComplete = false;
            Vector3 toPos = new Vector3(), toRot = new Vector3(), refPos = new Vector3();

            toPos = new Vector3(t.transform.position.x, camYOffset, t.transform.position.z);
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
            Debug.Log("Done moving camera");

        }

        public IEnumerator MoveCameraCombat(GameObject cam, BaseUnit unit)
        {
            Tile t = ttp.GetTileAtPos(unit.GetUnitObject().transform.position);

            Debug.Log("Moving camera to " + unit.name);

            bool posComplete = false, rotComplete = false, moveComplete = false;
            Vector3 toPos = new Vector3(), toRot = new Vector3(), refPos = new Vector3();

            toPos = new Vector3(t.transform.position.x, camYOffset, (t.transform.position.z - camZCombatOffset));
            toRot = new Vector3(camRotXCombatOffset, 0.0f, 0.0f);

            //mainCam.GetComponent<Camera>().fieldOfView = 60;

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

            Debug.Log("ZoomIn: " + zoomInFOV);

            Debug.Log("Zoom " + cam.fieldOfView + ", targetFOV: " + targetFOV);

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

            //Debug.LogError("No tile found");

            return null;
        }

        Tile GetTileHit(Transform camTransform)
        {
            RaycastHit[] hits;
            hits = Physics.RaycastAll(camTransform.position, camTransform.TransformDirection(Vector3.forward), 100.0f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.transform.tag == "Tile")
                {
                    //Debug.Log("Hit " + hits[i].transform.gameObject.name);

                    Debug.DrawRay(camTransform.position, camTransform.TransformDirection(Vector3.forward) * hits[i].distance * 2, Color.yellow);
                    return hits[i].collider.transform.GetComponent<Tile>();
                }
            }

            //Debug.LogError("No tile found");

            return null;
        }

        public Tile GetTileAtCoords(float x, float y)
        {
            RaycastHit[] hits;
            hits = Physics.RaycastAll(new Vector3(y, -50, x), Vector3.up, 200.0f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.transform.tag == "Tile")
                {
                    Debug.Log("Hit " + hits[i].transform.gameObject.name);

                    //Debug.DrawRay(camTransform.position, camTransform.TransformDirection(Vector3.forward) * hits[i].distance * 2, Color.yellow);
                    return hits[i].collider.transform.GetComponent<Tile>();
                }
            }

            Debug.LogError("No tile found at " + new Vector3(y, 0, x));
            return null;
        }

        public void ToggleTileSelectCursor(bool enabled)
        {
            tileSelectCursor.SetActive(enabled);
        }
    }
}
