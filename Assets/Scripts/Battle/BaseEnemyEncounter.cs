using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    [System.Serializable]
    public class BaseEnemyEncounter
    {
        public EnemyUnitSO enemy;
        public Vector2 spawnCoordinates;
        [HideInInspector] public int battleID;
    }
}