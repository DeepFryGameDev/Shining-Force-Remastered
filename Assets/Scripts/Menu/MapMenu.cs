using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepFry
{
    public class MapMenu : MonoBehaviour
    {
        BattleStateMachine bsm;

        MenuPrefabManager mpm;

        public Transform EnemiesPanelParent, HeroesPanelParent;

        public TMP_Text text;

        public GameObject unitButtonPrefab;

        private void Start()
        {
            bsm = FindObjectOfType<BattleStateMachine>();
            mpm = FindObjectOfType<MenuPrefabManager>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && mpm.canOpenMap && !mpm.mapOpen)
            {
                GenerateUnitButtons();

                mpm.OpenCanvas(mpm.mapCanvas, true);
                bsm.currentUnit.GetUnitObject().GetComponent<PlayerMovement>().canMove = false;
                mpm.mapOpen = true;
                mpm.mouseCameraObject.SetActive(false);
            }

            if (mpm.mapOpen)
            {
                if (Input.GetKeyDown("c"))
                {
                    mpm.OpenCanvas(mpm.mapCanvas, false);
                    bsm.currentUnit.GetUnitObject().GetComponent<PlayerMovement>().canMove = true;

                    mpm.mouseCameraObject.SetActive(true);

                    mpm.mapOpen = false;
                }
            }
        }

        void GenerateUnitButtons()
        {

            foreach (Transform child in HeroesPanelParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in EnemiesPanelParent)
            {
                Destroy(child.gameObject);
            }

            foreach (BaseUnit unit in bsm.activeUnits)
            {
                GameObject unitButton = Instantiate(unitButtonPrefab);
                unitButton.transform.Find("NameText").GetComponent<Text>().text = unit.name;

                unitButton.GetComponent<UnitButtonBehavior>().unit = unit;

                if (unit.unitType == unitTypes.PLAYER)
                {
                    unitButton.transform.SetParent(HeroesPanelParent);

                }
                else if (unit.unitType == unitTypes.ENEMY)
                {
                    unitButton.transform.SetParent(EnemiesPanelParent);
                }
            }
        }

        public void SetText(string txt)
        {
            text.text = txt;
        }
    }
}