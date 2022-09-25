using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepFry
{
    public enum itemMenuModes
    {
        IDLE,
        USE,
        GIVE,
        DROP,
        EQUIP
    }

    public class BattleMenu : BaseMenu
    {
        BattleStateMachine bsm;
        BattleCamera batCam;
        MenuPrefabManager mpm;
        TileSelection ts;

        MagicItemMenu mim;
        TMP_Text commandNameText;

        BasePlayerUnit currentPlayerUnit;

        public itemMenuModes itemMenuMode;

        private void Awake()
        {
            mim = FindObjectOfType<MagicItemMenu>();
            bsm = FindObjectOfType<BattleStateMachine>();
            batCam = FindObjectOfType<BattleCamera>();
            mpm = FindObjectOfType<MenuPrefabManager>();
            ts = FindObjectOfType<TileSelection>();

            commandNameText = GameObject.Find("MainMenu/TextBG/MenuText").GetComponent<TMP_Text>();

            menuState = menuStates.IDLE;
            itemMenuMode = itemMenuModes.IDLE;

            SetMainMenuButtons();
        }
        void Update()
        {
            switch (bsm.turnState)
            {
                case turnStates.IDLE:

                    break;

                case turnStates.MOVE:
                    if (Input.GetKeyDown("c") && bsm.turnState != turnStates.SELECT)
                    {
                        bsm.turnState = turnStates.SELECT;
                    }
                    break;
                case turnStates.MENU:

                    // draw menu

                    if (itemMenuMode == itemMenuModes.IDLE)
                    {
                        if (Input.GetKeyDown("e"))
                        {
                            Invoke(hoveredButton.GetComponent<MenuIconBehavior>().commandMethod, 0f);
                        }

                        if (Input.GetKeyDown("w"))
                        {
                            SetHovered(topButton);
                        }

                        if (Input.GetKeyDown("d"))
                        {
                            SetHovered(rightButton);
                        }

                        if (Input.GetKeyDown("s"))
                        {
                            SetHovered(bottomButton);
                        }

                        if (Input.GetKeyDown("a"))
                        {
                            SetHovered(leftButton);
                        }

                        switch (menuState)
                        {
                            
                            case menuStates.MAIN:
                                if (!buttonsSet)
                                {
                                    ShowMenu(mainMenu, 2);

                                    buttonsSet = true;
                                }

                                if (Input.GetKeyDown("c"))
                                {
                                    //Debug.Log("Go back to movement phase");
                                    HideMenu(mainMenu);

                                    commandNameText.text = "";

                                    menuState = menuStates.IDLE;

                                    bsm.turnState = turnStates.MOVE;
                                    batCam.cameraMode = CameraModes.PLAYERMOVE;
                                    bsm.selectableTilesFound = false;

                                    mpm.canOpenMap = true;
                                }

                                break;
                            case menuStates.ITEM:
                                if (Input.GetKeyDown("c"))
                                {
                                    //Debug.Log("Go back to main menu");
                                    HideMenu(mainMenu);

                                    SetMainMenuButtons();

                                    ShowMenu(mainMenu, 2);

                                    menuState = menuStates.MAIN;
                                }

                                break;
                            case menuStates.MAGIC:
                                if (Input.GetKeyDown("c"))
                                {
                                    //Debug.Log("Go back to main menu");
                                    HideMenu(magicItemMenu);

                                    SetMainMenuButtons();

                                    ShowMenu(mainMenu, 2);

                                    menuState = menuStates.MAIN;
                                }
                                break;
                        }
                    } else
                    {
                        if (Input.GetKeyDown("c"))
                        {
                            if (ts.inSelection)
                            {
                                Debug.Log("Open inventory again");
                                ts.fromMenu = true;
                                OpenInventory();
                            } else
                            {
                                mim.HideMenu();
                                mim.menuMode = MagicItemMenuModes.IDLE;

                                itemMenuMode = itemMenuModes.IDLE;
                                menuState = menuStates.ITEM;

                                ShowMenu(mainMenu, 0);
                            }
                        }
                        
                    }
                    break;

                case turnStates.SELECT:



                    break;
            }
        }

        void ShowMagicMenu()
        {
            Debug.Log("Showing magic menu");

            mim.SetAndShowUnitMagic((BasePlayerUnit)bsm.currentUnit);

            menuState = menuStates.MAGIC;
            mim.menuMode = MagicItemMenuModes.MAGIC;

            mim.GetComponent<Animator>().SetBool("menuOpened", true);
        }

        public void OpenInventory()
        {
            currentPlayerUnit = (BasePlayerUnit)bsm.currentUnit;

            // confirm if unit has anything in inventory. if nothing, play cancel SE and don't open menu. otherwise, proceed
            if (currentPlayerUnit.inventory.Length > 0)
            {
                mim.SetAndShowUnitItems(currentPlayerUnit);
                HideMenu(mainMenu);
                HideMenu(magicItemMenu);
                ShowMenu(magicItemMenu, 0);

                menuState = menuStates.ITEM;
                mim.menuMode = MagicItemMenuModes.ITEM;
            }
            else
            {
                Debug.Log(currentPlayerUnit.name + " has no items in inventory.");
            }
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

        public void ItemButtonPressed()
        {
            HideMenu(mainMenu);
            SetItemMenuButtons();
            ShowMenu(mainMenu, 0);

            menuState = menuStates.ITEM;
        }

        void UseItemButtonPressed()
        {
            itemMenuMode = itemMenuModes.USE;

            OpenInventory();
        }

        void EquipItemButtonPressed()
        {
            Debug.Log("Equip Item button pressed");
            itemMenuMode = itemMenuModes.EQUIP;

            OpenInventory();
        }

        void DropItemButtonPressed()
        {
            Debug.Log("Drop Item button pressed");
            itemMenuMode = itemMenuModes.DROP;

            OpenInventory();
        }

        void GiveItemButtonPressed()
        {
            Debug.Log("Give Item button pressed");
            itemMenuMode = itemMenuModes.GIVE;

            OpenInventory();
        }

        void MagicButtonPressed()
        {
            // Hide menu
            HideMenu(mainMenu);

            // Display learned magic abilities in UI
            ShowMagicMenu();
        }

        public void HideMenu(GameObject menu)
        {
            Debug.Log("Hiding menu");

            menu.GetComponent<Animator>().SetBool("menuOpened", false);

            menuState = menuStates.IDLE;

            topButton.GetComponent<MenuIconBehavior>().hovered = false;
            rightButton.GetComponent<MenuIconBehavior>().hovered = false;
            bottomButton.GetComponent<MenuIconBehavior>().hovered = false;
            leftButton.GetComponent<MenuIconBehavior>().hovered = false;
        }

        void EndTurn()
        {
            HideMenu(mainMenu);

            commandNameText.text = "";

            batCam.cameraMode = CameraModes.IDLE;

            bsm.battleState = battleStates.ENDTURN;
            bsm.turnState = turnStates.IDLE;
        }

        public void ShowMenu(GameObject menu, int defaultHoveredButton)
        {
            // play open menu animation
            menu.GetComponent<Animator>().SetBool("menuOpened", true);

            buttonsSet = false;

            if (menu == mainMenu)
            {
                if (menuState == menuStates.ITEM)
                {
                    GameObject.Find("MainMenu/TextBG/MenuText").GetComponent<TMP_Text>().text = itemMenuIcons[defaultHoveredButton].commandName;
                } else
                {
                    GameObject.Find("MainMenu/TextBG/MenuText").GetComponent<TMP_Text>().text = mainMenuIcons[defaultHoveredButton].commandName;
                }                
            }

            switch (defaultHoveredButton)
            {
                case 0:
                    SetHovered(topButton);
                    topButton.GetComponent<MenuIconBehavior>().hovered = true;
                    break;
                case 1:
                    SetHovered(rightButton);
                    rightButton.GetComponent<MenuIconBehavior>().hovered = true;
                    break;
                case 2:
                    SetHovered(bottomButton);
                    bottomButton.GetComponent<MenuIconBehavior>().hovered = true;
                    break;
                case 3:
                    SetHovered(leftButton);
                    leftButton.GetComponent<MenuIconBehavior>().hovered = true;
                    break;
                default:
                    Debug.LogError("Incorrect hovered button: " + defaultHoveredButton);
                    break;
            }
            
        }

        protected override void SetMainMenuButtons()
        {
            topButton = transform.Find("MainMenu/MenuPanel/TopButton").gameObject;
            rightButton = transform.Find("MainMenu/MenuPanel/RightButton").gameObject;
            bottomButton = transform.Find("MainMenu/MenuPanel/BottomButton").gameObject;
            leftButton = transform.Find("MainMenu/MenuPanel/LeftButton").gameObject;

            topButton.transform.Find("Icon").GetComponent<Image>().sprite = mainMenuIcons[0].defaultIcon;
            rightButton.transform.Find("Icon").GetComponent<Image>().sprite = mainMenuIcons[1].defaultIcon;
            bottomButton.transform.Find("Icon").GetComponent<Image>().sprite = mainMenuIcons[2].defaultIcon;
            leftButton.transform.Find("Icon").GetComponent<Image>().sprite = mainMenuIcons[3].defaultIcon;

            topButton.GetComponent<MenuIconBehavior>().SetCommand(mainMenuIcons[0].commandName, mainMenuIcons[0].defaultIcon, mainMenuIcons[0].hoveredIcon);
            rightButton.GetComponent<MenuIconBehavior>().SetCommand(mainMenuIcons[1].commandName, mainMenuIcons[1].defaultIcon, mainMenuIcons[1].hoveredIcon);
            bottomButton.GetComponent<MenuIconBehavior>().SetCommand(mainMenuIcons[2].commandName, mainMenuIcons[2].defaultIcon, mainMenuIcons[2].hoveredIcon);
            leftButton.GetComponent<MenuIconBehavior>().SetCommand(mainMenuIcons[3].commandName, mainMenuIcons[3].defaultIcon, mainMenuIcons[3].hoveredIcon);

            topButton.GetComponent<MenuIconBehavior>().commandMethod = "AttackButtonPressed";
            rightButton.GetComponent<MenuIconBehavior>().commandMethod = "ItemButtonPressed";
            bottomButton.GetComponent<MenuIconBehavior>().commandMethod = "StayButtonPressed";
            leftButton.GetComponent<MenuIconBehavior>().commandMethod = "MagicButtonPressed";
        }

        public void SetItemMenuButtons()
        {
            // set the 4 icons starting at N and going clockwise to W
            topButton.transform.Find("Icon").GetComponent<Image>().sprite = itemMenuIcons[0].defaultIcon;
            rightButton.transform.Find("Icon").GetComponent<Image>().sprite = itemMenuIcons[1].defaultIcon;
            bottomButton.transform.Find("Icon").GetComponent<Image>().sprite = itemMenuIcons[2].defaultIcon;
            leftButton.transform.Find("Icon").GetComponent<Image>().sprite = itemMenuIcons[3].defaultIcon;

            topButton.GetComponent<MenuIconBehavior>().SetCommand(itemMenuIcons[0].commandName, itemMenuIcons[0].defaultIcon, itemMenuIcons[0].hoveredIcon);
            rightButton.GetComponent<MenuIconBehavior>().SetCommand(itemMenuIcons[1].commandName, itemMenuIcons[1].defaultIcon, itemMenuIcons[1].hoveredIcon);
            bottomButton.GetComponent<MenuIconBehavior>().SetCommand(itemMenuIcons[2].commandName, itemMenuIcons[2].defaultIcon, itemMenuIcons[2].hoveredIcon);
            leftButton.GetComponent<MenuIconBehavior>().SetCommand(itemMenuIcons[3].commandName, itemMenuIcons[3].defaultIcon, itemMenuIcons[3].hoveredIcon);

            topButton.GetComponent<MenuIconBehavior>().commandMethod = "UseItemButtonPressed";
            rightButton.GetComponent<MenuIconBehavior>().commandMethod = "EquipItemButtonPressed";
            bottomButton.GetComponent<MenuIconBehavior>().commandMethod = "DropItemButtonPressed";
            leftButton.GetComponent<MenuIconBehavior>().commandMethod = "GiveItemButtonPressed";
        }
    }
}

