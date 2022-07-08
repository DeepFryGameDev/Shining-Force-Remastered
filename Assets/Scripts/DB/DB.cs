using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class DB : MonoBehaviour
    {
        public List<PlayerUnitSO> playerUnits;


        public static DB GameDB { get; private set; }

        private void Awake()
        {
            if (GameDB != null && GameDB != this)
            {
                Destroy(this);
            }
            else
            {
                GameDB = this;
            }
        }
    }
}