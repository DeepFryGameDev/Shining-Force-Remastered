using UnityEngine;
using DeepFry;

[CreateAssetMenu(fileName = "EnemyUnit", menuName = "Units/EnemyUnit", order = 1)]
public class EnemyUnitSO : ScriptableObject
{
    public int ID;
    public new string name;
    public int level;

    public float speed;

    public GameObject unitPrefab;

    public BaseEnemyUnit GetEnemyUnit()
    {
        BaseEnemyUnit enemyUnit = new BaseEnemyUnit();

        enemyUnit.name = name;
        enemyUnit.unitType = unitTypes.ENEMY;

        enemyUnit.level = level;

        enemyUnit.speed = speed;

        enemyUnit.unitPrefab = unitPrefab;

        return enemyUnit;
    }
}
