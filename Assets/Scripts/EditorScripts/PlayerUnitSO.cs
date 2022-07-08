using UnityEngine;
using DeepFry;

[CreateAssetMenu(fileName = "PlayerUnit", menuName = "Units/PlayerUnit", order = 1)]
public class PlayerUnitSO : ScriptableObject
{
    public int ID;
    public new string name;
    public int level;

    public float speed;

    public GameObject unitPrefab;

    public BasePlayerUnit GetPlayerUnit()
    {
        BasePlayerUnit playerUnit = new BasePlayerUnit();

        playerUnit.name = name;
        playerUnit.unitType = unitTypes.PLAYER;

        playerUnit.level = level;

        playerUnit.speed = speed;

        playerUnit.unitPrefab = unitPrefab;

        return playerUnit;
    }
}
