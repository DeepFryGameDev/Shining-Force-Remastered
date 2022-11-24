using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace DeepFry
{
    public static class BattleInit
    {
        public static List<BaseEnemyEncounter> enemyCombatants = new List<BaseEnemyEncounter>();
        public static List<BasePlayerUnit> playerCombatants = new List<BasePlayerUnit>();

        public static List<BaseUnit> combatants = new List<BaseUnit>();

        static int battleIDIndex;

        public static void InitializeBattle(List<BaseEnemyEncounter> enemyUnits, int sceneIndex)
        {
            InitPlayerUnits(DB.GameDB.activePlayerUnits);
            InitEnemyUnits(enemyUnits);

            InitField(sceneIndex);
        }

        static void InitField(int sceneIndex)
        {
            Debug.Log("Initializing Field");

            SceneManager.LoadScene(sceneIndex);
        }

        static void InitPlayerUnits(List<BasePlayerUnit> playerUnits)
        {
            Debug.Log("Initializing Player Units");
            
            playerCombatants.Clear();
            playerCombatants = playerUnits;

            foreach (BasePlayerUnit playerUnit in playerCombatants)
            {
                Debug.Log("Added " + playerUnit.name + " to combatants");
                playerUnit.battleID = battleIDIndex;
                battleIDIndex++;
                combatants.Add(playerUnit);
            }
        }

        static void InitEnemyUnits(List<BaseEnemyEncounter> enemyUnits)
        {
            Debug.Log("Initializing Enemy Units");

            enemyCombatants.Clear();

            enemyCombatants = enemyUnits;

            foreach (BaseEnemyEncounter bee in enemyCombatants)
            {
                Debug.Log("Added " + bee.enemy.name + " to enemy combatants using ID " + battleIDIndex);
                combatants.Add(bee.enemy.GetEnemyUnit(battleIDIndex));
                bee.battleID = battleIDIndex;
                battleIDIndex++;
            }
        }
    }
}