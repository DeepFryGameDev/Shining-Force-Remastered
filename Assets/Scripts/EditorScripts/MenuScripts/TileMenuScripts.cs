using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.AI;

namespace DeepFry
{
    public class TileMenuScripts
    {
        [MenuItem("DeepFryTools/Tile/Assign Tile Material")]
        public static void AssignTileMaterial()
        {
            GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
            Material material = Resources.Load<Material>("Tile/Tile");

            foreach (GameObject t in tiles)
            {
                t.GetComponent<Renderer>().material = material;
            }
        }

        [MenuItem("DeepFryTools/Tile/Assign Tile Name")]
        public static void AssignTileName()
        {
            GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

            foreach (GameObject t in tiles)
            {
                t.name = "Tile (" + t.transform.position.z + ", " + t.transform.position.x + ")";
            }
        }

        [MenuItem("DeepFryTools/Tile/Set Tile Coordinates Helper Text")]
        public static void SetTileCoordinates()
        {
            GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

            foreach (GameObject t in tiles)
            {
                t.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "(" + t.transform.position.z + ", " + t.transform.position.x + ")";
            }
        }

        [MenuItem("DeepFryTools/Tile/Set NavMesh Obstacles")]
        public static void SetNavMeshObstacles()
        {
            GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

            foreach (GameObject t in tiles)
            {
                NavMeshObstacle obstacle = t.GetComponent<NavMeshObstacle>();
                if (t.GetComponent<Tile>().walkable)
                {
                    obstacle.enabled = false;
                } else
                {
                    obstacle.enabled = true;
                }
            }
        }
    }
}