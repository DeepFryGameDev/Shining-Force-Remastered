using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class EnemyTacticsMove : TacticsMove
    {
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
            CalculatePath();
            FindSelectableTiles();
            //actualTargetTile.target = true;
        }

        void CalculatePath()
        {
            Tile targetTile = GetTargetTile(target);
            FindPath(targetTile);
        }

        void FindNearestTarget()
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

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
    }
}

