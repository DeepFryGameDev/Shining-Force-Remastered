using System.Collections;
using UnityEngine;

namespace DeepFry
{
    public enum unitTypes
    {
        PLAYER,
        ENEMY
    }

    public class BaseUnit
    {
        public string name;
        public int ID;
        public unitTypes unitType;
        public int level;

        public float speed;

        public GameObject unitPrefab;

        public GameObject unitObject;

        public void SetUnitObject(GameObject obj) { unitObject = obj; }

        public GameObject GetUnitObject() { return unitObject; }

        public IEnumerator ResetAnimator()
        {
            unitObject.GetComponent<Animator>().enabled = false;
            yield return new WaitForSeconds(0.05f);
            unitObject.GetComponent<Animator>().enabled = true;
        }
    }
}