using DeepFry;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUnit", menuName = "Units/PlayerUnit", order = 1)]
public class PlayerUnitSO : ScriptableObject
{
    public int ID;
    public new string name;

    public unitRaces unitRace;

    public int exp, maxExp, level;

    public PlayerUnitClasses unitClass;

    public int HP, maxHP, MP, maxMP;
    public int attack, defense, agility, move;

    public MagicSO[] learnedMagic;

    public BaseItemSO[] items;

    public GameObject unitPrefab;    

    public BasePlayerUnit GetPlayerUnit()
    {
        Debug.LogWarning("Getting player unit for " + name);
        BasePlayerUnit playerUnit = new BasePlayerUnit
        {
            name = name,
            ID = ID,
            unitType = unitTypes.PLAYER,

            unitRace = unitRace,

            level = level,
            exp = exp,
            maxExp = maxExp,

            learnedMagicSOs = learnedMagic,

            items = items,

            unitClass = unitClass,

            HP = HP,
            MP = MP,
            maxHP = maxHP,
            maxMP = maxMP,

            attack = attack,
            defense = defense,
            agility = agility,
            move = move,

            unitPrefab = unitPrefab
        };

        playerUnit.unitRace = unitRace;

        return playerUnit;
    }
}
