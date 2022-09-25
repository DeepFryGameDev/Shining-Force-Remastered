using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DeepFry
{
    public class StatusMenu : MonoBehaviour
    {
        public bool statusWindowOpened;

        MenuPrefabManager mpm;

        // Start is called before the first frame update
        void Start()
        {
            mpm = FindObjectOfType<MenuPrefabManager>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void DisplayStatusWindow(BaseUnit unit)
        {
            GenerateStatsPanel(unit);

            mpm.OpenCanvas(transform.gameObject, true);
        }

        void GenerateStatsPanel(BaseUnit unit)
        {
            transform.Find("StatusPanel/StatsPanel/NameText").GetComponent<TMP_Text>().text = unit.name;

            transform.Find("StatusPanel/StatsPanel/HPText").GetComponent<TMP_Text>().text = unit.HP.ToString() + " / " + unit.maxHP.ToString();
            transform.Find("StatusPanel/StatsPanel/MPText").GetComponent<TMP_Text>().text = unit.MP.ToString() + " / " + unit.maxMP.ToString();

            transform.Find("StatusPanel/StatsPanel/AttackText").GetComponent<TMP_Text>().text = unit.attack.ToString();
            transform.Find("StatusPanel/StatsPanel/DefenseText").GetComponent<TMP_Text>().text = unit.defense.ToString();
            transform.Find("StatusPanel/StatsPanel/AgilityText").GetComponent<TMP_Text>().text = unit.agility.ToString();
            transform.Find("StatusPanel/StatsPanel/MoveText").GetComponent<TMP_Text>().text = unit.move.ToString();

            if (unit as BasePlayerUnit != null)
            {
                BasePlayerUnit bpu = unit as BasePlayerUnit;
                transform.Find("StatusPanel/StatsPanel/ClassText").GetComponent<TMP_Text>().text = bpu.unitClass.ToString();
                transform.Find("StatusPanel/StatsPanel/LevelText").GetComponent<TMP_Text>().text = bpu.level.ToString();

                transform.Find("StatusPanel/StatsPanel/EXPText").GetComponent<TMP_Text>().text = bpu.exp.ToString();
            }

            if (unit as BaseEnemyUnit != null)
            {
                transform.Find("StatusPanel/StatsPanel/ClassText").GetComponent<TMP_Text>().text = "";
                transform.Find("StatusPanel/StatsPanel/LevelText").GetComponent<TMP_Text>().text = "N / A"; // eventually make global variable for "N / A"

                transform.Find("StatusPanel/StatsPanel/EXPText").GetComponent<TMP_Text>().text = "N / A";
            }
        }
    }
}

