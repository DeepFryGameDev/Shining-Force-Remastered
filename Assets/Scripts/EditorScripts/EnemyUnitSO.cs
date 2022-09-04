using UnityEngine;
using DeepFry;

[CreateAssetMenu(fileName = "EnemyUnit", menuName = "Units/EnemyUnit", order = 1)]
public class EnemyUnitSO : ScriptableObject
{
    public int ID;
    public new string name;

    public unitRaces unitRace;

    public int HP, maxHP, MP, maxMP;
    public int attack, defense, agility, move;

    public GameObject unitPrefab;    

    public BaseEnemyUnit GetEnemyUnit()
    {
        BaseEnemyUnit enemyUnit = new BaseEnemyUnit();

        enemyUnit.name = name;
        enemyUnit.unitType = unitTypes.ENEMY;

        enemyUnit.HP = HP;
        enemyUnit.MP = MP;
        enemyUnit.maxHP = maxHP;
        enemyUnit.maxMP = maxMP;

        enemyUnit.attack = attack;
        enemyUnit.defense = defense;
        enemyUnit.agility = agility;
        enemyUnit.move = move;

        enemyUnit.unitPrefab = unitPrefab;

        enemyUnit.unitRace = unitRace;

        return enemyUnit;
    }
}
