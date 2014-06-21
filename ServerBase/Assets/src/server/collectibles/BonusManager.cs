using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BonusManager : MonoBehaviour {

	/** List of stat effects like additional speed or points bonus*/
	private Dictionary<string, Bonus> stats = new Dictionary<string, Bonus>();
	
	private NetworkPlayer player;
	
	/** Points calculated out of the bonus are multiplied by this value for the final result!*/
	private float pointMultiplier = 1f, currentPoints, displayTime; //The time in milliseconds that need to be displayed
		
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
	public void updateTime(float time)
    {
		if (time != displayTime) {
			displayTime = time;
			networkView.RPC("setTimeOnClient", player, displayTime);
		}
	}

	/**
	 * Set the NetworkPlayer for this bonus manager
	 */
	public void setPlayer(NetworkPlayer player)
    {
		this.player = player;
	}
	
	/**
	 * Get the current amount of collected points
	 */
	public float getPoints()
    {
		return currentPoints;
	}
	
	/**
	 * Add the given amount of points to the manager
	 */
	public void addPoints(float points)
    {
		currentPoints += points;
		networkView.RPC("setPointsOnClient", player, currentPoints);
	}
	
	/**
	 * Override the points with the given value
	 */
	public void setPoints(float points)
    {
		currentPoints = points;
		networkView.RPC("setPointsOnClient", player, currentPoints);
	}
	
	/**
	 * Remove the given amount of points
	 */
	public void removePoints(float points)
    {
		currentPoints -= points;
		networkView.RPC("setPointsOnClient", player, currentPoints);
	}
	
	/**
	 * Add a bonus to the manager. In case it's a point bonus this will execute the
	 * methods to properly add score points.
	 * This method also updates the client-side 
	 */
	public void addStat(Bonus newBonus, float ttl)
    {
		newBonus.expireTime = Time.time + ttl;
		networkView.RPC("addStatOnClient", player, newBonus.toString());
		Debug.Log("Adding stat: " + newBonus.toString());
		
		if (newBonus.getType() == BonusType.POINTS)
        {
			addPoints(newBonus.getValue());
		}
		else
        {
			this.stats.Add(newBonus.getGuid(), newBonus);
		}
	}
	
	/**
	 * Remove a given bonus from the manager.
	 * This will also update the client-side
	 */
	public void removeStat(Bonus bonus)
    {
		networkView.RPC("removeStatOnClient", player, bonus.getGuid());
		this.stats.Remove(bonus.getGuid());
	}
	
	/**
	 * Remove a names bonus from the manager.
	 * This will also update the client-side
	 */
	public void removeStat(string bonus)
    {
		this.stats.Remove(bonus);
		networkView.RPC("removeStatOnClient", player, bonus);
	}
	
	/**
	 * Add one to the point multiplier
	 * This also updates the client.side
	 */
	public void incrementMultiplier()
    {
		pointMultiplier++;
	}
	
	/**
	 * Run through the bonuses and check if they are expired.
	 * If so, remove them. This will also send update information to the client.
	 */
	public void updateStats() {
		//To avoid concurrent modifications we file a list
		//which we will process after the real bonus list
		List<string> toRem = new List<string>();

		foreach (string id in stats.Keys)
        {
			Bonus bon = stats[id];
			if (bon.isExpired())
            {
				Debug.Log("Removing " + bon.toString());
				toRem.Add(id);
			}
		}
		//Now remove the things
		foreach (string idx in toRem)
        {
			removeStat(idx); //This will trigger an RPC on the client side too!
		}
		//aaaand done!	
	}

    
	/**
	 * Create the actual value for the input data based on existing boni.
	 */
	public float createScore(float value, BonusType type, bool useMultiply)
    {
		foreach (string id in stats.Keys)
        {
			Bonus bonus = stats[id];
			if ((bonus.getType() == type) && (!bonus.isExpired()))
            {
				value += bonus.process(bonus.value);
			}
		}

        float product;
		if (useMultiply)
        {
			product = value * pointMultiplier;
		}
		else
        {
			product = value;
		}

		if (type == BonusType.SPEED && product > 0.4)//Limit the speed bonus to avoid crazy movement speeds
        { 
			product = 0.4f;
		}
		return product;
	}
    
	public bool isGameOver()
    {
		return displayTime <= 0;
	}
}
