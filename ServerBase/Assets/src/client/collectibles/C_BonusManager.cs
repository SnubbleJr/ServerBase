using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Client-side bonus manager implementation, used to represent bonus and score values to the client.
 */

class C_BonusManager : MonoBehaviour {
	private Dictionary<string, Bonus> stats = new Dictionary<string, Bonus>();
	
	private float currentPoints;
	
	private float timeRemaining;
	
	private bool  useExistingtableData;
	
	private string[] tableData; //Scores table data
	
	private ScoreDisplay scores;
	
	/**
	 * Get the amount of points currently scored by the player
	 * @return
	 */
	public float getPoints (){
		return Mathf.RoundToInt(currentPoints);
	}
	
	public float getTime (){
		return timeRemaining;
	}
	
	public string getTimeAsString (){
		int d = (int)timeRemaining; //millis
		int minutes = d / 60; //minutes
		int seconds = d % 60; //Seconds
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}
	
	public float getBonusForType ( BonusType type  ){
		float amount = 0.0f;
		foreach(string id in stats.Keys) {
			Bonus bonus = stats[id];
			if (bonus.getType() == type) {
				amount += bonus.getValue();
			}
		}
		float product = Mathf.Floor(amount * 10f) / 10f;
		if (type != BonusType.SPEED) {
			return product;
		}
		//Do some stuff to make the speed bonus look better on the hud :)
		if (product > 0.4f) {
			product = 0.4f;
		}
		return product / 0.004f;
	}
	
	/**
	 * Create a score based on all existing bonus stats.
	 * NOTE: This is a stripped down version as the client actually doesn't need to
	 * create any scores, the server does that. We only do the speed bonus here
	 * for prediction purposes.
	 * @param	value
	 * @param	useMultiply
	 * @return
	 */
	public float createScore ( float value ,   bool useMultiply  ){
		foreach(string id in stats.Keys) {
			Bonus bonus = stats[id];
			if ((bonus.getType() == BonusType.SPEED) && (!bonus.isExpired())) {
				value += bonus.process(value);
			}
		}
		return value;
	}
	
	public void  setScoreTable ( string[] table  ){
		if (scores == null) {
			Debug.Log("ScoreBoard data is not set yet, caching table data for when the scoredboard is set.");
			tableData = table;
			useExistingtableData = true;
			return;
		}
		scores.setScoreTable(table);
	}
	
	public void  setScoreBoard ( ScoreDisplay score  ){
		scores = score;
		//This is a case where a player would connect during the scoreboard display
		//It might also occur when connecting late, after another player has connected,
		//Or after connecting multiple times. So yeah, that's that
		if (useExistingtableData) {
			scores.setScoreTable(tableData);
		}
	}
	
	/**
	 * Called from the server to make the client remove a bonus from the stack
	 * @param	stat
	 */
	[RPC]
	public void removeStatOnClient ( string stat  ){
		stats.Remove(stat);
	}
	
	/**
	 * Called from the server to ake the client add a bonus to the stack
	 * @param	serialized
	 */
	[RPC]
	public void addStatOnClient ( string serialized  ){
		try {
			Bonus bonus = Bonus.deserialize(serialized);
			if (bonus.getType() != BonusType.POINTS) {
				stats.Add(bonus.getGuid(), bonus);
			}
		}
        catch (Exception e)
        {
			Debug.LogError(e);
		}
	}
	
	/**
	 * Called from the server to override current client points
	 * @param	points
	 */
	[RPC]
	public void setPointsOnClient ( float points  ){
		currentPoints = points;
	}
	
	[RPC]
	public void setTimeOnClient ( float time  ){
		timeRemaining = time;
	}
	
	public void  showScores (){
		if (scores == null) {
			Debug.Log("ScoreBoard data is not set yet, trying again in next tick!");
			return;
		}
		scores.displayScores = true;
	}
	
	public void  hideScores (){
		if (scores == null) {
			Debug.Log("ScoreBoard data is not set yet, trying again in next tick!");
			return;
		}
		scores.displayScores = false;
		scores.hideBackdrop();
	}
}