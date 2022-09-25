using DeepFry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/New Item", order = 1)]
public class ItemSO : ScriptableObject
{
    public int ID;
    new public string name;

    public int targetRange;
    public int effectRange;

    public Sprite icon;

    public int value;

    public TargetTypes targetType;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public BaseItem GetBaseItem()
    {
        BaseItem newItem = new BaseItem
        {
            ID = ID,
            name = name,

            icon = icon,

            effectRange = effectRange,
            targetRange = targetRange,

            value = value,

            targetType = targetType
        };


        return newItem;
    }
}