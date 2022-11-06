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

    public MagicSO[] learnedMagic;

    public BaseItemSO[] items;

    public GameObject unitPrefab;

    public bool boss;

    public BaseEnemyUnit GetEnemyUnit(int battleID)
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

            learnedMagic = learnedMagic,

            items = items,

            attackRange = attackRange,

            unitPrefab = unitPrefab,

            battleID = battleID,

            unitRace = unitRace,

            boss = boss
        };

        return enemyUnit;
    }
}
