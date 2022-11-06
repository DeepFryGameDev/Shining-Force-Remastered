using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class TestAction : MonoBehaviour
    {
        public List<BaseEnemyEncounter> enemyList;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                Debug.Log("Start battle");
                Debug.Log("---");

                BattleInit.InitializeBattle(enemyList);
            }
        }
    }
}