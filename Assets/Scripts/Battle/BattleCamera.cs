using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum CameraModes
{
    IDLE,
    PLAYERMOVE,
    PLAYERMENU
}

namespace DeepFry
{
    public class BattleCamera : MonoBehaviour
    {
        public CinemachineFreeLook gameCam; 
        public CinemachineVirtualCamera orbitCam;

        BattleStateMachine bsm;

        public CameraModes cameraMode;
        bool inOrbit;

        [Range(0.1f, 1.0f)]
        public float orbitSpeed = .25f;
        [Range(0.1f, 1.0f)]
        public float orbitFactor = .5f;

        string xInput, yInput;

        // Start is called before the first frame update
        void Start()
        {
            cameraMode = CameraModes.IDLE;
            bsm = FindObjectOfType<BattleStateMachine>();

            xInput = gameCam.m_XAxis.m_InputAxisName;
            yInput = gameCam.m_YAxis.m_InputAxisName;
        }

        // Update is called once per frame
        void Update()
        {
            switch (cameraMode)
            {
                case CameraModes.IDLE:
                    if (inOrbit) // stop orbit
                    {
                        CancelOrbit();
                    }

                    break;
                case CameraModes.PLAYERMOVE:
                    if (inOrbit) // stop orbit
                    {
                        CancelOrbit();
                    }

                    break;
                case CameraModes.PLAYERMENU:
                    if (!inOrbit)
                    {
                        PrepareOrbit();
                    } else
                    {
                        ProcessOrbit();
                    }
                    break;
            }
        }

        void ProcessOrbit()
        {
            orbitCam.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value -= orbitSpeed * orbitFactor;
        }

        void PrepareOrbit()
        {
            gameCam.m_XAxis.m_InputAxisName = string.Empty;
            gameCam.m_YAxis.m_InputAxisName = string.Empty;

            orbitCam.Follow = bsm.currentUnit.GetUnitObject().transform;
            orbitCam.LookAt = bsm.currentUnit.GetUnitObject().transform;

            orbitCam.transform.gameObject.SetActive(true);
            gameCam.transform.gameObject.SetActive(false);

            inOrbit = true;
        }

        void CancelOrbit()
        {
            Debug.Log(orbitCam.transform.position);
            gameCam.transform.position = orbitCam.transform.position;
            gameCam.transform.rotation = orbitCam.transform.rotation;
            Debug.Log(gameCam.transform.position);

            orbitCam.Follow = null;
            orbitCam.LookAt = null;

            gameCam.transform.gameObject.SetActive(true);
            orbitCam.transform.gameObject.SetActive(false);

            gameCam.m_XAxis.m_InputAxisName = xInput;
            gameCam.m_YAxis.m_InputAxisName = yInput;

            inOrbit = false;

        }
    }

}
