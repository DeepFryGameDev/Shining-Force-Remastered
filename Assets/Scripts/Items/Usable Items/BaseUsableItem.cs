using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class BaseUsableItem : BaseItem
    {
        public UsableItemTypes usableItemType;

        public int targetRange;
        public int effectRange;

        public int value;

        public TargetTypes targetType;
    }
}