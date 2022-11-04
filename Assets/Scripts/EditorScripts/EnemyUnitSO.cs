using UnityEngine;
using DeepFry;

[CreateAssetMenu(fileName = "EnemyUnit", menuName = "Units/EnemyUnit", order = 1)]
public class EnemyUnitSO : ScriptableObject
{
    public int ID;
    public new string name;

    public unitRaces unitRace;

    public int level;

    public int attackRange;

    public int HP, maxHP, MP, maxMP;
    public int attack, defense, agility, move;

    public GameObject unitPrefab;    

    public BaseEnemyUnit GetEnemyUnit()
    {
        BaseEnemyUnit enemyUnit = new BaseEnemyUnit
        {
            name = name,
            unitType = unitTypes.ENEMY,

            level = level,
            HP = HP,
            MP = MP,
            maxHP = maxHP,
            maxMP = maxMP,

            attack = attack,
            defense = defense,
            agility = agility,
            move = move,

            attackRange = attackRange,

            unitPrefab = unitPrefab,

            unitRace = unitRace
        };

        return enemyUnit;
    }
}
