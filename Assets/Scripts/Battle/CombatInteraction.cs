using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DeepFry
{
    public class CombatInteraction : MonoBehaviour
    {
        public float textDrawSpeed = 0.05f;
        public float messageDelay = 3.0f;
        public bool interactionStarted;

        BaseCombatInteraction combatInteraction;

        BattleStateMachine bsm;

        AudioManager am;

        TileSelection ts;

        public TMP_Text targetNameText, targetClassLevelText, targetHPText, targetMPText;
        public Image HPBar, MPBar;

        public CanvasGroup ciCanvasGroup;

        public GameObject messageTextBG;
        public TMP_Text messageText;

        public CanvasGroup messageCG;
        public CanvasGroup detailsPanelCG;

        bool messageStarted, messageInterrupt;

        MagicProcessing mp;

        MoveCanvas mc;
        BattleMenu bm;

        // Start is called before the first frame update
        void Start()
        {
            ts = FindObjectOfType<TileSelection>();
            mc = FindObjectOfType<MoveCanvas>();
            bm = FindObjectOfType<BattleMenu>();

            bsm = FindObjectOfType<BattleStateMachine>();

            am = FindObjectOfType<AudioManager>();

            mp = FindObjectOfType<MagicProcessing>();
        }

        // Update is called once per frame
        void Update()
        {
            CheckForMessageInterrupt();
        }

        private void CheckForMessageInterrupt()
        {
            if (messageStarted && Input.GetKeyDown("e"))
            {
                Debug.Log("Message interrupted");
                messageInterrupt = true;
            }
        }

        public void SetNewBaseCombatInteraction(CombatInteractionTypes interactionType, BaseUnit primaryUnit, List<BaseUnit> targetUnits, BaseUsableItem item)
        {
            combatInteraction = null;

            BaseCombatInteraction bci = new BaseCombatInteraction
            {
                primaryUnit = primaryUnit,
                targetUnits = targetUnits,
                magicUsed = null,
                itemUsed = item,
                interactionType = interactionType
            };

            combatInteraction = bci;
        }

        public void SetNewBaseCombatInteraction(CombatInteractionTypes interactionType, BaseUnit primaryUnit, List<BaseUnit> targetUnits, BaseMagic magic)
        {
            combatInteraction = null;

            BaseCombatInteraction bci = new BaseCombatInteraction
            {
                primaryUnit = primaryUnit,
                targetUnits = targetUnits,
                magicUsed = magic,
                itemUsed = null,
                interactionType = interactionType
            };

            combatInteraction = bci;
        }

        public void SetNewBaseCombatInteraction(CombatInteractionTypes interactionType, BaseUnit primaryUnit, List<BaseUnit> targetUnits)
        {
            combatInteraction = null;

            BaseCombatInteraction bci = new BaseCombatInteraction
            {
                primaryUnit = primaryUnit,
                targetUnits = targetUnits,
                magicUsed = null,
                itemUsed = null,
                interactionType = interactionType
            };

            combatInteraction = bci;
        }

        public void BuildNewCombatInteraction()
        {
            mc.ToggleMenu(false);

            interactionStarted = true;

            ts.mainCam.transform.position = ts.selectionCam.transform.position;
            ts.mainCam.transform.rotation = ts.selectionCam.transform.rotation;
            ts.mainCam.SetActive(true);
            ts.selectionCam.SetActive(false);

            // workaround for now
            ts.ToggleBrain(false);

            ts.ToggleTileSelectCursor(false);

            combatInteraction.primaryUnit.GetUnitObject().transform.LookAt(combatInteraction.targetUnits[0].GetUnitObject().transform);

            if (combatInteraction.primaryUnit.unitType == unitTypes.PLAYER)
            {
                am.PlayBattleMusic(BattleThemes.PLAYERFIGHT);
            } else
            {
                am.PlayBattleMusic(BattleThemes.ENEMYFIGHT);
            }

            StartCoroutine(RunCombatInteraction());
        }

        IEnumerator RunCombatInteraction()
        {
            int totalEXP = 0, effectValue = 0;

            String message = String.Empty;

            #region Stage 1 - Move camera to primary unit and update unit UI (No need for modularity)

            // zoom camera and show unit details for primary unit --------------------------------------------------------
            UpdateUnitUI(combatInteraction.primaryUnit);

            ShowDetailsPanel(true);

            Debug.Log("1) Moving camera to primary unit");
            StartCoroutine(ts.ZoomToUnit(ts.mainCam, true));

            yield return StartCoroutine(ts.MoveCameraCombat(ts.mainCam, combatInteraction.primaryUnit));

            Debug.Log("Done moving camera");
            #endregion

            #region Stage 2 - Display opening combat message in UI (**Modular**)
            // display first message -------------------------------------------------------------------------------------

            switch (combatInteraction.interactionType)
            {
                case CombatInteractionTypes.ATTACK:
                    message = combatInteraction.primaryUnit.name + "'s attack!";
                    break;
                case CombatInteractionTypes.MAGIC:
                    message = combatInteraction.primaryUnit.name + " casts " + combatInteraction.magicUsed.name + "!";
                    break;
                case CombatInteractionTypes.ITEM:
                    message = combatInteraction.primaryUnit.name + " uses " + combatInteraction.itemUsed.name + "!";
                    break;
            }

            yield return StartCoroutine(DisplayMessage(message, 0)); // yield until message is completed (or button pressed)

            #endregion

            #region Stage 3 - Display opening combat animation (**Modular**)
            // show animation ---------------------------------------------------------------------------------------------

            switch (combatInteraction.interactionType)
            {
                case CombatInteractionTypes.ATTACK:
                    combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().SetTrigger("attack");

                    while (combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    while (combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    break;
                case CombatInteractionTypes.MAGIC:
                    combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().SetBool("isCasting", true);

                    while (combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    break;
                case CombatInteractionTypes.ITEM:
                    combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().SetTrigger("usedItem");

                    while (combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    while (combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("UsedItem"))
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    break;
            }

            ClearMessageText();

            #endregion

            // for each target unit (stages 4-9)
            foreach (BaseUnit unit in combatInteraction.targetUnits)
            {
                bool healed = false, killed = false;

                #region Stage 4 - Calculate effect (**Modular**)
                switch (combatInteraction.interactionType)
                {
                    case CombatInteractionTypes.ATTACK:
                        effectValue = GetAttackDamageAmount(unit);
                        break;
                    case CombatInteractionTypes.MAGIC:
                        switch (combatInteraction.magicUsed.magicType)
                        {
                            case MagicTypes.HEAL:
                                effectValue = GetHealedAmount(unit, combatInteraction.magicUsed.value);
                                break;
                            case MagicTypes.MISC:
                                mp.ExecuteMagic(combatInteraction.magicUsed.name);
                                break;
                        }
                        break;
                    case CombatInteractionTypes.ITEM:
                        switch (combatInteraction.itemUsed.usableItemType)
                        {
                            case UsableItemTypes.HEAL:
                                effectValue = GetHealedAmount(unit, combatInteraction.itemUsed.value);
                                break;
                        }                        
                        break;
                }

                #endregion

                #region Stage 5 - Move camera to target unit and update unit UI (No need for modularity)

                // pan camera and show unit details for target unit -----------------------------------------------------------

                unit.GetUnitObject().transform.LookAt(combatInteraction.primaryUnit.GetUnitObject().transform);

                UpdateUnitUI(unit);

                Debug.Log("2) Moving camera to target unit");
                yield return StartCoroutine(ts.MoveCameraCombat(ts.mainCam, unit));

                #endregion

                #region Stage 6 - Display receiving animation (**Modular**)                

                // show receiving animation ------------------------------------------------------------------------------------

                switch (combatInteraction.interactionType)
                {
                    case CombatInteractionTypes.ATTACK:
                        unit.GetUnitObject().GetComponent<Animator>().SetTrigger("takeDamage");
                        break;
                    case CombatInteractionTypes.MAGIC:
                        // show spell animation
                        StartCoroutine(ShowSpellEffect(combatInteraction.magicUsed, unit.GetUnitObject(), true));

                        break;
                    case CombatInteractionTypes.ITEM:
                        unit.GetUnitObject().GetComponent<Animator>().SetTrigger("receivedItem");

                        while (unit.GetUnitObject().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        {
                            yield return new WaitForEndOfFrame();
                        }

                        while (unit.GetUnitObject().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ReceivedItem"))
                        {
                            yield return new WaitForEndOfFrame();
                        }

                        break;
                }

                #endregion

                #region Stage 7 - Update unit and UI (**Modular**)
                // update units

                switch (combatInteraction.interactionType)
                {
                    case CombatInteractionTypes.ATTACK:
                        UpdateUnitHP(unit, -effectValue);
                        break;
                    case CombatInteractionTypes.MAGIC:
                        switch (combatInteraction.magicUsed.magicType)
                        {
                            case MagicTypes.HEAL:
                                UpdateUnitHP(unit, effectValue);
                                break;
                        }
                        break;
                    case CombatInteractionTypes.ITEM:
                    switch (combatInteraction.itemUsed.usableItemType)
                        {
                            case UsableItemTypes.HEAL:
                                UpdateUnitHP(unit, effectValue);
                                break;
                        }
                        break;
                }

                // update UI
                UpdateUnitUI(unit);

                #endregion

                #region Stage 8 - Display effect message in UI (**Modular**)

                // display effect message ---------------------------------------------------------
                message = String.Empty;

                switch (combatInteraction.interactionType)
                {
                    case CombatInteractionTypes.ATTACK:
                        message = unit.name + " got damaged by " + effectValue + ".";
                        break;
                    case CombatInteractionTypes.MAGIC:
                        switch (combatInteraction.magicUsed.magicType)
                        {
                            case MagicTypes.HEAL:
                                message = unit.name + " recovered " + effectValue + " hit points.";
                                healed = true;
                                break;
                        }
                        break;
                    case CombatInteractionTypes.ITEM:

                        switch (combatInteraction.itemUsed.usableItemType)
                        {
                            case UsableItemTypes.HEAL:
                                message = unit.name + "'s HP is restored by: " + effectValue + "!";
                                healed = true;
                                break;
                        }
                        break;
                }

                yield return StartCoroutine(DisplayMessage(message, messageDelay)); // yield until message is completed (or button pressed)

                #endregion

                #region Stage 9 - Check for death

                if (unit.HP <= 0)
                {
                    yield return ProcessDeath(unit);
                    killed = true;
                }                

                #endregion

                #region Stage 10 - Increase temporary EXP value (No need for modularity)

                if (combatInteraction.primaryUnit.unitType == unitTypes.PLAYER)
                {
                    totalEXP += GetEXP(combatInteraction.primaryUnit, unit, effectValue, healed, killed);
                }                

                #endregion
            }

            #region Stage 11 - Move camera back to primary unit and update unit UI (No need for modularity)
            // zoom camera and show unit details for primary unit --------------------------------------------------------
            UpdateUnitUI(combatInteraction.primaryUnit);

            Debug.Log("3) Moving camera back to primary unit");
            StartCoroutine(ts.MoveCameraCombat(ts.mainCam, combatInteraction.primaryUnit));

            // set animations back to idle
            switch (combatInteraction.interactionType)
            {
                case CombatInteractionTypes.MAGIC:
                    combatInteraction.primaryUnit.GetUnitObject().GetComponent<Animator>().SetBool("isCasting", false);
                    break;
            }            

            #endregion

            #region Stage 12 - Display EXP gain message (No need for modularity)
            if (combatInteraction.primaryUnit.unitType == unitTypes.PLAYER)
            {
                // display exp gains -------------------------------------------------------------------
                message = String.Empty;

                message = combatInteraction.primaryUnit.name + " gained " + totalEXP + " EXP.";

                yield return StartCoroutine(DisplayMessage(message, messageDelay)); // yield until message is completed (or button pressed)
                #endregion

                #region Stage 13 - Process EXP (No need for modularity)
                // get base player and increase exp
                StartCoroutine(ProcessExp(totalEXP));

                #endregion

                #region Stage 14 - Fade back into gameplay (No need for modularity)

                // fade back into gameplay     
                yield return StartCoroutine(ts.ZoomToUnit(ts.mainCam, false));
            }

            #endregion

            #region Stage 15 - Clean up (No need for modularity)

            CleanUpCombatInteraction();

            #endregion
        }

        private IEnumerator ProcessExp(int totalEXP)
        {
            BasePlayerUnit bpu = combatInteraction.primaryUnit.GetUnitObject().GetComponent<PlayerUnitObject>().playerUnit;
            bpu.GainEXP(totalEXP);

            if (bpu.exp >= 100) // magic number, will need to adjust
            {
                // leveled up
                bpu.Levelup();

                string message = combatInteraction.primaryUnit.name + " became level " + bpu.level + "!";
                yield return StartCoroutine(DisplayMessage(message, messageDelay)); // yield until message is completed (or button pressed)
            }
        }

        private IEnumerator ProcessDeath(BaseUnit unit)
        {
            // show death animation
            unit.GetUnitObject().GetComponent<Animator>().SetTrigger("death");
            // remove from queue
            bsm.RemoveFromTurnQueue(unit);

            if (unit.unitType == unitTypes.PLAYER)
            {
                unit.GetUnitObject().GetComponent<PlayerUnitObject>().playerUnit.dead = true;
            }
            else
            {
                // remove from active units
                bsm.activeUnits.Remove(unit);
            }

            // destroy unit
            StartCoroutine(DestroyUnitObject(unit, 3));
            yield return StartCoroutine(DisplayMessage(unit.name + " was defeated!", messageDelay));
        }

        IEnumerator DestroyUnitObject(BaseUnit unit, float waitTime)
        {            
            yield return new WaitForSeconds(waitTime);
            Destroy(unit.GetUnitObject());            
            Debug.Log(unit.name + " removed from battle.");
        }

        private int GetAttackDamageAmount(BaseUnit targetUnit)
        {
            // get attacker's attack + equipment power
            int tempAttack = 0, tempDefense = 0, tempTotal = 0, tempAfterHP = 0;

            if (targetUnit.unitType == unitTypes.PLAYER)
            {
                BasePlayerUnit tempBPU = (BasePlayerUnit)targetUnit;

                // get attacker's attack + equipment power
                tempAttack = tempBPU.attack + tempBPU.GetEquippedWeapon().attack;

                // get target's defense
                tempDefense = tempBPU.defense; // will probably be updated
            }
            else if (targetUnit.unitType == unitTypes.ENEMY)
            {
                // get attacker's attack + equipment power
                tempAttack = targetUnit.attack;

                // get target's defense
                tempDefense = targetUnit.defense;
            }

            // get value first one - second one
            tempTotal = tempAttack - tempDefense;

            // Apply land effect. (still needs to be added)

            // Subtract a random amount between 0 % and 25 %
            int tempRandom = GetRandomBetweenRange(0, 25);
            tempTotal = Mathf.RoundToInt(tempTotal * (tempRandom * .1f));

            // subtract all of this from target's HP.
            tempAfterHP = targetUnit.HP - tempTotal;

            if (tempAfterHP < 0)
            {
                return targetUnit.HP;
            } else
            {
                return tempAfterHP;
            }
        }

        IEnumerator ShowSpellEffect(BaseMagic magic, GameObject target, bool playAudio)
        {
            // instantiate spell effect
            GameObject newSpellEffect = Instantiate(magic.effectPrefab, target.transform.position, Quaternion.identity, GameObject.Find("[Effects]").transform);

            if (playAudio)
                am.PlayEffect(magic.effectAudio);

            // while spell effect is still going
            yield return new WaitForSeconds(magic.effectPrefab.GetComponent<MagicEffect>().lifetime);

            // then destroy it
            Destroy(newSpellEffect);
        }

        private void UpdateUnitHP(BaseUnit unit, int value)
        {
            unit.HP += value;

            if (unit.HP > unit.maxHP) unit.HP = unit.maxHP;
        }

        int GetHealedAmount(BaseUnit targetUnit, int healAmount)
        {
            int temp = 0;
            int tempHP = targetUnit.HP;

            for (int i = 0; i < healAmount; i++)
            {
                if (tempHP < targetUnit.maxHP)
                {
                    tempHP += 1;
                    temp++;
                } else
                {
                    return temp;
                }
            }

            return temp;
        }

        int GetEXP(BaseUnit primaryUnit, BaseUnit targetUnit, int effectValue, bool healed, bool killed)
        {
            float expReceived = 0;
            int levelDiff = targetUnit.level - primaryUnit.level;

            if (killed) // Experience for killing
            {
                if (levelDiff > 0) // (E-lev - C-lev) > 0 -> Range(46,50)
                {
                    expReceived = GetRandomBetweenRange(46, 50);
                }
                else if (levelDiff == 0) // (E-lev - C-lev) = 0 -> Range(30,40)
                {
                    expReceived = GetRandomBetweenRange(30, 40);
                }
                else if (levelDiff == -1) // (E-lev - C-lev) = -1 -> Range(15,25)
                {
                    expReceived = GetRandomBetweenRange(15, 25);
                }
                else if (levelDiff < -1) // (E-lev - C-lev) < -1 -> Range(1,3)
                {
                    expReceived = GetRandomBetweenRange(1, 3);
                }
            }

            if (healed)
            {
                //verify if healing class. if not, 1-2. if so, continue
                int mod = 1;

                if (levelDiff > 0) // (E-lev - C-lev) > 0
                {
                    // no changes. yet.
                }
                else if (levelDiff == 0) // (E-lev - C-lev) = 0
                {
                    mod = 2;
                }
                else if (levelDiff == -1) // (E-lev - C-lev) = -1
                {
                    mod = 4;
                }
                else if (levelDiff < -1) // (E-lev - C-lev) < -1
                {
                    mod = GetRandomBetweenRange(1, 2);
                }

                float calc = (float)effectValue / (float)targetUnit.maxHP;

                expReceived = 50 * calc / mod;

                if (expReceived < 10)
                {
                    expReceived = 10;
                }

            }
            else if (effectValue > 0) // damage was received
            {
                expReceived += GetRandomBetweenRange(46, 50);

                if (levelDiff > 0) // (E-lev - C-lev) > 0
                {
                    // no changes. yet.
                }
                else if (levelDiff == 0) // (E-lev - C-lev) = 0
                {
                    expReceived /= 2;
                }
                else if (levelDiff == -1) // (E-lev - C-lev) = -1
                {
                    expReceived /= 4;
                }
                else if (levelDiff < -1) // (E-lev - C-lev) < -1
                {
                    expReceived = GetRandomBetweenRange(1, 2);
                }

                expReceived = expReceived / targetUnit.maxHP * effectValue;
            }

            int expRounded = Mathf.FloorToInt(expReceived);
            if (expRounded <= 0) expRounded = 1;


            if (expRounded > 49)
            {
                expRounded = 49;
            }

            return expRounded;
        }

        public int GetRandomBetweenRange(int min, int max)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            return Random.Range(min, max + 1);
        }

        public IEnumerator DisplayMessage(string message, float timeToWait)
        {
            Debug.Log("~-~-~-~-~-~ Message: " + message + " ~-~-~-~-~-~");

            if (messageCG.alpha == 0)
            {
                messageCG.alpha = 1;
            }

            int index = 0;
            string temp = string.Empty;

            messageStarted = true;

            while (index < message.Length)
            {
                while (message[index] == ' ')
                    ++index;

                ++index;

                temp = message.Substring(0, index);

                messageText.text = temp;

                if (messageInterrupt)
                {
                    messageInterrupt = false;
                    messageText.text = message;
                    break;
                }

                yield return new WaitForSeconds(0.05f);
            }

            messageStarted = false;

            if (timeToWait > 0)
            {
                float tempTime = 0;
                messageStarted = true;

                while (tempTime < timeToWait)
                {
                    if (messageInterrupt)
                    {
                        messageStarted = false;
                        messageInterrupt = false;
                        break;
                    }

                    tempTime += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                messageStarted = false;
            }
        }

        public void ClearMessageText()
        {
            if (messageCG.alpha == 1)
            {
                messageCG.alpha = 0;
                messageText.text = String.Empty;
            }
        }

        void ShowDetailsPanel(bool show)
        {
            if (show)
            {
                detailsPanelCG.alpha = 1;
            } else
            {
                detailsPanelCG.alpha = 0;
            }
        }

        void UpdateUnitUI(BaseUnit unit)
        {
            // set unit details
            targetNameText.text = unit.name;
            targetHPText.text = unit.HP + " / " + unit.maxHP;
            targetMPText.text = unit.MP + "/ " + unit.maxMP;

            HPBar.fillAmount = unit.HP / unit.maxHP;
            if (unit.maxMP == 0)
            {
                MPBar.fillAmount = 0;
            } else
            {
                MPBar.fillAmount = unit.MP / unit.maxMP;
            }            

            if (unit.unitType == unitTypes.PLAYER)
            {
                BasePlayerUnit tempBPU = (BasePlayerUnit)unit;
                targetClassLevelText.text = tempBPU.unitClass.ToString() + " " + tempBPU.level.ToString();
            } else if (unit.unitType == unitTypes.ENEMY)
            {
                targetClassLevelText.text = "";
            }

            // show ui if not already
            if (ciCanvasGroup.alpha == 0)
            {
                ciCanvasGroup.alpha = 1;
            }

            // move camera to primary unit and zoom into it
        }

        void CleanUpCombatInteraction()
        {
            // hide details panel
            ShowDetailsPanel(false);

            // hide combat interaction UI
            ClearMessageText();
            ts.ToggleBrain(true);

            ts.gameCam.SetActive(true);

            am.PlayBattleMusic(BattleThemes.BATTLE);

            Debug.Log("Go to next turn.");

            interactionStarted = false;
            bm.EndTurn();
        }
    }
}
