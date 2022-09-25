using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

namespace DeepFry
{
    public class TacticsMove : MonoBehaviour
    {
        protected float tileStoppingDistance = 0.1f;

        public bool turn = false;

        List<Tile> selectableTiles = new List<Tile>();
        public List<Tile> tempSelectableTiles = new List<Tile>();

        GameObject[] tiles;

        Stack<Tile> path = new Stack<Tile>();
        public Tile currentTile;

        public bool moving = false;
        public int move = 5;
        public float jumpHeight = 2;
        public float moveSpeed = 2;
        public float jumpVelocity = 4.5f;

        public BaseUnit unit;

        public Tile lastTile;

        Vector3 velocity = new Vector3();
        Vector3 heading = new Vector3();

        float halfHeight = 0;

        bool fallingDown = false;
        bool jumpingUp = false;
        bool movingEdge = false;
        Vector3 jumpTarget;

        public int tileLayer = 1 << 6;

        public Tile actualTargetTile;

        List<Tile> pathToTargetTile = new List<Tile>();

        protected NavMeshAgent agent;
        protected Animator anim;

        BattleStateMachine bsm;

        public void Init()
        {
            tiles = GameObject.FindGameObjectsWithTag("Tile");

            halfHeight = GetComponent<Collider>().bounds.extents.y;

            agent = GetComponent<NavMeshAgent>();

            anim = GetComponent<Animator>();

            bsm = FindObjectOfType<BattleStateMachine>();

            //TurnManager.AddUnit(this);
        }

        public void GetCurrentTile()
        {
            currentTile = GetTargetTile(gameObject);
            currentTile.current = true;
        }

        public Tile GetTargetTile(GameObject target)
        {
            RaycastHit hit;
            Tile tile = null;

            if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, Mathf.Infinity, tileLayer))
            {
                //Debug.Log("Hit collider: " + hit.collider.gameObject.name);
                tile = hit.collider.GetComponent<Tile>();
            }

            return tile;
        }

        public void ComputeAdjacencyLists(float jumpHeight, Tile target)
        {
            //tiles = GameObject.FindGameObjectsWithTag("Tile");

            foreach (GameObject tile in tiles)
            {
                Tile t = tile.GetComponent<Tile>();
                t.FindNeighbors(jumpHeight, target);
            }
        }

        public void ComputeAdjacencyListsForTargetTile(float jumpHeight, Tile target)
        {
            //tiles = GameObject.FindGameObjectsWithTag("Tile");
           
            foreach (GameObject tile in tiles)
            {
                Tile t = tile.GetComponent<Tile>();
                if (t.selectable)
                {
                    t.FindNeighborsForTarget(jumpHeight, target);
                }                
            }
        }

        public void FindSelectableTiles()
        {
            ComputeAdjacencyLists(jumpHeight, null);
            GetCurrentTile();

            Queue<Tile> process = new Queue<Tile>();

            /*
            process.Enqueue(currentTile);
            currentTile.visited = true;

            if (lastTile != null)
            {
                process.Enqueue(lastTile);
                currentTile.visited = true;
                currentTile.selectable = true;
            } else
            {
                process.Enqueue(currentTile);
                currentTile.visited = true;
            }

            */

            process.Enqueue(currentTile);
            currentTile.visited = true;
            currentTile.selectable = true;

            //currentTile.parent = ??  leave as null 

            while (process.Count > 0)
            {
                Tile t = process.Dequeue();

                selectableTiles.Add(t);
                t.selectable = true;

                t.GetComponent<LandEffect>().SetMultipliers(unit);

                float tempMove = move - (t.distance * t.GetComponent<LandEffect>().GetMovementMultiplier());
                //Debug.Log("tempMove for " + t.gameObject.name + ": " + tempMove);

                if (t.distance < move)
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        if (!tile.visited)
                        {
                            tile.parent = t;
                            tile.visited = true;
                            tile.distance = 1 + t.distance;
                            process.Enqueue(tile);
                        }
                    }
                }

                /*if (t.distance < move)
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        if (!tile.visited)
                        {
                            tile.parent = t;
                            tile.visited = true;
                            tile.distance = 1 + t.distance;
                            process.Enqueue(tile);
                        }
                    }
                }*/
            }

            foreach (Tile t in selectableTiles)
            {
                CheckPath(t);
            }
        }

        public void SetLastTile()
        {
            GetCurrentTile();
            lastTile = currentTile;
        }       

        public void RemoveSelectableTiles()
        {
            if (currentTile != null)
            {
                currentTile.selectable = false;
                currentTile.current = false;
                currentTile = null;
            }

            foreach (Tile tile in selectableTiles)
            {
                tile.Reset();
            }

            selectableTiles.Clear();
        }

        void CalculateHeading(Vector3 target)
        {
            heading = target - transform.position;
            heading.Normalize();
        }

        void SetHorizotalVelocity()
        {
            velocity = heading * moveSpeed;
        }

        void Jump(Vector3 target)
        {
            if (fallingDown)
            {
                FallDownward(target);
            }
            else if (jumpingUp)
            {
                JumpUpward(target);
            }
            else if (movingEdge)
            {
                MoveToEdge();
            }
            else
            {
                PrepareJump(target);
            }
        }

        void PrepareJump(Vector3 target)
        {
            float targetY = target.y;
            target.y = transform.position.y;

            CalculateHeading(target);

            if (transform.position.y > targetY)
            {
                fallingDown = false;
                jumpingUp = false;
                movingEdge = true;

                jumpTarget = transform.position + (target - transform.position) / 2.0f;
            }
            else
            {
                fallingDown = false;
                jumpingUp = true;
                movingEdge = false;

                velocity = heading * moveSpeed / 3.0f;

                float difference = targetY - transform.position.y;

                velocity.y = jumpVelocity * (0.5f + difference / 2.0f);
            }
        }

        void FallDownward(Vector3 target)
        {
            velocity += Physics.gravity * Time.deltaTime;

            if (transform.position.y <= target.y)
            {
                fallingDown = false;
                jumpingUp = false;
                movingEdge = false;

                Vector3 p = transform.position;
                p.y = target.y;
                transform.position = p;

                velocity = new Vector3();
            }
        }

        void JumpUpward(Vector3 target)
        {
            velocity += Physics.gravity * Time.deltaTime;

            if (transform.position.y > target.y)
            {
                jumpingUp = false;
                fallingDown = true;
            }
        }

        void MoveToEdge()
        {
            if (Vector3.Distance(transform.position, jumpTarget) >= 0.05f)
            {
                SetHorizotalVelocity();
            }
            else
            {
                movingEdge = false;
                fallingDown = true;

                velocity /= 5.0f;
                velocity.y = 1.5f;
            }
        }

        protected Tile FindLowestF(List<Tile> list)
        {
            Tile lowest = list[0];

            foreach (Tile t in list)
            {
                if (t.f < lowest.f)
                {
                    lowest = t;
                }
            }

            list.Remove(lowest);

            return lowest;
        }

        protected Tile FindEndTile(Tile t)
        {
            Stack<Tile> tempPath = new Stack<Tile>();

            Tile next = t.parent;
            while (next != null)
            {
                tempPath.Push(next);
                next = next.parent;
            }

            if (tempPath.Count <= move)
            {
                return t.parent;
            }

            Tile endTile = null;
            for (int i = 0; i <= move; i++)
            {
                endTile = tempPath.Pop();
            }

            return endTile;
        }

        protected void FindPath(Tile target) // enemy usage
        {
            ComputeAdjacencyLists(jumpHeight, target);
            GetCurrentTile();

            List<Tile> openList = new List<Tile>();
            List<Tile> closedList = new List<Tile>();

            openList.Add(currentTile);
            //currentTile.parent = ??
            currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position);
            currentTile.f = currentTile.h;

            while (openList.Count > 0)
            {
                Tile t = FindLowestF(openList);

                closedList.Add(t);

                if (t == target)
                {
                    actualTargetTile = FindEndTile(t);

                    //UpdateTileMovementRating(actualTargetTile);

                    // Move enemy to actualTargetTile's position with navmesh
                    //StartCoroutine(MoveEnemyUnit(actualTargetTile));

                    return;
                }

                foreach (Tile tile in t.adjacencyList)
                {
                    if (closedList.Contains(tile))
                    {
                        //Do nothing, already processed
                    }
                    else if (openList.Contains(tile))
                    {
                        float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);

                        if (tempG < tile.g)
                        {
                            tile.parent = t;

                            tile.g = tempG;
                            tile.f = tile.g + tile.h;
                        }
                    }
                    else
                    {
                        tile.parent = t;

                        tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                        tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                        tile.f = tile.g + tile.h;

                        openList.Add(tile);
                    }
                }
            }

            //todo - what do you do if there is no path to the target tile?
            Debug.Log("Path not found");
        }

        protected void CheckPath(Tile target) // player usage
        {
            pathToTargetTile.Clear();

            //Debug.Log("Finding path from " + bsm.lastTile.gameObject.name + " to " + target.gameObject.name);

            ComputeAdjacencyListsForTargetTile(jumpHeight, target);

            List<Tile> openList = new List<Tile>(); //any tile that has not been processed
            List<Tile> closedList = new List<Tile>(); //any tile that has been processed
                                                      //when the target tile is added to the closedList, we have found the closest path to the target tile

            openList.Add(bsm.lastTile);
            //currentTile.parent = ??
            bsm.lastTile.h = Vector3.Distance(bsm.lastTile.transform.position, target.transform.position);
            bsm.lastTile.f = bsm.lastTile.h;

            while (openList.Count > 0)
            {
                Tile t = FindLowestF(openList);
                //Debug.Log("1: " + t.gameObject.name);
                closedList.Add(t);
                foreach (Tile tile in closedList)
                {
                    //Debug.Log(tile.gameObject.name);
                }
                //Debug.Log("t: " + t.gameObject.name);
                //Debug.Log("target: " + target.gameObject.name);
                if (t == target)
                {
                    //Debug.Log("if t == target: true");

                    //actualTargetTile = FindEndTile(t);

                    //UpdateTileMovementRating(actualTargetTile);

                    UpdateTileMovementRating(target);

                    //Debug.Log("actual target file: " + actualTargetTile.gameObject.name);
                    return;
                }

                foreach (Tile tile in t.adjacencyList)
                {
                    //Debug.Log("in foreach loop: " + tile.gameObject.name);
                    if (closedList.Contains(tile))
                    {
                        //Debug.Log("do nothing");
                        //do nothing, already processed
                    }
                    else if (openList.Contains(tile))
                    {
                        //Debug.Log("if openlist contains tile in adjecency list");
                        float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                        if (tempG < tile.g) //found quicker way to target
                        {
                            //Debug.Log("tempG < tile.g");
                            tile.parent = t;

                            tile.g = tempG;
                            tile.f = tile.g + tile.h;
                        }
                    }
                    else //never processed the tile
                    {
                        //Debug.Log("never processed " + tile.gameObject.name);
                        tile.parent = t;

                        tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                        tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                        tile.f = tile.g + tile.h;

                        openList.Add(tile);
                        //Debug.Log("added to openList: " + tile.gameObject.name);
                    }
                }

            }

            //todo: what do you do if there is no path to the target tile?  Likely just skip turn I'm thinking
            Debug.Log("Path not found");
        }

        private void UpdateTileMovementRating(Tile targetTile)
        {
            List<Tile> pathTiles = new List<Tile>();

            path.Clear();
            Tile next = targetTile;
            while (next != null)
            {
                path.Push(next);

                if (!pathTiles.Contains(next))
                {
                    pathTiles.Add(next);
                    next = next.parent;
                }
                else
                {
                    break;
                }
            }

            // Movement should start at unit's Move var
            // for each tile in the path, 1 movement point should be reduced.  This value is multiplied by the tile's movement rating.
            // if the overall value is still above 0, the tile can be reached.  If the value is lower than 0, the tile should be marked as 'selectable=false'.

            float movePoints = (float)move;

            foreach (Tile t in pathTiles)
            {
                movePoints -= (1 * t.GetComponent<LandEffect>().GetMovementMultiplier());
            }

            if (movePoints >= 0)
            {
                //Debug.Log("Can move to " + targetTile.gameObject.name);

                if (!tempSelectableTiles.Contains(targetTile))
                {
                    tempSelectableTiles.Add(targetTile);
                }
            }
            else
            {
                //Debug.Log("Cannot move to " + targetTile.gameObject.name);
                targetTile.selectable = false;
            }
        }
    }

}
