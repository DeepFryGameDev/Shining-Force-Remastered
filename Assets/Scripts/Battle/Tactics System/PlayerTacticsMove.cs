using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class PlayerTacticsMove : TacticsMove
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {          

        }

        public IEnumerator MovePlayerUnit(Tile tarTile)
        {
            gameObject.GetComponent<PlayerMovement>().ToggleCanMove();
            //gameObject.GetComponent<PlayerMovement>().controller.enabled = false;

            transform.LookAt(tarTile.transform.position);

            Vector3 navMeshTargetPos = new Vector3(tarTile.transform.position.x, 0, tarTile.transform.position.z);

            Debug.Log("Moving player unit to : " + navMeshTargetPos);
            anim.SetBool("isRunning", true);

            agent.SetDestination(navMeshTargetPos);

            while (Vector3.Distance(transform.position, navMeshTargetPos) > tileStoppingDistance)
            {
                //Debug.Log("Distance: " + Vector3.Distance(transform.position, navMeshTargetPos));
                yield return new WaitForEndOfFrame();
            }

            anim.SetBool("isRunning", false);
            agent.isStopped = true;
            agent.velocity = new Vector3(0, 0, 0);
            agent.ResetPath();

            transform.position = new Vector3(navMeshTargetPos.x, transform.position.y, navMeshTargetPos.z);
        }
    }
}
