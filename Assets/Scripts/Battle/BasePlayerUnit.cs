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
        public PlayerUnitClasses playerUnitClass;

        public int exp, maxExp, level;
    }
}