using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum selectModes
{
    START,
    SELECTTILE,
    MENU,
    UNITSELECTED,
    PREPAREIDLE,
    IDLE
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
        public float camRotThreshold = .3f;
        [Range(0, 5f)]
        public float camMoveSpeed = 3f;

        public bool selectionOn, selectionOff;

        public selectModes selectMode;

        public GameObject mainCam, selectionCam, tileSelectCursor;

        BattleStateMachine bsm;
        MenuPrefabManager mpm;

        bool camMoveReady, rotationComplete, positionComplete;
        Vector3 toPos, toRot, refPos;

        MapMenu mm;
        StatusMenu sm;
        

        void Start()
        {
            selectMode = selectModes.START;
            bsm = GetComponent<BattleStateMachine>();

            mpm = FindObjectOfType<MenuPrefabManager>();

            camMoveReady = false;

            toRot = new Vector3(camRotXOffset, 0.0f, 0.0f);

            mm = FindObjectOfType<MapMenu>();
            sm = FindObjectOfType<StatusMenu>();
        }


        void Update()
        {
            switch (selectMode)
            {
                case selectModes.START:
                    if (selectionOn)
                    {
                        selectionOn = false;
                        positionComplete = false;
                        rotationComplete = false;

                        selectionCam.transform.position = mainCam.transform.position;
                        selectionCam.transform.rotation = mainCam.transform.rotation;

                        selectionCam.SetActive(true);
                        mainCam.SetActive(false);

                        toPos = new Vector3(bsm.currentUnit.GetUnitObject().GetComponent<TacticsMove>().lastTile.transform.position.x, 
                            camYOffset,
                            bsm.currentUnit.GetUnitObject().GetComponent<TacticsMove>().lastTile.transform.position.z - camZOffset);

                        camMoveReady = true;
                    }

                    if (bsm.selectModeReady && positionComplete && rotationComplete) // add check if camera is in place too
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

                    break;
                case selectModes.SELECTTILE:

                    HandleCameraMovementInput();

                    if (Input.GetKeyDown("e"))
                    {
                        Debug.Log("Checking tile " + GetTileHit().gameObject.name);

                        if (GetUnitOnTile(GetTileHit()) != null)
                        {
                            if (GetUnitOnTile(GetTileHit()).unitType == unitTypes.PLAYER)
                            {
                                sm.DisplayStatusWindow(GetUnitOnTile(GetTileHit()) as BasePlayerUnit);
                                sm.statusWindowOpened = true;
                                selectMode = selectModes.UNITSELECTED;
                            }
                            else if (GetUnitOnTile(GetTileHit()).unitType == unitTypes.ENEMY)
                            {
                                sm.DisplayStatusWindow(GetUnitOnTile(GetTileHit()) as BaseEnemyUnit);
                                sm.statusWindowOpened = true;
                                selectMode = selectModes.UNITSELECTED;
                            }
                        }
                    }

                    if (Input.GetKeyDown("c"))
                    {
                        selectionOff = true;
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
                        mainCam.SetActive(true);
                        selectionCam.SetActive(false);
                        selectMode = selectModes.IDLE;

                        bsm.workingPTM.GetComponent<PlayerMovement>().controller.enabled = true;
                        bsm.turnState = turnStates.MOVE;

                        bsm.selectableTilesFound = false;
                    }

                    break;
                case selectModes.IDLE:

                    break;
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

                if (Vector3.Distance(selectionCam.transform.position, toPos) <= camDistanceThreshold)
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

        BaseUnit GetUnitOnTile(Tile tile)
        {
            RaycastHit[] hits;
            Debug.Log(tile.transform.position);

            Vector3 posToTry = new Vector3(tile.transform.position.x, 1, tile.transform.position.z);

            hits = Physics.RaycastAll(posToTry, Vector3.down, 5.0f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.CompareTag("PlayerUnit"))
                {
                    return hits[i].collider.GetComponent<TacticsMove>().unit;
                }

                if (hits[i].collider.CompareTag("EnemyUnit"))
                {
                    return (BaseUnit)hits[i].collider.GetComponent<EnemyTacticsMove>().enemyUnit;
                }
            }
            return null;
        }
    }
}
