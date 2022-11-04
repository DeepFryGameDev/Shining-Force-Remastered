using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MagicTypes
{
    HEAL,
    DAMAGE,
    MISC
}

namespace DeepFry
{
    public class BaseMagic
    {
        public int ID;
        public string name;
        public MagicTypes magicType;

        public int mpCost;
        public Sprite icon;

        public GameObject effectPrefab;
        public AudioClip effectAudio;

        public int value;

        public int targetRange;
        public int effectRange;

        public int[] levelsToUpgrade;

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