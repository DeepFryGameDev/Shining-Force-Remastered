using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class UnitButtonBehavior : MonoBehaviour
    {
        MenuPrefabManager mpm;
        StatusMenu sm;
        MapMenu mm;

        public BaseUnit unit;

        void Start()
        {
            mpm = FindObjectOfType<MenuPrefabManager>();
            sm = FindObjectOfType<StatusMenu>();
            mm = FindObjectOfType<MapMenu>();
        }

        void Update()
        {

        }

        public void OnCursorEnter()
        {
            if (unit.unitType == unitTypes.ENEMY)
            {
                mm.SetText(unit.name);
            } else if (unit.unitType == unitTypes.PLAYER)
            {
                BasePlayerUnit bpu = unit as BasePlayerUnit;
                mm.SetText(bpu.unitClass.ToString() + " - " + bpu.name);
            }
        }

        public void OnCursorExit()
        {
            mm.SetText("");
        }

        public void OnCursorClick()
        {
            sm.DisplayStatusWindow(unit);
        }
    }
}

