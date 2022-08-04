using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepFry
{
    public class MapCamera : MonoBehaviour
    {
        public Transform map;

        void Update()
        {
            //GetInput();
        }

        void GetInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateLeft();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                RotateRight();
            }
        }

        public void RotateLeft()
        {
            map.Rotate(Vector3.up, 90, Space.Self);
        }

        public void RotateRight()
        {
            map.Rotate(Vector3.up, -90, Space.Self);
        }
    }
}

