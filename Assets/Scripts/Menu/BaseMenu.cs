using System;
using System.Collections;
using System.Collections.Generic;
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
        //public Sprite[] mainMenuIcons;
        public BaseIcon[] mainMenuIcons;
        public Sprite[] itemMenuIcons;
        public Sprite[] magicMenuIcons;

        public turnStates turnState;
        public menuStates menuState;

        public GameObject topButton, rightButton, bottomButton, leftButton;

        bool menuDrawn;

        public GameObject hoveredButton;
        BattleStateMachine bsm;

        private void Awake()
        {
            bsm = FindObjectOfType<BattleStateMachine>();

            turnState = turnStates.IDLE;
            menuState = menuStates.IDLE;

            topButton = transform.Find("MenuPanel/TopButton").gameObject;
            rightButton = transform.Find("MenuPanel/RightButton").gameObject;
            bottomButton = transform.Find("MenuPanel/BottomButton").gameObject;
            leftButton = transform.Find("MenuPanel/LeftButton").gameObject;
        }

        public void SetHovered(GameObject menuButton)
        {
            if (hoveredButton != menuButton)
            {
                hoveredButton = menuButton;

                foreach (Transform child in transform.Find("MenuPanel"))
                {
                    child.GetComponent<MenuIconBehavior>().hovered = false;
                }

                hoveredButton.GetComponent<MenuIconBehavior>().hovered = true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            switch (turnState)
            {
                case turnStates.IDLE:
                    // hide menu
                    HideMenu();

                    break;
                case turnStates.MENU:
                    // draw menu
                    ShowMenu();

                    if (Input.GetKeyDown("e"))
                    {
                        Invoke(hoveredButton.GetComponent<MenuIconBehavior>().commandMethod, 0f);
                    }

                    break;
            }

            if (!menuDrawn)
            {
                switch (menuState)
                {
                    case menuStates.IDLE:
                        // Do nothing

                        break;
                    case menuStates.MAIN:
                        SetMainMenuButtons();
                        break;
                    case menuStates.ITEM:

                        break;
                    case menuStates.MAGIC:

                        break;
                }
            }
        }

        void HideMenu()
        {
            if (menuDrawn)
            {
                Debug.Log("Hiding menu");

                GetComponent<Animator>().SetBool("menuOpened", false);

                menuState = menuStates.IDLE;

                menuDrawn = false;

                turnState = turnStates.IDLE;
            }
        }

        void ShowMenu()
        {
            if (!menuDrawn)
            {
                Debug.Log("Showing menu");

                menuState = menuStates.MAIN;

                // play open menu animation
                GetComponent<Animator>().SetBool("menuOpened", true);
            }
        }

        void SetMainMenuButtons()
        {
            // set the 4 icons starting at N and going clockwise to W
            topButton.transform.Find("Icon").GetComponent<Image>().sprite = mainMenuIcons[0].defaultIcon;
            rightButton.transform.Find("Icon").GetComponent<Image>().sprite = mainMenuIcons[1].defaultIcon;
            bottomButton.transform.Find("Icon").GetComponent<Image>().sprite = mainMenuIcons[2].defaultIcon;
            leftButton.transform.Find("Icon").GetComponent<Image>().sprite = mainMenuIcons[3].defaultIcon;

            topButton.GetComponent<MenuIconBehavior>().SetIcons(mainMenuIcons[0].defaultIcon, mainMenuIcons[0].hoveredIcon);
            rightButton.GetComponent<MenuIconBehavior>().SetIcons(mainMenuIcons[1].defaultIcon, mainMenuIcons[1].hoveredIcon);
            bottomButton.GetComponent<MenuIconBehavior>().SetIcons(mainMenuIcons[2].defaultIcon, mainMenuIcons[2].hoveredIcon);
            leftButton.GetComponent<MenuIconBehavior>().SetIcons(mainMenuIcons[3].defaultIcon, mainMenuIcons[3].hoveredIcon);

            topButton.GetComponent<MenuIconBehavior>().commandMethod = "AttackButtonPressed";
            rightButton.GetComponent<MenuIconBehavior>().commandMethod = "ItemButtonPressed";
            bottomButton.GetComponent<MenuIconBehavior>().commandMethod = "StayButtonPressed";
            leftButton.GetComponent<MenuIconBehavior>().commandMethod = "MagicButtonPressed";

            // set hovered
            SetHovered(transform.Find("MenuPanel/BottomButton").gameObject);

            menuDrawn = true;
        }

        void StayButtonPressed()
        {
            Debug.Log("Staying");
            EndTurn();
        }

        void AttackButtonPressed()
        {
            Debug.Log("Attack");
        }

        void ItemButtonPressed()
        {
            Debug.Log("Item");
        }

        void MagicButtonPressed()
        {
            Debug.Log("Magic");
        }

        void EndTurn()
        {
            HideMenu();
            bsm.battleState = battleStates.ENDTURN;
        }
    }
}