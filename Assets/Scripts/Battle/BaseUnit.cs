using System.Collections;
using UnityEngine;

namespace DeepFry
{
    public enum unitTypes
    {
        PLAYER,
        ENEMY
    }

    public enum unitRaces
    {
        HUMAN,
        CENTAUR,
        DEMON
    }

    public class BaseUnit
    {
        public string name;
        public int ID;
        public unitTypes unitType;
        public unitRaces unitRace;

        public int level;

        public int HP, maxHP, MP, maxMP;
        public int attack, defense, agility, move;

        public MagicSO[] learnedMagic;

        public BaseItemSO[] items;

        public GameObject unitPrefab;

        public int battleID;

        GameObject unitObject;

        int tileLayer = 1 << 6;

        public void SetUnitObject(GameObject obj) 
        {
            unitObject = obj; 
        }

        public GameObject GetUnitObject() 
        {
            return unitObject; 
        }

        public IEnumerator ResetAnimator()
        {
            unitObject.GetComponent<Animator>().enabled = false;
            yield return new WaitForSeconds(0.05f);
            unitObject.GetComponent<Animator>().enabled = true;
        }

        public Tile GetTile()
        {
            RaycastHit hit;
            Tile tile = null;

            if (unitObject == null)
            {
                Debug.LogError("NULL");
            }

            Vector3 tempPos = new Vector3(unitObject.transform.position.x, unitObject.transform.position.y - 50f, unitObject.transform.position.z);

            if (Physics.Raycast(tempPos, Vector3.up, out hit, Mathf.Infinity, tileLayer))
            {
                //Debug.Log("Hit collider: " + hit.collider.gameObject.name);
                tile = hit.collider.GetComponent<Tile>();
            }

            //if (!tile) Debug.LogWarning("GetTileAtPos: No tile found at position " + tempPos);

            return tile;
        }

        public BaseEnemyUnit GetBaseEnemyUnit() { return (BaseEnemyUnit)this; }

    }
}