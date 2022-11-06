using DeepFry;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MagicItemMenuModes
{
    IDLE,
    MAGIC,
    ITEM
}

namespace DeepFry
{
    public class MagicItemMenu : MonoBehaviour
    {
        [Range(0, .25f)]
        public float hoveredBlinkTime = .15f;

        int hoveredButtonVal;
        public bool menuOpen;

        public MagicItemMenuModes menuMode;

        GameObject menuPanel;
        Image highlightBorder;

        Color highlightColor, transparentColor;
        float timeElapsed;        
        bool highlightOn;

        MagicProcessing mp;
        ItemProcessing ip;

        BattleMenu bm;

        BasePlayerUnit currentBPU;

        public Sprite blankIcon;

        Image topIcon, rightIcon, bottomIcon, leftIcon;
        Image topBorder, rightBorder, bottomBorder, leftBorder;
        MenuIconBehavior topMIB, rightMIB, bottomMIB, leftMIB;

        TMP_Text menuText;

        // Start is called before the first frame update
        void Start()
        {
            menuPanel = GameObject.Find("[UI]/MagicItemMenu/MenuPanel");

            mp = FindObjectOfType<MagicProcessing>();
            ip = FindObjectOfType<ItemProcessing>();

            bm = FindObjectOfType<BattleMenu>();

            highlightColor = new Color(255, 255, 255, 255);
            transparentColor = new Color(255, 255, 255, 0);

            // set button stuff
            topIcon = menuPanel.transform.Find("TopButton/Icon").GetComponent<Image>();
            rightIcon = menuPanel.transform.Find("RightButton/Icon").GetComponent<Image>();
            bottomIcon = menuPanel.transform.Find("BottomButton/Icon").GetComponent<Image>();
            leftIcon = menuPanel.transform.Find("LeftButton/Icon").GetComponent<Image>();

            topBorder = menuPanel.transform.Find("TopButton/HighlightBorder").GetComponent<Image>();
            rightBorder = menuPanel.transform.Find("RightButton/HighlightBorder").GetComponent<Image>();
            bottomBorder = menuPanel.transform.Find("BottomButton/HighlightBorder").GetComponent<Image>();
            leftBorder = menuPanel.transform.Find("LeftButton/HighlightBorder").GetComponent<Image>();

            topMIB = menuPanel.transform.Find("TopButton").GetComponent<MenuIconBehavior>();
            rightMIB = menuPanel.transform.Find("RightButton").GetComponent<MenuIconBehavior>();
            bottomMIB = menuPanel.transform.Find("BottomButton").GetComponent<MenuIconBehavior>();
            leftMIB = menuPanel.transform.Find("LeftButton").GetComponent<MenuIconBehavior>();

            // set ui stuff
            menuText = GameObject.Find("MagicItemMenu/TextBG/MenuText").GetComponent<TMP_Text>();
        }

        // Update is called once per frame
        void Update()
        {
            if (menuOpen)
            {
                ProcessHighlight();

                GetInput();
            }
        }

        private void GetInput()
        {
            if (Input.GetKeyDown("e"))
            {
                switch (menuMode)
                {
                    case MagicItemMenuModes.ITEM:
                        switch (bm.itemMenuMode)
                        {
                            case itemMenuModes.USE:
                                BaseUsableItem item = currentBPU.inventory[hoveredButtonVal].GetBaseUsableItem();
                                ip.PrepareItemTileSelection(item, currentBPU);
                                break;
                            case itemMenuModes.EQUIP:
                                Debug.Log("Equip chosen in menu");
                                HideMenu();
                                switch (hoveredButtonVal)
                                {
                                    case 0:
                                        StartCoroutine(ip.EquipItem(currentBPU, topMIB.GetItem()));
                                        break;
                                    case 1:
                                        StartCoroutine(ip.EquipItem(currentBPU, rightMIB.GetItem()));
                                        break;
                                    case 2:
                                        StartCoroutine(ip.EquipItem(currentBPU, bottomMIB.GetItem()));
                                        break;
                                    case 3:
                                        StartCoroutine(ip.EquipItem(currentBPU, leftMIB.GetItem()));
                                        break;
                                }
                                
                                break;
                            case itemMenuModes.DROP:
                                Debug.Log("Drop chosen in menu");
                                HideMenu();
                                StartCoroutine(ip.DropItem(currentBPU, currentBPU.inventory[hoveredButtonVal]));
                                break;
                            case itemMenuModes.GIVE:
                                Debug.Log("Give chosen in menu");
                                HideMenu();
                                if (currentBPU.inventory[hoveredButtonVal].itemType == ItemTypes.USABLE)
                                {
                                    BaseUsableItem giveUsableItem = currentBPU.inventory[hoveredButtonVal].GetBaseUsableItem();
                                    ip.PrepareItemTileSelection(giveUsableItem, currentBPU);
                                }
                                if (currentBPU.inventory[hoveredButtonVal].itemType == ItemTypes.EQUIPMENT)
                                {
                                    BaseEquipment giveEquip = null;

                                    switch (hoveredButtonVal)
                                    {
                                        case 0:
                                            giveEquip = DB.GameDB.GetEquipmentItem(topMIB.GetItem().ID);
                                            break;
                                        case 1:
                                            giveEquip = DB.GameDB.GetEquipmentItem(rightMIB.GetItem().ID);
                                            break;
                                        case 2:
                                            giveEquip = DB.GameDB.GetEquipmentItem(bottomMIB.GetItem().ID);
                                            break;
                                        case 3:
                                            giveEquip = DB.GameDB.GetEquipmentItem(leftMIB.GetItem().ID);
                                            break;
                                    }

                                    ip.PrepareItemTileSelection(giveEquip, currentBPU);
                                }

                                break;
                        }                        
                        break;
                    case MagicItemMenuModes.MAGIC:
                        mp.MagicChosen(currentBPU.learnedMagicSOs[hoveredButtonVal].GetBaseMagic(), currentBPU);
                        break;
                }                
            }

            if (Input.GetKeyDown("w"))
            {
                hoveredButtonVal = 0;
                SetHighlight();
            }

            if (Input.GetKeyDown("d") && (rightMIB.GetItem() != null || rightMIB.GetMagic() != null))
            {
                hoveredButtonVal = 1;
                SetHighlight();
            }

            if (Input.GetKeyDown("s") && (bottomMIB.GetItem() != null || bottomMIB.GetMagic() != null))
            {
                hoveredButtonVal = 2;
                SetHighlight();
            }

            if (Input.GetKeyDown("a") && (leftMIB.GetItem() != null || leftMIB.GetMagic() != null))
            {
                hoveredButtonVal = 3;
                SetHighlight();
            }

            if (Input.GetKeyDown("c"))
            {
                HideMenu();
            }
        }

        public void SetAndShowUnitMagic(BasePlayerUnit bpu)
        {
            currentBPU = (BasePlayerUnit)bpu;
            Debug.Log("CurrentBPU: " + currentBPU.name);

            // for each magic learned by player unit, fill in menu UI going clockwise(might need to change)

            menuText.text = bpu.learnedMagicSOs[hoveredButtonVal].name;
            topIcon.sprite = bpu.learnedMagicSOs[hoveredButtonVal].icon;

            //bottomButton.GetComponent<MenuIconBehavior>().hovered = true;
            hoveredButtonVal = 0;

            SetHighlight();   

            menuOpen = true;          
        }

        public void SetAndShowUnitItems(BasePlayerUnit bpu)
        {
            ResetItemButtons();

            currentBPU = (BasePlayerUnit)bpu;
            Debug.Log("CurrentBPU: " + currentBPU.name);

            // for each item in bpu's inventory, fill in menu UI going clockwise(might need to change)
            int index = 0;

            foreach (BaseItem item in bpu.inventory)
            {
                switch (index)
                {
                    case 0:
                        topIcon.sprite = item.icon;
                        topMIB.SetItem(item);
                        break;
                    case 1:
                        rightIcon.sprite = item.icon;
                        rightMIB.SetItem(item);
                        break;
                    case 2:
                        bottomIcon.sprite = item.icon;
                        bottomMIB.SetItem(item);
                        break;
                    case 3:
                        leftIcon.sprite = item.icon;
                        leftMIB.SetItem(item);
                        break;
                }
                index++;
            } 

            hoveredButtonVal = 0;

            SetHighlight();

            menuOpen = true;
        }

        public void SetAndShowUnitEquipment(BasePlayerUnit bpu)
        {
            ResetItemButtons();

            currentBPU = (BasePlayerUnit)bpu;

            // for each item in bpu's inventory, fill in menu UI going clockwise(might need to change)
            int index = 0;

            foreach (BaseItem item in bpu.inventory)
            {
                if (item.itemType == ItemTypes.EQUIPMENT)
                {
                    switch (index)
                    {
                        case 0:
                            topIcon.sprite = item.icon;
                            topMIB.SetItem(item);
                            break;
                        case 1:
                            rightIcon.sprite = item.icon;
                            rightMIB.SetItem(item);
                            break;
                        case 2:
                            bottomIcon.sprite = item.icon;
                            bottomMIB.SetItem(item);
                            break;
                        case 3:
                            leftIcon.sprite = item.icon;
                            leftMIB.SetItem(item);
                            break;
                    }
                    index++;
                }
            }

            //bottomButton.GetComponent<MenuIconBehavior>().hovered = true;
            hoveredButtonVal = 0;

            SetHighlight();

            menuOpen = true;
        }

        void ResetItemButtons()
        {
            topIcon.sprite = blankIcon;
            topMIB.ResetButton();

            rightIcon.sprite = blankIcon;
            rightMIB.ResetButton();

            bottomIcon.sprite = blankIcon;
            bottomMIB.ResetButton();

            leftIcon.sprite = blankIcon;
            leftMIB.ResetButton();
        }

        void SetHighlight()
        {
            switch (hoveredButtonVal)
            {
                case 0:
                    highlightBorder = topBorder;                    
                    menuText.text = topMIB.GetItem().name;
                    break;
                case 1:
                    highlightBorder = rightBorder;
                    menuText.text = rightMIB.GetItem().name;
                    break;
                case 2:
                    highlightBorder = bottomBorder;
                    menuText.text = bottomMIB.GetItem().name;
                    break;
                case 3:
                    highlightBorder = leftBorder;
                    menuText.text = leftMIB.GetItem().name;
                    break;
            }

            highlightBorder.color = highlightColor;
        }

        void ProcessHighlight()
        {
            if (timeElapsed <= hoveredBlinkTime)
            {
                timeElapsed += Time.deltaTime;
            }
            else 
            {
                timeElapsed = 0;
                highlightOn = !highlightOn;
            }

            if (highlightOn)
            {
                highlightBorder.color = highlightColor;
            } else
            {
                highlightBorder.color = transparentColor;
            }
        }

        public void HideMenu()
        {
            //Debug.Log("Hiding menu");
            bm.HideMenu(gameObject);

            menuOpen = false;
        }

        public void ShowMenu()
        {
            bm.ShowMenu(gameObject, 0);
            menuOpen = true;
        }
    }
}

