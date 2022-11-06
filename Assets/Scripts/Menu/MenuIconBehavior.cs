using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepFry
{
    public class MenuIconBehavior : MonoBehaviour
    {
        public bool hovered;

        public Sprite defaultIcon, hoveredIcon;

        public string commandMethod;

        BaseItem item;
        BaseMagic magic;

        bool started, showingDefault;

        float animTime = 0.25f;

        float timeElapsed;

        Image iconImage;

        TMP_Text commandNameText;
        string commandName;

        // Start is called before the first frame update
        void Start()
        {
            iconImage = transform.Find("Icon").GetComponent<Image>();

            commandNameText = GameObject.Find("MainMenu/TextBG/MenuText").GetComponent<TMP_Text>();
        }

        // Update is called once per frame
        void Update()
        {
            if (hovered && !started)
            {
                started = true;

                commandNameText.text = commandName;

                StartCoroutine(AnimateIcon());
            } else
            {
                if (!hovered && started)
                {
                    StopAnimation();
                }
            }
        }

        public void SetItem(BaseItem itemToSet)
        {
            magic = null;
            item = itemToSet;
        }

        public void SetMagic(BaseMagic magicToSet)
        {
            magic = magicToSet;
            item = null;
        }

        public void ResetButton()
        {
            item = null;
            magic = null;
        }

        public BaseItem GetItem()
        {
            return item;
        }

        public BaseMagic GetMagic()
        {
            return magic;
        }

        IEnumerator AnimateIcon()
        {
            SwitchIcon();

            while (hovered)
            {
                yield return new WaitForEndOfFrame();
                timeElapsed += Time.deltaTime;

                if (timeElapsed >= animTime)
                {
                    timeElapsed = 0;
                    SwitchIcon();
                }               
            }            
        }

        private void SwitchIcon()
        {
            if (showingDefault)
            {
                iconImage.sprite = hoveredIcon;
            }
            else
            {
                iconImage.sprite = defaultIcon;
            }

            showingDefault = !showingDefault;
        }

        void StopAnimation()
        {
            started = false;
            iconImage.sprite = defaultIcon;
            timeElapsed = 0;
        }

        public void SetCommand(string cmdName, Sprite defIcon, Sprite hovIcon)
        {
            commandName = cmdName;
            defaultIcon = defIcon;
            hoveredIcon = hovIcon;
        }
    }

}
