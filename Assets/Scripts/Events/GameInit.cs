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

        private void Update()
        {

        }

        private void GenerateStartingUnits()
        {
            if (!DB.GameDB.dbInitialized)
            {
                DB.GameDB.SetInitialPlayerUnits(DB.GameDB.allPlayerUnits);
            }     
        }
    }
}