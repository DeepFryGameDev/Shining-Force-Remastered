using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepFry
{
    public enum menuStates
    {
        IDLE,
        MAIN,
        MAGIC,
        ITEM
    }

    public class BaseMenu : MonoBehaviour
    {
        public BaseIcon[] mainMenuIcons;
        public BaseIcon[] itemMenuIcons;

        //public turnStates turnState;
        public menuStates menuState;

        public GameObject mainMenu, magicItemMenu;

        public GameObject topButton, rightButton, bottomButton, leftButton;

        public bool buttonsSet;        

        public GameObject hoveredButton;

        

        private void Awake()
        {
            //turnState = turnStates.IDLE;
            menuState = menuStates.IDLE;
        }

        protected virtual void SetMainMenuButtons()
        {
            //
        }

        public void SetHovered(GameObject menuButton)
        {
            if (hoveredButton != menuButton)
            {
                hoveredButton = menuButton;

                foreach (Transform child in mainMenu.transform.Find("MenuPanel"))
                {
                    child.GetComponent<MenuIconBehavior>().hovered = false;
                }

                hoveredButton.GetComponent<MenuIconBehavior>().hovered = true;
            }
        }

        public void SetHovered(int num)
        {
            GameObject tempButton;
            switch (num)
            {
                case 0:
                    tempButton = topButton;
                    break;
                case 1:
                    tempButton = rightButton;
                    break;
                case 2:
                    tempButton = bottomButton;
                    break;
                case 3:
                    tempButton = leftButton;
                    break;
                default:
                    Debug.LogError("SetHovered - value invalid: " + num);
                    tempButton = null;
                    break;
            }

            SetHovered(tempButton);
        }
    }
}