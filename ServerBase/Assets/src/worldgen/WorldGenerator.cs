using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * This class generates the world according to a specific pattern.
 * The idea is to do one layer after the other - basically like a 3d-printer.
 * Based on the lower layer it will create the next following layer of tiles.
 * This class can also be used as reference for placing other objects as it will contain
 * information regarding positions of all world-generated objects
 * @author Chris
 */

public class WorldGenerator : MonoBehaviour {
	/** The base tile with which we wanna fill the world*/
	public GameObject basicTile;
	
	public GameObject offlineTile; //Optional, use only with offline mode
	
	public GameObject blueLight;
	
	public float bonusRegenerationDelay = 30f; //Generate boni every x seconds
	
	public GameObject[] bonusElements;
	
	public bool  isOffline;
	
	/** Bounding points as vector2 where x = min and y = max valid ordinate on x*/
	private Vector2 xBound = new Vector2( -135f, 245f);
	
	/** Bounding points as vector2 where x = min and y = max valid ordinate on z*/
	private Vector2 zBound = new Vector2( -245f, 115f);
	
	/** Bounding points as vector2 where x = min and y = max valid ordinate on y*/
	private Vector2 yBound = new Vector2(2.5f, 7.5f); //This should be 2 tile sizes assuming a tile is 5 units tall
	
	private Dictionary <Vector3, GameObject> fixedTiles = new Dictionary<Vector3, GameObject>();
	private Dictionary <Vector3, GameObject> bonusTiles = new Dictionary<Vector3, GameObject>();
	private Dictionary <Vector3, GameObject> tmpTiles = new Dictionary<Vector3, GameObject>();
	private Dictionary <BonusType, int> weightTable= new Dictionary<BonusType, int>();
	
	private int bonusArrayLength;
	private int totalWeight;
	private int maxBoni = -1;
	
	private float regenerateTime; //At this time the regeneration of boni is triggered
	
	private bool positionIsEmpty ( Vector3 test  ){
		 return !fixedTiles.ContainsKey(test) && !bonusTiles.ContainsKey(test) && !tmpTiles.ContainsKey(test);
	}
	
	public void  generate (){
		if (Network.isClient) {
			return;
		}
		//The height
		for (float currentY = yBound.x; currentY <= yBound.y; currentY+=5) {
			for (float currentX = xBound.x; currentX < xBound.y; currentX += 5) {
				for (float currentZ = zBound.x; currentZ < zBound.y; currentZ += 5) {
					Vector3 pos = new Vector3(currentX, currentY, currentZ);
					if (Random.value < 0.05f) { //Spawn block
						if (positionIsEmpty(pos)) {
							if (isOffline) {
								fixedTiles.Add(pos, Instantiate(offlineTile, pos, Quaternion.identity) as GameObject);
							}
							else {
								if (currentY < 3) { //Only first layer!
									if (Random.value < 0.37f) {
										int bid = GroupManager.nextGroupId();
                                        GameObject btmp = Network.Instantiate(basicTile, pos, Quaternion.identity, bid) as GameObject;
										btmp.GetComponent<GroupManager>().groupId = bid;
										tmpTiles.Add(pos, btmp);
										Debug.Log("Dynamic Block spawned with ID " + bid);
										//System.IO.File.AppendAllText("server.log", "Dynamic Block spawned with ID " + bid + "\n");
									}
									else {
                                        fixedTiles.Add(pos, Network.Instantiate(offlineTile, pos, Quaternion.identity, (int)NetworkGroup.GEOMETRY) as GameObject);
									}
								}
								else { //On the second layer, only spawn static objects
                                    fixedTiles.Add(pos, Network.Instantiate(offlineTile, pos, Quaternion.identity, (int)NetworkGroup.GEOMETRY) as GameObject);
								}
								
							}
						}
					}
					else if (Random.value < 0.002f) { //Spawn light
						if (positionIsEmpty(pos)) {
							if (isOffline) {
                                fixedTiles.Add(pos, Instantiate(blueLight, pos, Quaternion.identity) as GameObject);
							}
							else {
                                fixedTiles.Add(pos, Network.Instantiate(blueLight, pos, Quaternion.identity, (int)NetworkGroup.GEOMETRY) as GameObject);
							}
						}
					}
				}
			}
		}
		generateBoni();
	}
	
	private void clearBonusList (){
		List<Vector3> toRemove = new List<Vector3>();
		foreach(Vector3 go in bonusTiles.Keys) {
			if (bonusTiles[go] == null) {
				toRemove.Add(go);
			}
		}
		Debug.Log("Found " + toRemove.Count + " null elements in bonus list - removing");
		foreach(Vector3 pos in toRemove) {
			bonusTiles.Remove(pos);
		}
	}
	
	private void  generateBoni (){
		if (Network.isClient) {
			return;
		}
		int currentlyGeneratedBoni = 0;
		int randomNew = Random.Range(5, 20);
		for (float currentY = yBound.x; currentY <= yBound.y; currentY+=5) {
			for (float currentX = xBound.x; currentX < xBound.y; currentX += 5) {
				for (float currentZ = zBound.x; currentZ < zBound.y; currentZ += 5) {
					Vector3 pos = new Vector3(currentX, currentY, currentZ);
					if (!positionIsEmpty(pos)) {
						continue;
					}
					if (maxBoni != -1 && ((currentlyGeneratedBoni + bonusTiles.Count >= maxBoni) || (currentlyGeneratedBoni >= randomNew))) {
						break; //We're done, do not exceed the max amount of boni
					}
					if (Random.value < 0.05f && !isOffline && currentY < 3) { //Make sure things spawn on first layer only
						//Use a very custom network group id here so we can effectively use network instantiate and destroy
						//and keep track of the buffered RPCs
						int iid = GroupManager.nextGroupId();
						GameObject tmp = Network.Instantiate(getWeightedRandomBonus(), pos, Quaternion.identity, iid) as GameObject;
						//We need to make this id known so we can reference it later in PlayerManager
						tmp.GetComponent<GroupManager>().groupId = iid;
						bonusTiles.Add(pos, tmp);
						Debug.Log("Bonus spawned with ID " + iid);
						//System.IO.File.AppendAllText("server.log", "Bonus spawned with ID " + iid + "\n");
						currentlyGeneratedBoni++;
					}
				}
			}
		}
		if (regenerateTime == 0) { //That means we're running the first time
			maxBoni = bonusTiles.Count;
		}
		regenerateTime = Time.time + bonusRegenerationDelay;
	}
	
	private int bonusTypeToArrayIndex ( BonusType type  ){
		int ret = 0;
		switch(type) {
			case BonusType.POINTS:
				ret = 0;
				break;
			case BonusType.POINT_MULTIPLY:
				ret = 1;
				break;
			case BonusType.SPEED:
				ret = 2;
				break;
		}
		
		return ret;
	}
	
	private GameObject getWeightedRandomBonus (){
		float randomNum= Random.Range(0, totalWeight);
		/*
		 * The following is a dirty work-around because there are no values for the bonus objects
		 * at the moment of the world generating stuff.
		 * The element index is taken from how the scenes netman is set up so watch out!
		 */
		foreach(BonusType bonus in weightTable.Keys) {
			int weight = weightTable[bonus];
			if (randomNum < weight) {
				return bonusElements[bonusTypeToArrayIndex(bonus)];
			}
			randomNum -= weight;
		}
		return bonusElements[0]; //Return default, a point 
	}
	
	public WorldGenerator (){
		weightTable.Add(BonusType.POINTS, 20);
		weightTable.Add(BonusType.POINT_MULTIPLY, 8);
		weightTable.Add(BonusType.SPEED, 5);
	}
	
	/**
	 * Destroys and re-spawns all bonuses.
	 * Use with caution!
	 */
	public void  respawnBoni (){
		//Reset the max boni modifier to allow the spawner
		//to generate a fresh list of boni
		maxBoni = -1;
		//Reset regenerate time so the generator knows we want to start over
		regenerateTime = 0;
		
		//Remove null elements from the list
		clearBonusList();
		
		//Destroy all remaining objects
		foreach(Vector3 pos in bonusTiles.Keys) {
			GameObject bonus = bonusTiles[pos];
			NetworkAutoDestroy nad = bonus.GetComponent<NetworkAutoDestroy>();
			if (nad == null) {
				Debug.Log("NAD for " + bonus.name + " did not exist. What sort of idiocy is this? Fix it!");
				continue;
			}
			//Schedule deletion for the next update tick
			nad.destroyThis = true;
			nad.expireTime = -1;
		}
		
		//Remove all key/values in the tile list
		bonusTiles.Clear();
		
		//Create new boni
		generateBoni();
	}
	
	public void  Start (){
		if (Network.isClient) {
			return;
		}
		foreach(BonusType bonus in weightTable.Keys) {
			int weight = weightTable[bonus];
			totalWeight += weight;
		}
		Debug.Log("Total weight is " + totalWeight);
		
		bonusArrayLength = bonusElements.Length;
		generate();
	}
	
	public void  Update (){
		if (Network.isClient) {
			return;
		}
		if (Time.time >= regenerateTime) {
			clearBonusList();
			generateBoni();
		}
	}
}
