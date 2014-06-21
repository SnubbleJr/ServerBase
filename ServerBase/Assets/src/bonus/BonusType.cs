using UnityEngine;
using System.Collections;

/**
 * Enum that contains all types of boni.
 * Those can be stacked via BonusManager
 */

public enum BonusType
{
    SPEED, //speed bonus for the ships
    POINTS, //Collect to get points
    POINT_MULTIPLY, //additional base points added to collected items
    REGEN_HEALTH, //Regenerate health instantly to speed up again
    SHIELD, //A shield to prevent other players from slowing you down when shooting at you
    COLLECT_ALL //enables the collector to collect any items regardless of the currently active one
}