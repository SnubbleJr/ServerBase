using UnityEngine;
using System.Collections;

/**
    * The basic bonus class used for managing boni as a whole
    */

public class Bonus
{

    public BonusType type;
    public float expireTime;
    public string guid;
    public float value;
    public bool isExpirable = true;

    /**
        * Create a new Bonus Object with the given values
        * @param	type the type of this bonus
        * @param	ttl time for this bonus to be active
        * @param	minValue minimal bonus value
        * @param	maxValue maximum bonus value
        */
    public Bonus(BonusType type, float ttl, float minValue, float maxValue)
    {
        this.type = type;
        this.expireTime = Time.time + ttl;
        this.value = UnityEngine.Random.Range(minValue, maxValue);
        this.guid = "b_" + type + "_" + expireTime + "_" + value;
    }

    /**
        * Copy constructor used for deserialization purposes
        * @param	type the type
        * @param	value the value used
        * @param	expireTime the expire time set
        * @param	id the guid
        */
    public Bonus(BonusType type, float value, float expireTime, string id)
    {
        this.type = type;
        this.expireTime = expireTime;
        this.value = value;
        this.guid = id;
    }

    /**
        * Adds the bonus value of this bonus to the given score value
        * @param	score
        * @return
        */
    public float process(float score)
    {
        return score + value;
    }

    /**
        * Get the type of this bonus.
        * @return BonusType 
        */
    public BonusType getType()
    {
        return type;
    }

    /**
        * Get this Bonus* unique identifier name.
        * Used to identify the bonus throughout the network
        * @return
        */
    public string getGuid()
    {
        return guid;
    }

    /**
        * Get the actual value this bonus adds
        * @return
        */
    public float getValue()
    {
        return value;
    }

    /**
        * Check if the bonus has expired (if expirable).
        * Returns true is bonus is expired, false otherwise
        * @return
        */
    public bool isExpired()
    {
        return isExpirable && (Time.time > expireTime);
    }

    /**
        * Return a string presentation of this Bonus object,
        * this is mainly used for serializing and sending over the network
        * @return
        */
    public string toString()
    {

        return type.ToString() + "," + value + "," + expireTime + "," + guid;
    }

    /**
        * Deserializes a bonus object string that has been created with Bonus.toString()
        * @param	input
        * @return Bonus
        */
    public static Bonus deserialize(string input)
    {
        string[] split = input.Split(","[0]);
        if (split.Length < 4)
        {
            throw new InconsistencyException("Received invalud Bonus data! Expected lenght is 4, but was " + split.Length);
        }
        //The split should be: TYPE, value, expireTime, guid so here goes
        BonusType type;
        float value;
        float expireTime;
        string guid;
        switch (split[0])
        {
            case "SPEED":
                type = BonusType.SPEED;
                break;
            case "POINTS":
                type = BonusType.POINTS;
                break;
            case "POINT_MULTIPLY":
                type = BonusType.POINT_MULTIPLY;
                break;
            case "REGEN_HEALTH":
                type = BonusType.REGEN_HEALTH;
                break;
            case "SHIELD":
                type = BonusType.SHIELD;
                break;
            case "COLLECT_ALL":
                type = BonusType.COLLECT_ALL;
                break;
            default:
                throw new InconsistencyException("BonusType " + split[0] + "is invalid!");
        }
        value = float.Parse(split[1]);
        expireTime = float.Parse(split[2]);
        guid = split[3];
        return new Bonus(type, value, expireTime, guid);

    }
}