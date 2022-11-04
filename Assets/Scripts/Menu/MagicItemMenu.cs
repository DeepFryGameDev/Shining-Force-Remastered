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

        // Start is called before the first frame update
        void Start()
        {
            menuPanel = GameObject.Find("[UI]/MagicItemMenu/MenuPanel");

            mp = FindObjectOfType<MagicProcessing>();
            ip = FindObjectOfType<ItemProcessing>();

            bm = FindObjectOfType<BattleMenu>();

            highlightColor = new Color(255, 255, 255, 255);
            transparentColor = new Color(255, 255, 255, 0);
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
                                UsableItemSO uisou = (UsableItemSO)currentBPU.inventory[hoveredButtonVal];
                                ip.ItemChosen(uisou.GetBaseItem(), currentBPU);
                                break;
                            case itemMenuModes.EQUIP:
                                Debug.Log("Equip chosen in menu");
                                break;
                            case itemMenuModes.DROP:
                                Debug.Log("Drop chosen in menu");
                                UsableItemSO uisod = (UsableItemSO)currentBPU.inventory[hoveredButtonVal];
                                ip.RemoveFromInventory(uisod.GetBaseItem(), currentBPU);
                                break;
                            case itemMenuModes.GIVE:
                                Debug.Log("Give chosen in menu");
                                break;
                        }                        
                        break;
                    case MagicItemMenuModes.MAGIC:
                        mp.MagicChosen(currentBPU.learnedMagic[hoveredButtonVal].GetBaseMagic(), currentBPU);
                        break;
                }                
            }

            if (Input.GetKeyDown("w"))
            {
                hoveredButtonVal = 0;
                SetHighlight();
            }

            if (Input.GetKeyDown("d") && currentBPU.inventory.Length >= 2)
            {
                hoveredButtonVal = 1;
                SetHighlight();
            }

            if (Input.GetKeyDown("s") && currentBPU.inventory.Length >= 3)
            {
                hoveredButtonVal = 2;
                SetHighlight();
            }

            if (Input.GetKeyDown("a") && currentBPU.inventory.Length >= 4)
            {
                hoveredButtonVal = 3;
                SetHighlight();
            }

            if (Input.GetKeyDown("c"))
            {
                
            }
        }

        public void SetAndShowUnitMagic(BasePlayerUnit bpu)
        {
            currentBPU = (BasePlayerUnit)bpu;
            Debug.Log("CurrentBPU: " + currentBPU.name);

            // for each magic learned by player unit, fill in menu UI going clockwise(might need to change)

            GameObject.Find("MagicItemMenu/TextBG/MenuText").GetComponent<TMP_Text>().text = bpu.learnedMagic[hoveredButtonVal].name;
            GameObject.Find("MagicItemMenu/MenuPanel/TopButton/Icon").GetComponent<Image>().sprite = bpu.learnedMagic[hoveredButtonVal].icon;

            //bottomButton.GetComponent<MenuIconBehavior>().hovered = true;
            hoveredButtonVal = 0;

            SetHighlight();   

            menuOpen = true;          
        }

        public void SetAndShowUnitItems(BasePlayerUnit bpu)
        {
            currentBPU = (BasePlayerUnit)bpu;
            Debug.Log("CurrentBPU: " + currentBPU.name);

            // for each item in bpu's inventory, fill in menu UI going clockwise(might need to change)

            GameObject.Find("MagicItemMenu/TextBG/MenuText").GetComponent<TMP_Text>().text = bpu.inventory[0].name;
            GameObject.Find("MagicItemMenu/MenuPanel/TopButton/Icon").GetComponent<Image>().sprite = bpu.inventory[0].icon;

            if (bpu.inventory.Length > 1)
            {
                GameObject.Find("MagicItemMenu/MenuPanel/RightButton/Icon").GetComponent<Image>().sprite = bpu.inventory[1].icon;
            }
            
            if (bpu.inventory.Length > 2)
            {
                GameObject.Find("MagicItemMenu/MenuPanel/BottomButton/Icon").GetComponent<Image>().sprite = bpu.inventory[2].icon;
            }
            
            if (bpu.inventory.Length > 3)
            {
                GameObject.Find("MagicItemMenu/MenuPanel/LeftButton/Icon").GetComponent<Image>().sprite = bpu.inventory[3].icon;
            }            

            //bottomButton.GetComponent<MenuIconBehavior>().hovered = true;
            hoveredButtonVal = 0;

            SetHighlight();

            menuOpen = true;
        }

        void SetHighlight()
        {
            switch (hoveredButtonVal)
            {
                case 0:
                    highlightBorder = menuPanel.transform.Find("TopButton/HighlightBorder").GetComponent<Image>();
                    highlightBorder.GetComponent<Image>().color = highlightColor;
                    break;
                case 1:
                    highlightBorder = menuPanel.transform.Find("RightButton/HighlightBorder").GetComponent<Image>();
                    highlightBorder.GetComponent<Image>().color = highlightColor;
                    break;
                case 2:
                    highlightBorder = menuPanel.transform.Find("BottomButton/HighlightBorder").GetComponent<Image>();
                    highlightBorder.GetComponent<Image>().color = highlightColor;
                    break;
                case 3:
                    highlightBorder = menuPanel.transform.Find("LeftButton/HighlightBorder").GetComponent<Image>();
                    highlightBorder.GetComponent<Image>().color = highlightColor;
                    break;
            }
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
    }
}

