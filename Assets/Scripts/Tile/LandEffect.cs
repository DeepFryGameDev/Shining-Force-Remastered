using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LandTypes
{
    EVENGROUND,
    PATHSBRIDGES,
    SKY,
    OVERGROWTH,
    FOREST,
    MOUNTAIN,
    SAND,
    HIGHMOUNTAIN,
    WATER
}

/*
 * https://sf2.shiningforcecentral.com/guide/land-effect-guide/
 * Movement Cost and Land Effect
“On the map, there are various types of terrain, and each of them has their own movement cost and land effect. Movement cost causes certain character types’ movement range to change. 
When the land effect is high, it increases a character’s defence and therefor lessens the damage taken.”

Translated from TV Land One Pack #141: Shining Force II, a Japanese Shining Force 2 guide book
This can be used to your benefit, as weaker characters or those close to death may be saved by moving them to areas of high land effect. Try to avoid leaving such characters in 
areas of low land effect. Remember then that enemies in the open will be more vulnerable to your attacks than those in high land effect areas.

The percentages below (%) indicate the defence increase that character type receives when positioned on that type of terrain. The numbers above these indicate how the movement 
range is affected (we’ll call them the “movement cost”). A movement range of 0 means that type of character cannot move onto that type of terrain.

It’s hard to explain how the movement costs work, but I’ll try. Basically, the characters movement range is divided by the movement cost for that terrain (see table below) and 
then rounded to the nearest whole number to determine the number of squares that can be moved onto.

Let’s take Chester, for example – he has a movement range of 7. Bear in mind the square a character is standing on is on is “square number 1” of their movement range. 
If Chester is surrounded by even ground (this has a movement cost 1.0 for horse riders and centaurs), he can move up to 6 more squares away to the north, south, east or west, 
and diagonally up to those points too. That’s because his MOV of 7, when divided by the movement cost of 1.0 equals 7 still. If he’s surrounded by forest (movement cost of 2.5), 
his MOV of 7 is divided by 2.5, which equals 2.8 – this is then rounded to the nearest whole number, in this case, 3. Since he’s standing on square 1, he can then move up to 2 squares 
away north, south, east and west, or diagonally imbetween. Mixed terrains will result in oddly shaped movement areas, as each individual square will effect the number of squares 
accessable from that point. See, I told you it’s hard to explain!
*/

namespace DeepFry
{
    public class LandEffect : MonoBehaviour
    {
        public LandTypes landType;
        float defenseMultiplier, movementMultiplier;

        BattleStateMachine bsm;

        public float GetDefenseMultiplier() { return defenseMultiplier; }
        public float GetMovementMultiplier() { return movementMultiplier; }

        // Start is called before the first frame update
        void Start()
        {
            bsm = FindObjectOfType<BattleStateMachine>();

            defenseMultiplier = SetDefenseMultiplier();
        }

        public void SetMultipliers(BaseUnit unit)
        {
            defenseMultiplier = SetDefenseMultiplier();
            movementMultiplier = SetMovementMultiplier(unit);
        }

        float SetDefenseMultiplier()
        {
            switch (landType)
            {
                case LandTypes.EVENGROUND:
                    return .15f;
                case LandTypes.PATHSBRIDGES:
                    return 0f;
                case LandTypes.SKY:
                    return 0f;
                case LandTypes.OVERGROWTH:
                    return .3f;
                case LandTypes.FOREST:
                    return .3f;
                case LandTypes.MOUNTAIN:
                    return .3f;
                case LandTypes.SAND:
                    return 0;
                case LandTypes.HIGHMOUNTAIN:
                    return 0;
                case LandTypes.WATER:
                    return 0;

                default:
                    Debug.LogWarning("LandEffect GetDefenseMultiplier - landType not found, returning 0");
                    return 0;
            }
        }

        float SetMovementMultiplier(BaseUnit unit)
        {
            switch (landType)
            {
                case LandTypes.EVENGROUND:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 1;
                        case unitRaces.CENTAUR:
                            return 1;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
                case LandTypes.PATHSBRIDGES:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 1;
                        case unitRaces.CENTAUR:
                            return 1;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
                case LandTypes.SKY:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 0;
                        case unitRaces.CENTAUR:
                            return 0;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
                case LandTypes.OVERGROWTH:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 1.5f;
                        case unitRaces.CENTAUR:
                            return 1.5f;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
                case LandTypes.FOREST:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 2;
                        case unitRaces.CENTAUR:
                            return 2.5f;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
                case LandTypes.MOUNTAIN:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 1.5f;
                        case unitRaces.CENTAUR:
                            return 2.5f;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
                case LandTypes.SAND:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 1.5f;
                        case unitRaces.CENTAUR:
                            return 2.5f;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
                case LandTypes.HIGHMOUNTAIN:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 0;
                        case unitRaces.CENTAUR:
                            return 0;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
                case LandTypes.WATER:
                    switch (bsm.currentUnit.unitRace)
                    {
                        case unitRaces.HUMAN:
                            return 0;
                        case unitRaces.CENTAUR:
                            return 0;
                        case unitRaces.DEMON:
                            return 1;
                    }
                    break;
            }

            Debug.LogWarning("LandEffect GetMovementMultiplier - landType not found, returning 0");
            return 0;
        }
    }
}

