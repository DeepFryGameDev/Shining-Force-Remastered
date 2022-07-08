using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace DeepFry
{
    public static class BattleInit
    {
        public static List<EnemyUnitSO> enemyCombatants = new List<EnemyUnitSO>();
        public static List<PlayerUnitSO> playerCombatants = new List<PlayerUnitSO>();

        public static List<BaseUnit> combatants = new List<BaseUnit>();

        public static void InitializeBattle(List<PlayerUnitSO> playerUnits, List<EnemyUnitSO> enemyUnits)
        {
            InitPlayerUnits(playerUnits);
            InitEnemyUnits(enemyUnits);

            InitField();
        }

        static void InitField()
        {
            Debug.Log("Initializing Field");

            SceneManager.LoadScene("Battleground");
        }

        static void InitPlayerUnits(List<PlayerUnitSO> playerUnits)
        {
            Debug.Log("Initializing Player Units");
            
            playerCombatants.Clear();
            playerCombatants = playerUnits;

            foreach (PlayerUnitSO playerUnitSO in playerCombatants)
            {
                Debug.Log("Added " + playerUnitSO.GetPlayerUnit().name + " to combatants");
                combatants.Add(playerUnitSO.GetPlayerUnit());
            }
        }

        static void InitEnemyUnits(List<EnemyUnitSO> enemyUnits)
        {
            Debug.Log("Initializing Enemy Units");

            enemyCombatants.Clear();
            enemyCombatants = enemyUnits;

            foreach (EnemyUnitSO enemyUnitSO in enemyCombatants)
            {
                Debug.Log("Added " + enemyUnitSO.GetEnemyUnit().name + " to enemy combatants");
                combatants.Add(enemyUnitSO.GetEnemyUnit());
            }
        }
    }
}