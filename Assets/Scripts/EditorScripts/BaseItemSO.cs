using DeepFry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class BaseItemSO : ScriptableObject
{
    public int ID;
    new public string name;

    public Sprite icon;

    public int gilValue;

    public ItemTypes itemType;

    public BaseItem GetItem()
    {
        BaseItem item = new BaseItem
        {
            ID = ID,
            name = name,
            icon = icon,
            gilValue = gilValue,
            itemType = itemType
        };
        return item;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
