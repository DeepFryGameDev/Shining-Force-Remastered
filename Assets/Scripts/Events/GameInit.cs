using System;
using UnityEngine;

namespace DeepFry
{
    public class GameInit : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            GenerateStartingUnits();
        }

        private void GenerateStartingUnits()
        {
            foreach (PlayerUnitSO puSO in DB.GameDB.allPlayerUnits)
            {
                Debug.Log("-~-~-~- " + puSO.name + " added to active player units. -~-~-~-");
                DB.GameDB.activePlayerUnits.Add(puSO.GetPlayerUnit());
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}