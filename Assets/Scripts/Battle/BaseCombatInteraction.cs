using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatInteractionTypes
{
    ATTACK,
    MAGIC,
    ITEM
}

namespace DeepFry
{
    public class BaseCombatInteraction
    {
        public BaseUnit primaryUnit;
        public List<BaseUnit> targetUnits = new List<BaseUnit>();
        public BaseMagic magicUsed;
        public BaseUsableItem itemUsed;
        public CombatInteractionTypes interactionType;
    }
}
