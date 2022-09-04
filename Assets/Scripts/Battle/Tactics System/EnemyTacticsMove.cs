using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class EnemyTacticsMove : TacticsMove
    {
        public BaseEnemyUnit enemyUnit;

        GameObject target;

        // Start is called before the first frame update
        void Start()
        {

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

            StartCoroutine(MoveUnit(actualTargetTile));
        }

        void CalculatePath()
        {
            Tile targetTile = GetTargetTile(target);
            FindPath(targetTile);

            CheckPath(actualTargetTile);
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

