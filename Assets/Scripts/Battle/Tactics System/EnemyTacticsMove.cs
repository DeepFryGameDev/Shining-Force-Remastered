using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

namespace DeepFry
{
    public class EnemyTacticsMove : TacticsMove
    {
        public BaseEnemyUnit enemyUnit;

        GameObject target;
        BaseUnit targetUnit;

        TileTargetProcessing ttp;
        CombatInteraction ci;
        TileSelection ts;

        BattleCamera batCam;

        // Start is called before the first frame update
        void Start()
        {
            ttp = FindObjectOfType<TileTargetProcessing>();
            ci = FindObjectOfType<CombatInteraction>();
            ts = FindObjectOfType<TileSelection>();

            batCam = FindObjectOfType<BattleCamera>();
        }

        // Update is called once per frame
        void Update()
        {
            /*if (!moving)
            {
                FindNearestTarget();
                CalculatePath();
                FindSelectableTiles();
                actualTargetTile.target = true;
            } else
            {
                Debug.Log("Move to tile: " + actualTargetTile.gameObject.name);
            }*/
        }

        public void SetActualTargetTile()
        {
            FindNearestTarget();
            FindSelectableTiles();
            CalculatePath();
            //FindSelectableTiles();
            //actualTargetTile.target = true;

            //StartCoroutine(MoveUnit(actualTargetTile));

            StartCoroutine(MoveUnit(GetTargetTile()));
        }

        Tile GetTargetTile()
        {
            //Debug.Log("Trying to get actual target tile from origin tile: " + originTile.name);

            // set aggression - each tile number away is a higher chance of ignoring attack order
            // aggression = number of tiles away minus enemy's movement range
            // so if 20 tiles away, but enemy's movement range is 5, aggression is 15

            //Debug.Log("Number of tiles to target " + targetUnit.GetTile().name + " from current position: " +  GetNumberOfTilesToTarget(targetUnit.GetTile()));
            int aggression = GetNumberOfTilesToTarget(targetUnit.GetTile()) - move;
            Debug.Log("Aggression: " + aggression);

            if (aggression >= 9) // 90% chance to move a small amount of tiles in a random direction
            {
                int rand = GetRandomBetweenRange(1, 100);
                Debug.Log("Aggression >= 9 - Random chance to be move a couple tiles away: " + rand);
                if (rand <= 90) // Aggression >= 9 - Return a tile a small amount away in random direction
                {
                    rand = GetRandomBetweenRange(1, 2); // modify this at some point for small random moves (magic numbers)
                    return GetRandomTileFromCurrent(rand);
                } else
                {
                    Debug.Log("Aggression >= 9 - Return a tile toward target within 1 or 2 tiles");
                    rand = GetRandomBetweenRange(1, 2);
                    return GetTileAlongPath(rand);
                }
            } else if (aggression <= 1) // aggression * 10% chance to move random amount of tiles in random direction
            {                
                int rand = GetRandomBetweenRange(1, 100);
                Debug.Log("Aggression <= 1 - Random chance to move random amount of tiles: " + rand);

                if (rand <= (aggression * 10)) // Aggression <= 1 - Return 1 or 2 tiles away in a random direction
                {
                    rand = GetRandomBetweenRange(1, 2); // modify this at some point for small random moves (magic numbers)
                    return GetRandomTileFromCurrent(rand);
                }
                else // Aggression <= 1 - Return a tile toward target within movement
                {
                    rand = GetRandomBetweenRange(1, 2);
                    return GetTileAlongPath(rand);
                }
            } else // within attack range - return target's tile
            {
                return targetUnit.GetTile();
            }
        }

        Tile GetTileAlongPath(int spaces)
        {
            int count = 0;
            GetNumberOfTilesToTarget(targetUnit.GetTile());
            foreach (Tile t in pathToTargetTile)
            {
                count++;

                if (count == spaces)
                {
                    return t;
                }
            }
            return null;
        }

        Tile GetRandomTileFromCurrent(int spaces)
        {
            List<Tile> tilesAccountedFor = new List<Tile>();
            int tempCount = 1;
            Vector3 curTilePos = unit.GetTile().transform.position;
            Tile tempTile = unit.GetTile();

            for (int i = 0; i < spaces; i++)
            {
                bool found = false;
                RaycastHit[] hits = null;
                Vector3 posToTry = new Vector3(tempTile.transform.position.x, -0.045f, tempTile.transform.position.z);

                while (!found)
                {
                    int randomDirection = GetRandomBetweenRange(1, 4);
                    switch (randomDirection)
                    {
                        case 1: // up
                            hits = Physics.RaycastAll(posToTry, Vector3.forward, 1.0f);
                            break;
                        case 2: // right
                            hits = Physics.RaycastAll(posToTry, Vector3.right, 1.0f);
                            break;
                        case 3: // down
                            hits = Physics.RaycastAll(posToTry, Vector3.back, 1.0f);
                            break;
                        case 4: // left
                            hits = Physics.RaycastAll(posToTry, Vector3.left, 1.0f);
                            break;
                    }

                    for (int j = 0; j < hits.Length; j++)
                    {
                        if (hits[j].collider.CompareTag("Tile") && !tilesAccountedFor.Contains(hits[j].collider.GetComponent<Tile>()) 
                            && hits[j].collider.GetComponent<Tile>().walkable)
                        {
                            if (tempCount == spaces && hits[j].collider.GetComponent<Tile>().GetUnitOnTile() != null) // last space and is already being occupied by a unit
                            {
                                // try again
                                break;
                            }

                            tempTile = hits[j].collider.GetComponent<Tile>();
                            tilesAccountedFor.Add(tempTile);
                            found = true;
                            tempCount++;
                            break;
                        }

                        // no tile found (likely on border), try again
                    }
                }
            }

            return tempTile;
        }

        int GetRandomBetweenRange(int min, int max)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            return UnityEngine.Random.Range(min, max + 1);
        }

        void CalculatePath()
        {
            Tile targetTile = GetTargetTile(target);
            FindPath(targetTile);

            //CheckPath(actualTargetTile);
        }

        void FindNearestTarget()
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag("PlayerUnit");

            GameObject nearest = null;
            float distance = Mathf.Infinity;

            foreach (GameObject obj in targets)
            {
                float d = Vector3.Distance(transform.position, obj.transform.position);
                
                if (d < distance)
                {
                    distance = d;
                    nearest = obj;
                    // object is closer
                }
            }

            target = nearest;
            targetUnit = target.GetComponent<PlayerTacticsMove>().unit;
        }

        IEnumerator MoveUnit(Tile tarTile)
        {
            Vector3 navMeshTargetPos = new Vector3(tarTile.transform.position.x, 0, tarTile.transform.position.z);

            anim.SetBool("flyForward", true);

            agent.SetDestination(navMeshTargetPos);

            while (Vector3.Distance(transform.position, navMeshTargetPos) > tileStoppingDistance)
            {
                //Debug.Log("Distance: " + Vector3.Distance(transform.position, navMeshTargetPos));
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Stopping");
            anim.SetBool("flyForward", false);
            agent.isStopped = true;
            agent.velocity = new Vector3(0, 0, 0);
            agent.ResetPath();
            transform.position = new Vector3(navMeshTargetPos.x, transform.position.y, navMeshTargetPos.z);            

            StartCoroutine(PostMove());
        }

        IEnumerator PostMove()
        {
            // Check if targets in range (get number of tiles traversed to target tile, and verify that tile is within monster's reach)
            List<Tile> targetTiles = ttp.GetTargetTiles(enemyUnit.attackRange);
            
            List<BaseUnit> targetList = new List<BaseUnit>();
            targetList.Add(targetUnit); // <--- this and the line above will need to be adjusted

            foreach (Tile tile in targetTiles)
            {
                if (ttp.GetUnitOnTile(tile) == targetUnit)
                {
                    // prepare combat
                    StartCoroutine(PrepareCombatInteraction(targetList));

                    break;
                }
            }

            while (ci.interactionStarted)
            {
                yield return new WaitForEndOfFrame();
            }        

            // Then end turn
            GameObject.FindObjectOfType<BattleStateMachine>().battleState = battleStates.ENDTURN;
        }
        IEnumerator PrepareCombatInteraction(List<BaseUnit> targetList)
        {
            yield return new WaitForSeconds(1); // allows camera to move and settle on unit - this will be adjusted

            // simulate tile selection going to target.
            ts.targetUnit = targetUnit;

            ttp.BeginTileSelectForAction();
            ttp.currentBTT = ttp.GetBaseTileTarget(enemyUnit);

            yield return new WaitForSeconds(2);            

            ts.targetMode = targetModes.IDLE;
            ts.SetSelectableTiles(false);
            ts.inSelection = false;

            ts.mainCam.SetActive(true);
            ts.selectionCam.SetActive(false);
            batCam.gameCam.enabled = false;

            // zoom camera out like tile select for player
            // move cursor to target tile
            // anything else that is done before actions are started by player

            // build combat interaction

            ci.SetNewBaseCombatInteraction(CombatInteractionTypes.ATTACK, enemyUnit, targetList);
            ci.BuildNewCombatInteraction();
        }
    }
}

