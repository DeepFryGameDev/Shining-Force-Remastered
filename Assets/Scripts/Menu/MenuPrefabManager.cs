using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPrefabManager : MonoBehaviour
{
    public GameObject optionsCanvas;
    public GameObject mapCanvas;

    public GameObject mapUnitButtonPrefab;

    public GameObject mouseCameraObject;

    public bool mapOpen, optionsOpen, canOpenMap;

    private void Start()
    {
        canOpenMap = true;
    }

    public void OpenCanvas(GameObject obj, bool open)
    {
        if (open)
            obj.GetComponent<CanvasGroup>().alpha = 1;
        else
            obj.GetComponent<CanvasGroup>().alpha = 0;

        obj.GetComponent<CanvasGroup>().interactable = open;
        obj.GetComponent<CanvasGroup>().blocksRaycasts = open;
    }
}
