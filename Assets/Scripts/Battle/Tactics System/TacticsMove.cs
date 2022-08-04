using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DeepFry
{
    public class TacticsMove : MonoBehaviour
    {
        public bool turn = false;

        List<Tile> selectableTiles = new List<Tile>();
        GameObject[] tiles;

        Stack<Tile> path = new Stack<Tile>();
        Tile currentTile;

        public bool moving = false;
        public int move = 5;
        public float jumpHeight = 2;
        public float moveSpeed = 2;
        public float jumpVelocity = 4.5f;

        Vector3 velocity = new Vector3();
        Vector3 heading = new Vector3();

        float halfHeight = 0;

        bool fallingDown = false;
        bool jumpingUp = false;
        bool movingEdge = false;
        Vector3 jumpTarget;

        int tileLayer = 1 << 6;

        public Tile actualTargetTile;

        NavMeshAgent agent;

        public void Init()
        {
            tiles = GameObject.FindGameObjectsWithTag("Tile");

            halfHeight = GetComponent<Collider>().bounds.extents.y;

            agent = GetComponent<NavMeshAgent>();

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

        public void FindSelectableTiles()
        {
            ComputeAdjacencyLists(jumpHeight, null);
            GetCurrentTile();

            Queue<Tile> process = new Queue<Tile>();

            process.Enqueue(currentTile);
            currentTile.visited = true;
            //currentTile.parent = ??  leave as null 

            while (process.Count > 0)
            {
                Tile t = process.Dequeue();

                selectableTiles.Add(t);
                t.selectable = true;

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
            }
        }

        public void Move()
        {
            if (path.Count > 0)
            {
                Tile t = path.Peek();
                Vector3 target = t.transform.position;

                //Calculate the unit's position on top of the target tile
                target.y += halfHeight + t.GetComponent<Collider>().bounds.extents.y;

                if (Vector3.Distance(transform.position, target) >= 0.05f)
                {
                    bool jump = transform.position.y != target.y;

                    if (jump)
                    {
                        Jump(target);
                    }
                    else
                    {
                        CalculateHeading(target);
                        SetHorizotalVelocity();
                    }

                    //Locomotion
                    transform.forward = heading;
                    transform.position += velocity * Time.deltaTime;
                }
                else
                {
                    //Tile center reached
                    transform.position = target;
                    path.Pop();
                }
            }
            else
            {
                RemoveSelectableTiles();
                moving = false;

                //TurnManager.EndTurn();
            }
        }

        protected void RemoveSelectableTiles()
        {
            if (currentTile != null)
            {
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

        protected void FindPath(Tile target)
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

                    // Move enemy to actualTargetTile's position with navmesh
                    StartCoroutine(MoveUnit());

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

        IEnumerator MoveUnit()
        {
            Vector3 navMeshTargetPos = new Vector3(actualTargetTile.transform.position.x, 0, actualTargetTile.transform.position.z);

            GetComponent<Animator>().SetBool("flyForward", true);

            agent.SetDestination(navMeshTargetPos);

            while (Vector3.Distance(transform.position, navMeshTargetPos) > 0.1f)
            {
                //Debug.Log("Distance: " + Vector3.Distance(transform.position, navMeshTargetPos));
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Stopping");
            GetComponent<Animator>().SetBool("flyForward", false);
            agent.isStopped = true;
            agent.ResetPath();

            transform.position = navMeshTargetPos;

            PostMove();
        }

        void PostMove()
        {
            // Check if targets in range

            // If not, skip turn.
            GameObject.FindObjectOfType<BattleStateMachine>().SetBattleState(battleStates.ENDTURN);
        }
    }

}
