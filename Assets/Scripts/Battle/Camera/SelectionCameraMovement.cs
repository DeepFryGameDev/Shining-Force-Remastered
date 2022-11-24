using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.IMGUI.Controls.PrimitiveBoundsHandle;

namespace DeepFry
{
    public class SelectionCameraMovement : MonoBehaviour
    {
        [Range(0, 5f)]
        public float camYOffset = 2.5f;
        [Range(0, 5f)]
        public float camZOffset = 4.3f;
        [Range(0, 180f)]
        public float camRotXOffset = 90.0f;
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
        [Range(10, 55)]
        public float zoomInFOV = 45;
        [Range(35, 70)]
        public float zoomOutFOV = 60;

        [Range(0, 1)]
        public float tileSelectCursorOffset = 0.06f;

        public float sensitivityX = 5F;
        public float sensitivityY = 5F;
        public float minimumX = -360F;
        public float maximumX = 360F;
        public float minimumY = -90F;
        public float maximumY = 90F;
        float rotationY = -90F;

        Vector3 lastGoodLocation;

        TileSelection ts;
        TileTargetProcessing ttp;
        StatusMenu sm;

        private void Start()
        {
            ts = FindObjectOfType<TileSelection>();
            ttp = FindObjectOfType<TileTargetProcessing>();
            sm = FindObjectOfType<StatusMenu>();
        }

        void Update()
        {
            switch (ts.tileSelectMode)
            {
                case tileSelectMode.CHECK:
                    switch (ts.selectMode)
                    {
                        case selectModes.SELECTTILE:
                            GetInput();

                            if (GetSelectedTile() != null)
                            {
                                SetTileSelectCursorPos(GetSelectedTile());
                            }
                            else
                            {
                                HideCursor();
                            }

                            if (Input.GetKeyDown("c"))
                            {
                                Debug.Log("Go back to turn mode");
                                ts.selectionOff = true;
                                ts.inSelection = false;
                                ts.selectMode = selectModes.PREPAREIDLE;
                            }
                            break;
                        case selectModes.UNITSELECTED:
                            if (Input.GetKeyDown("c"))
                            {
                                Debug.Log("Back out from selected unit");
                                ts.mpm.OpenCanvas(sm.gameObject, false);
                                sm.statusWindowOpened = false;
                                ts.selectMode = selectModes.SELECTTILE;                                
                            }
                            break;
                    }
                    break;
                case tileSelectMode.ACTION:
                    //Debug.Log("Ready for input");
                    break;
            }
                
        }
        private void SetTileSelectCursorPos(Tile tile)
        {
            Vector3 newPos = new Vector3(tile.transform.position.x, (tile.transform.position.y + tileSelectCursorOffset), tile.transform.position.z);
            ts.tileSelectCursor.transform.position = newPos;
            if (!ts.tileSelectCursor.activeInHierarchy)
            {
                ts.tileSelectCursor.SetActive(true);
            }            
        }

        void HideCursor()
        {
            ts.tileSelectCursor.transform.position = new Vector3(0, 0, 0);
            if (ts.tileSelectCursor.activeInHierarchy)
            {
                ts.tileSelectCursor.SetActive(false);
            }
        }

        void GetInput()
        {
            if (Input.GetKeyDown("e") && GetSelectedTile() != null)
            {
                Debug.Log("Checking tile " + GetSelectedTile().gameObject.name);

                if (ttp.GetUnitOnTile(GetSelectedTile()) != null)
                {
                    if (ttp.GetUnitOnTile(GetSelectedTile()).unitType == unitTypes.PLAYER)
                    {
                        sm.DisplayStatusWindow(ttp.GetUnitOnTile(GetSelectedTile()) as BasePlayerUnit);
                        sm.statusWindowOpened = true;
                        ts.selectMode = selectModes.UNITSELECTED;
                    }
                    else if (ttp.GetUnitOnTile(GetSelectedTile()).unitType == unitTypes.ENEMY)
                    {
                        sm.DisplayStatusWindow(ttp.GetUnitOnTile(GetSelectedTile()) as BaseEnemyUnit);
                        sm.statusWindowOpened = true;
                        ts.selectMode = selectModes.UNITSELECTED;
                    }
                }
            }

            if (Input.GetKey("w"))
            {
                transform.Translate(Vector3.forward * (Input.GetAxis("Vertical") * camMoveSpeed * Time.deltaTime), Space.Self);
            }

            if (Input.GetKey("d"))
            {
                transform.Translate(Vector3.right * (Input.GetAxis("Horizontal") * camMoveSpeed * Time.deltaTime), Space.Self);
            }

            if (Input.GetKey("s"))
            {
                transform.Translate(Vector3.back * (-Input.GetAxis("Vertical") * camMoveSpeed * Time.deltaTime), Space.Self);
            }

            if (Input.GetKey("a"))
            {
                transform.Translate(Vector3.left * (-Input.GetAxis("Horizontal") * camMoveSpeed * Time.deltaTime), Space.Self);
            }

            MouseControls();

            if (GetCurrentTile() != null)
            {
                lastGoodLocation = transform.position;
            } else
            {
                if (transform.position != lastGoodLocation)
                {
                    transform.position = lastGoodLocation;
                }                
            }

            // force distance from ground
            transform.position = new Vector3(transform.position.x, GetCurrentTile().transform.position.y + 2.37f, transform.position.z);
        }

        void MouseControls()
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }

        Tile GetSelectedTile()
        {
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), 3f);

            for (int i = 0; i < hits.Length; i++)
            {
                //Debug.Log("Hitting " + hits[i].collider.gameObject.name);
                if (hits[i].collider.transform.tag == "Tile")
                {
                    //Debug.Log("Hit " + hits[i].transform.gameObject.name);

                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hits[i].distance, Color.yellow);
                    return hits[i].collider.transform.GetComponent<Tile>();
                }
            }

            //Debug.LogError("No tile found");

            return null;
        }

        Tile GetCurrentTile()
        {
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, Vector3.down, 100.0f);

            for (int i = 0; i < hits.Length; i++)
            {
                //Debug.Log("Hitting " + hits[i].collider.gameObject.name);
                if (hits[i].collider.transform.tag == "Tile")
                {
                    //Debug.Log("Hit " + hits[i].transform.gameObject.name);

                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hits[i].distance * 2, Color.yellow);
                    return hits[i].collider.transform.GetComponent<Tile>();
                }
            }

            //Debug.LogError("No tile found");

            return null;
        }
    }
}

