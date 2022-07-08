using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class TestAction : MonoBehaviour
    {
        public List<EnemyUnitSO> enemyList;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                Debug.Log("Start battle");
                Debug.Log("---");

                BattleInit.InitializeBattle(DB.GameDB.playerUnits, enemyList);
            }
        }
    }
}