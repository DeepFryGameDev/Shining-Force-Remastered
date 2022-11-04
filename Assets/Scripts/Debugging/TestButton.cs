using DeepFry;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TestButton : MonoBehaviour
{
    public GameObject mainCam;
    public GameObject selectionCam;
    BattleStateMachine bsm;
    TileSelection ts;
    TileTargetProcessing ttp;

    public GameObject messageTextBG;
    public TMP_Text messageText;

    public MagicSO testMagic;

    AudioManager am;

    bool breaking, started;

    BaseTileTarget btt;

    // Start is called before the first frame update
    void Start()
    {
        ts = FindObjectOfType<TileSelection>();
        bsm = FindObjectOfType<BattleStateMachine>();
        ttp = FindObjectOfType<TileTargetProcessing>();

        am = FindObjectOfType<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DoTestThing()
    {
        Test();
    }

    void Test()
    {
        PrintTilesFromTarget();
    }

    void PrintTilesFromTarget() 
    {
        Tile homeTile = bsm.currentUnit.GetTile();

        Tile targetTile = null;

        foreach (BaseUnit unit in bsm.activeUnits)
        {
            if (unit.unitType == unitTypes.ENEMY)
            {
                targetTile = unit.GetTile();
            }
        }

        Debug.Log(bsm.currentUnit.name + " tile: " + homeTile.name);
        Debug.Log("targetTile: " + targetTile.name);
        Debug.Log("-~-~-~-");

        Debug.Log("Tiles from " + homeTile.name + " to " + targetTile.name + ": " + bsm.currentUnit.GetUnitObject().GetComponent<TacticsMove>().GetNumberOfTilesToTarget(targetTile));

        PrintUnitOnTile(homeTile);
        PrintUnitOnTile(targetTile);
    }

    void PrintUnitOnTile(Tile tile)
    {
        Debug.Log("PrintUnitOnTile: " + tile.GetUnitOnTile().name);
    }

    /*
    IEnumerator ShowSpellEffect()
    {
        Debug.Log(testMagic.effectPrefab.GetComponent<MagicEffect>().lifetime);

        // instantiate spell effect
        GameObject newSpellEffect = Instantiate(testMagic.effectPrefab, new Vector3(0, 0, 0), Quaternion.identity, GameObject.Find("[Effects]").transform);

        // while spell effect is still going
        yield return new WaitForSeconds(testMagic.effectPrefab.GetComponent<MagicEffect>().lifetime);

        // then destroy it
        Destroy(newSpellEffect);
    }

    // GetEXP(BaseUnit primaryUnit, BaseUnit targetUnit, int damageValue, bool killed, bool healed)
    int GetEXP(int primaryUnitLevel, int targetUnitLevel, int targetUnitMaxHP, int healDamVal, bool healed, bool killed)
    {
        float expReceived = 0;
        int levelDiff = targetUnitLevel - primaryUnitLevel;

        if (killed) // Experience for killing
        {          
            if (levelDiff > 0) // (E-lev - C-lev) > 0 -> Range(46,50)
            {
                expReceived = GetRandomBetweenRange(46, 50);
            } else if (levelDiff == 0) // (E-lev - C-lev) = 0 -> Range(30,40)
            {
                expReceived = GetRandomBetweenRange(30, 40);
            } else if (levelDiff == -1) // (E-lev - C-lev) = -1 -> Range(15,25)
            {
                expReceived = GetRandomBetweenRange(15, 25);
            } else if (levelDiff < -1) // (E-lev - C-lev) < -1 -> Range(1,3)
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

            float calc = (float)healDamVal / (float)targetUnitMaxHP;

            expReceived = 50 * calc / mod;

            if (expReceived < 10)
            {
                expReceived = 10;
            }

        } else if (healDamVal > 0) // damage was received
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

            expReceived = expReceived / targetUnitMaxHP * healDamVal;
        }

        int expRounded = Mathf.FloorToInt(expReceived);
        if (expRounded <= 0) expRounded = 1;


        if (expRounded > 49) {
            expRounded = 49;
        }

        return expRounded;
    }

    int GetRandomBetweenRange(int min, int max)
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        return Random.Range(min, max+1);
    }

    IEnumerator DisplayMessage(string message, float timeToWait)
    {
        if (!messageTextBG.activeInHierarchy)
        {
            messageTextBG.SetActive(true);
            messageTextBG.transform.parent.GetComponent<CanvasGroup>().alpha = 1;
        }

        int index = 0;
        string temp = string.Empty;

        started = true;
        while (index < message.Length)
        {
            while (message[index] == ' ')
                ++index;

            ++index;

            temp = message.Substring(0, index);

            messageText.text = temp;

            if (breaking)
            {
                messageText.text = message;
                breaking = false;
                break;
            }

            yield return new WaitForSeconds(0.05f);
        }
        started = false;
        

        if (timeToWait > 0)
        {
            float tempTime = 0;
            started = true;

            while (tempTime < timeToWait) 
            {
                if (breaking)
                {
                    breaking = false;
                    break;
                }

                tempTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            started = false;
        }      
    }*/
}
