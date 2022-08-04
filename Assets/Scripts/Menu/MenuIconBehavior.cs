using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeepFry
{
    public class MenuIconBehavior : MonoBehaviour
    {
        public bool hovered;

        public Sprite defaultIcon, hoveredIcon;

        public string commandMethod;

        bool started, showingDefault;

        float animTime = 0.25f;

        float timeElapsed;

        Image iconImage;

        // Start is called before the first frame update
        void Start()
        {
            iconImage = transform.Find("Icon").GetComponent<Image>();
        }

        // Update is called once per frame
        void Update()
        {
            if (hovered && !started)
            {
                started = true;
                StartCoroutine(AnimateIcon());
            } else
            {
                if (!hovered && started)
                {
                    StopAnimation();
                }
            }
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

        public void SetIcons(Sprite defIcon, Sprite hovIcon)
        {
            defaultIcon = defIcon;
            hoveredIcon = hovIcon;
        }
    }

}
