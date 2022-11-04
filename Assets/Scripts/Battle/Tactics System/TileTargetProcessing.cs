using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class TileTargetProcessing : MonoBehaviour
    {
        int tileLayer = 1 << 6;

        BattleStateMachine bsm;
        TileSelection ts;

        public BaseTileTarget currentBTT;
        public BaseMagic currentMagic;
        public BaseUsableItem currentItem;

        // Start is called before the first frame update
        void Start()
        {
            bsm = FindObjectOfType<BattleStateMachine>();
            ts = FindObjectOfType<TileSelection>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public BaseTileTarget GetBaseTileTarget(BaseUnit unit, BaseMagic magic)
        {
            BaseTileTarget btt = new BaseTileTarget
            {
                homeTile = bsm.TileStandingOn(unit.GetUnitObject()),
                targetTiles = GetTargetTiles(magic.targetRange)
            };

            currentBTT = btt;

            return btt;
        }

        public BaseTileTarget GetBaseTileTarget(BaseUnit unit, BaseUsableItem item)
        {
            BaseTileTarget btt = new BaseTileTarget
            {
                homeTile = bsm.TileStandingOn(unit.GetUnitObject()),
                targetTiles = GetTargetTiles(item.targetRange)
            };

            return btt;
        }

        public BaseTileTarget GetBaseTileTarget(BaseUnit unit)
        {
            BaseTileTarget btt = new BaseTileTarget
            {
                homeTile = bsm.TileStandingOn(unit.GetUnitObject()),
                targetTiles = GetTargetTiles(1)
            };

            return btt;
        }

        public List<Tile> GetTargetTiles(int range) // Tiles for available targetting
        {
            List<Tile> tiles = new List<Tile>();
            float x = bsm.TileStandingOn(bsm.currentUnit.GetUnitObject()).transform.position.z;
            float y = bsm.TileStandingOn(bsm.currentUnit.GetUnitObject()).transform.position.x;

            switch (range)
            {
                case 0:
                    // return tile player is standing on

                    tiles.Add(bsm.TileStandingOn(bsm.currentUnit.GetUnitObject()));

                    return tiles;

                case 1:
                    // return tiles 1 range further

                    if (GetTileAtPos(new Vector3((y + 1), 0, x)))
                    {
                        tiles.Add(GetTileAtPos(new Vector3((y + 1), 0, x)));
                    }

                    if (GetTileAtPos(new Vector3((y - 1), 0, x)))
                    {
                        tiles.Add(GetTileAtPos(new Vector3((y - 1), 0, x))); // Left
                    }

                    if (GetTileAtPos(new Vector3(y, 0, (x + 1))))
                    {
                        tiles.Add(GetTileAtPos(new Vector3(y, 0, (x + 1)))); // Up
                    }

                    if (GetTileAtPos(new Vector3(y, 0, (x - 1))))
                    {
                        tiles.Add(GetTileAtPos(new Vector3(y, 0, (x - 1)))); // Bottom
                    }

                    return tiles; // need to change

                case 2:
                    // return tiles from above, plus 1 range further

                    tiles.Add(bsm.TileStandingOn(bsm.currentUnit.GetUnitObject()));

                    if (GetTileAtPos(new Vector3((y + 1), 0, x)))
                    {
                        tiles.Add(GetTileAtPos(new Vector3((y + 1), 0, x)));
                    }

                    if (GetTileAtPos(new Vector3((y - 1), 0, x)))
                    {
                        tiles.Add(GetTileAtPos(new Vector3((y - 1), 0, x))); // Left
                    }

                    if (GetTileAtPos(new Vector3(y, 0, (x + 1))))
                    {
                        tiles.Add(GetTileAtPos(new Vector3(y, 0, (x + 1)))); // Up
                    }

                    if (GetTileAtPos(new Vector3(y, 0, (x - 1))))
                    {
                        tiles.Add(GetTileAtPos(new Vector3(y, 0, (x - 1)))); // Bottom
                    }

                    return tiles; // need to change
                default:
                    Debug.LogError("GetTargetTiles - No ID found");
                    return null;
            }
        }

        public List<Tile> GetTileRange(int range, Tile anchorTile) // Tiles for range of effect
        {
            List<Tile> tiles = new List<Tile>();

            switch (range)
            {
                case 0:
                    // return anchor tile
                    tiles.Add(anchorTile);
                    Debug.Log("Returning anchor tile " + anchorTile.gameObject.name);

                    break;
                case 1:
                    // return anchor plus tiles above, below, and to sides
                    tiles.Add(anchorTile);
                    // - here. medical herb and heal will both use this
                    Tile tempTile = GetTileAtPos(new Vector3(anchorTile.transform.position.x - 1, anchorTile.transform.position.y, anchorTile.transform.position.z));
                    if (tempTile != null) tiles.Add(tempTile);

                    tempTile = GetTileAtPos(new Vector3(anchorTile.transform.position.x + 1, anchorTile.transform.position.y, anchorTile.transform.position.z));
                    if (tempTile != null) tiles.Add(tempTile);

                    tempTile = GetTileAtPos(new Vector3(anchorTile.transform.position.x, anchorTile.transform.position.y, anchorTile.transform.position.z - 1));
                    if (tempTile != null) tiles.Add(tempTile);

                    tempTile = GetTileAtPos(new Vector3(anchorTile.transform.position.x, anchorTile.transform.position.y, anchorTile.transform.position.z + 1));
                    if (tempTile != null) tiles.Add(tempTile);

                    break;
                case 2:
                    tiles.Add(anchorTile);
                    break;
                case 3:
                    tiles.Add(anchorTile);
                    break;
                default:
                    Debug.LogError("GetTargetTiles - No ID found");
                    return null;
            }

            return tiles;
        }

        public void BeginTileSelectForAction()
        {
            // bring up tile targetting
            Debug.Log("Prepare tile targetting");

            // set a new variable on TileSelection to the nearest available target. use that when moving camera.
            ts.selectionOn = true;
            ts.targetMode = targetModes.PREPARATION; //(when ready, uncomment)
        }

        public Tile GetTileAtPos(Vector3 position)
        {
            RaycastHit hit;
            Tile tile = null;

            if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, tileLayer))
            {
                //Debug.Log("Hit collider: " + hit.collider.gameObject.name);
                tile = hit.collider.GetComponent<Tile>();
            }

            //if (!tile) Debug.LogWarning("GetTileAtPos: No tile found at position " + position);

            return tile;
        }

        public BaseUnit GetUnitOnTile(Tile tile)
        {
            RaycastHit[] hits;

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

        public BaseUnit GetUnitOnTile(Tile tile, TargetTypes targetType)
        {
            RaycastHit[] hits;

            Vector3 posToTry = new Vector3(tile.transform.position.x, 1, tile.transform.position.z);

            hits = Physics.RaycastAll(posToTry, Vector3.down, 5.0f);

            for (int i = 0; i < hits.Length; i++)
            {
                switch (targetType)
                {
                    case TargetTypes.PLAYER:
                        if (hits[i].collider.CompareTag("PlayerUnit"))
                        {
                            return hits[i].collider.GetComponent<TacticsMove>().unit;
                        }
                        break;
                    case TargetTypes.ENEMY:
                        if (hits[i].collider.CompareTag("EnemyUnit"))
                        {
                            return (BaseUnit)hits[i].collider.GetComponent<EnemyTacticsMove>().enemyUnit;
                        }
                        break;
                    case TargetTypes.ANY:
                        if (hits[i].collider.CompareTag("PlayerUnit"))
                        {
                            return hits[i].collider.GetComponent<TacticsMove>().unit;
                        }

                        if (hits[i].collider.CompareTag("EnemyUnit"))
                        {
                            return (BaseUnit)hits[i].collider.GetComponent<EnemyTacticsMove>().enemyUnit;
                        }
                        break;
                }
            }
            return null;
        }

        public BaseUnit GetFirstAvailableTarget(TargetTypes targetType)
        {
            if (targetType == TargetTypes.SELF)
            {
                return GetUnitOnTile(currentBTT.homeTile);
            }

            foreach (Tile tile in currentBTT.targetTiles)
            {
                if (GetUnitOnTile(tile, targetType) != null)
                {
                    return GetUnitOnTile(tile, targetType);
                }
            }

            return null;
        }
    }
}