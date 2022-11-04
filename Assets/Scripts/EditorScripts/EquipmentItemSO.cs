using DeepFry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Equip", menuName = "Items/New Equipment", order = 1)]
public class EquipmentItemSO : BaseItemSO
{   
    public EquipmentTypes equipType;

    public int attackRange;

    public int attack;

    public bool equipped;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public BaseEquipment GetBaseEquip()
    {
        BaseEquipment newEquip = new BaseEquipment
        {
            ID = ID,
            name = name,

            icon = icon,

            equipType = equipType,

            attack = attack,

            equipped = equipped,

            attackRange = attackRange
        };


        return newEquip;
    }
}