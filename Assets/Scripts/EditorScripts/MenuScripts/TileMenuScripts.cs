using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
                t.name = "Tile (" + t.transform.position.x + ", " + t.transform.position.z + ")";
            }
        }
    }
}