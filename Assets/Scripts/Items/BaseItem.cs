using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class BaseItem
    {
        public int ID;
        public string name;

        public Sprite icon;

        public int targetRange;
        public int effectRange;

        public int value;

        public TargetTypes targetType;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}