using DeepFry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Magic", menuName = "Magic/Spell", order = 1)]
public class MagicSO : ScriptableObject
{
    public int ID;
    new public string name;

    public MagicTypes magicType;

    public int mpCost;

    public int targetRange;
    public int effectRange;

    public int value;

    public int[] levelsToUpgrade;

    public Sprite icon;

    public GameObject effectPrefab;
    public AudioClip effectSE;

    public TargetTypes targetType;

    public BaseMagic GetBaseMagic()
    {
        BaseMagic newMagic = new BaseMagic
        {
            ID = ID,
            name = name,
            
            magicType = magicType,

            mpCost = mpCost,

            icon = icon,

            effectPrefab = effectPrefab,
            effectAudio = effectSE,

            value = value,

            targetRange = targetRange,
            effectRange = effectRange,

            levelsToUpgrade = levelsToUpgrade,

            targetType = targetType
        };

        return newMagic;
    }
}
