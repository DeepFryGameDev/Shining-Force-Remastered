using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class Tile : MonoBehaviour
    {
        public bool walkable = true;
        public bool current = false;
        public bool target = false;
        public bool selectable = false;

        GameObject wall;

        //For colors
        float curAlpha;
        Color currentTileColor, targetTileColor, selectableTileColor, naTileColor;

        public List<Tile> adjacencyList = new List<Tile>();

        //Needed BFS (breadth first search)
        public bool visited = false;
        public Tile parent = null;
        public int distance = 0;

        //For A*
        public float f = 0;
        public float g = 0;
        public float h = 0;

        public int tileLayer = 1 << 6;

        // Use this for initialization
        void Start()
        {
            curAlpha = GetComponent<Renderer>().materials[0].color.a;
            selectableTileColor = new Color(1, 1, 1, curAlpha);
            naTileColor = new Color(1, 1, 1, 0);

            wall = transform.Find("Wall").gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (!walkable)
            {
                GetComponent<Renderer>().materials[0].color = naTileColor;
                GetComponent<Animator>().SetBool("selectable", false);
                wall.SetActive(true);
            }
            else if (selectable)
            {
                GetComponent<Animator>().SetBool("selectable", false);
                GetComponent<Renderer>().materials[0].color = selectableTileColor;
                GetComponent<Animator>().SetBool("selectable", true);
                wall.SetActive(false);
            }
            else
            {
                GetComponent<Renderer>().materials[0].color = naTileColor;
                GetComponent<Animator>().SetBool("selectable", false);
                wall.SetActive(true);
            }
        }

        public void Reset()
        {
            adjacencyList.Clear();

            current = false;
            target = false;
            selectable = false;

            visited = false;
            parent = null;
            distance = 0;

            f = g = h = 0;
        }

        public void FindNeighbors(float jumpHeight, Tile target)
        {
            Reset();

            CheckTile(Vector3.forward, jumpHeight, target);
            CheckTile(-Vector3.forward, jumpHeight, target);
            CheckTile(Vector3.right, jumpHeight, target);
            CheckTile(-Vector3.right, jumpHeight, target);
        }

        public void FindNeighborsForTarget(float jumpHeight, Tile target)
        {
            CheckTile(Vector3.forward, jumpHeight, target);
            CheckTile(-Vector3.forward, jumpHeight, target);
            CheckTile(Vector3.right, jumpHeight, target);
            CheckTile(-Vector3.right, jumpHeight, target);
        }

        public void CheckTile(Vector3 direction, float jumpHeight, Tile target)
        {
            Vector3 halfExtents = new Vector3(0.25f, (1 + jumpHeight) / 2.0f, 0.25f);
            Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents);

            foreach (Collider item in colliders)
            {
                Tile tile = item.GetComponent<Tile>();

                if (tile != null && tile.walkable)
                {
                    RaycastHit hit;

                    if (!Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1, tileLayer) || (tile == target))
                    {
                        adjacencyList.Add(tile);
                    }
                }
            }
        }

        public BaseUnit GetUnitOnTile()
        {
            RaycastHit[] hits;

            Vector3 posToTry = new Vector3(transform.position.x, 1, transform.position.z);

            hits = Physics.RaycastAll(posToTry, Vector3.down, 5.0f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.CompareTag("PlayerUnit") || hits[i].collider.CompareTag("EnemyUnit"))
                {
                    return hits[i].collider.GetComponent<TacticsMove>().unit;
                }
            }
            return null;
        }
    }

}
