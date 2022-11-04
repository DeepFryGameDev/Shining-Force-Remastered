using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItemSO : ScriptableObject
{
    public int ID;
    new public string name;

    public Sprite icon;

    public int gilValue;

    public ItemTypes itemType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
