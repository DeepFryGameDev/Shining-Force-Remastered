using UnityEngine;

public enum PlayerUnitClasses
{
    SDMN,
    PRST
}

namespace DeepFry 
{
    public class BasePlayerUnit : BaseUnit
    {
        public PlayerUnitClasses unitClass;

        public int exp, maxExp, level;

        public MagicSO[] learnedMagic;

        public ItemSO[] inventory;
    }
}