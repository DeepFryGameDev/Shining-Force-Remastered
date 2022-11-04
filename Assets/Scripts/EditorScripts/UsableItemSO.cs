using DeepFry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/New Usable Item", order = 1)]
public class UsableItemSO : BaseItemSO
{
    public UsableItemTypes usableType;

    public int targetRange;
    public TargetTypes targetType;
    public int effectRange;  
    public int effectValue;   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public BaseUsableItem GetBaseItem()
    {
        BaseUsableItem newItem = new BaseUsableItem
        {
            ID = ID,
            name = name,

            icon = icon,

            usableItemType = usableType,

            effectRange = effectRange,
            targetRange = targetRange,

            value = effectValue,

            targetType = targetType
        };


        return newItem;
    }
}