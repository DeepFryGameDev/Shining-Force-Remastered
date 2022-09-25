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

    public MagicSO[] learnedMagic;

    public ItemSO[] inventory;

    public int HP, maxHP, MP, maxMP;
    public int attack, defense, agility, move;

    public GameObject unitPrefab;    

    public BasePlayerUnit GetPlayerUnit()
    {
        BasePlayerUnit playerUnit = new BasePlayerUnit();

        playerUnit.name = name;
        playerUnit.ID = ID;
        playerUnit.unitType = unitTypes.PLAYER;

        playerUnit.unitRace = unitRace;

        playerUnit.level = level;
        playerUnit.exp = exp;
        playerUnit.maxExp = maxExp;

        playerUnit.learnedMagic = learnedMagic;
        
        playerUnit.inventory = inventory;

        playerUnit.unitClass = unitClass;

        playerUnit.HP = HP;
        playerUnit.MP = MP;
        playerUnit.maxHP = maxHP;
        playerUnit.maxMP = maxMP;

        playerUnit.attack = attack;
        playerUnit.defense = defense;
        playerUnit.agility = agility;
        playerUnit.move = move;

        playerUnit.unitPrefab = unitPrefab;

        playerUnit.unitRace = unitRace;

        return playerUnit;
    }
}
