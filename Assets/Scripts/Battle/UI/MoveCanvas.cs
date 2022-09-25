using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem.LowLevel;

namespace DeepFry
{
    public class MoveCanvas : MonoBehaviour
    {
        CanvasGroup cg;
        TMP_Text landEffectText, nameText, classLevelText, hpText, mpText;
        Image hpSlider, mpSlider;

        public bool canvasDrawn;

        // Start is called before the first frame update
        void Start()
        {
            canvasDrawn = false;

            cg = GetComponent<CanvasGroup>();

            landEffectText = transform.Find("LandEffectPanel/LandEffectVal").GetComponent<TMP_Text>();
            nameText = transform.Find("UnitDetailsPanel/NameText").GetComponent<TMP_Text>();
            classLevelText = transform.Find("UnitDetailsPanel/ClassLevelText").GetComponent<TMP_Text>();
            hpText = transform.Find("UnitDetailsPanel/HPText").GetComponent<TMP_Text>();
            mpText = transform.Find("UnitDetailsPanel/MPText").GetComponent<TMP_Text>();

            hpSlider = transform.Find("UnitDetailsPanel/HPBar/Fill Area/Fill").GetComponent<Image>();
            mpSlider = transform.Find("UnitDetailsPanel/MPBar/Fill Area/Fill").GetComponent<Image>();
        }

        public void DrawCanvas(BasePlayerUnit unit, Tile tile)
        {
            landEffectText.text = (tile.GetComponent<LandEffect>().GetDefenseMultiplier() * 100) + "%";

            nameText.text = unit.name;
            classLevelText.text = unit.unitClass.ToString() + " " + unit.level.ToString();
            hpText.text = unit.HP + " / " + unit.maxHP;
            mpText.text = unit.MP + " / " + unit.maxMP;

            hpSlider.fillAmount = unit.HP / unit.maxHP;
            mpSlider.fillAmount = unit.MP / unit.maxMP;
            
            cg.alpha = 1;
            
            canvasDrawn = true;
        }

        public void UpdateLandEffect(Tile tile)
        {
            landEffectText.text = (tile.GetComponent<LandEffect>().GetDefenseMultiplier() * 100) + "%";
        }

        public void ToggleMenu(bool open)
        {
            GetComponent<Animator>().SetBool("menuOpened", open);
        }
    }
}

