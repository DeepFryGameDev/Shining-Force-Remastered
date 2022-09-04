using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class OptionsMenu : MonoBehaviour
    {
        MenuPrefabManager mpm;
        BattleStateMachine bsm;

        private void Start()
        {
            bsm = FindObjectOfType<BattleStateMachine>();
            mpm = FindObjectOfType<MenuPrefabManager>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !mpm.mapOpen)
            {
                mpm.OpenCanvas(mpm.optionsCanvas, true);
                bsm.currentUnit.GetUnitObject().GetComponent<PlayerMovement>().canMove = false;
                mpm.canOpenMap = false;
                mpm.optionsOpen = true;
                mpm.mouseCameraObject.SetActive(false);
            }

            if (mpm.optionsOpen)
            {
                if (Input.GetKeyDown("c"))
                {
                    mpm.OpenCanvas(mpm.optionsCanvas, false);
                    bsm.currentUnit.GetUnitObject().GetComponent<PlayerMovement>().canMove = true;
                    mpm.canOpenMap = true;

                    mpm.mouseCameraObject.SetActive(true);
                    
                    mpm.optionsOpen = false;
                }
            }
        }
    }
}