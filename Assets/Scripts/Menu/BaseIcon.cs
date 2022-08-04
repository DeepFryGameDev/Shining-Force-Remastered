using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    [CreateAssetMenu(fileName = "BaseIcon", menuName = "Icons/BaseIcon", order = 1)]

    public class BaseIcon : ScriptableObject
    {
        public Sprite defaultIcon, hoveredIcon;
    }
}
