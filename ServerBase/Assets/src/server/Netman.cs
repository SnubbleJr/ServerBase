using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Server-side implementation for the network manager.
 * In this class are ONLY functions that are called on or as the server.
 * It takes care of the general networking fun such as player tracking and spawning of players
 */
public class Netman : MonoBehaviour {
	public GameObject player;
	public static string levelName; //level name
	public static bool  levelIsLoaded;

	private List<C_PlayerManager> playerTracker = new List<C_PlayerManager>();
	private List<NetworkPlayer> scheduledSpawns = new List<NetworkPlayer>();

	private Dictionary<NetworkPlayer, float> playerScores = new Dictionary<NetworkPlayer, float>();

	private bool  processSpawnRequests = false;
	
	//Stuff for the playing time as this should not me handled within bonus manager
	private float timeEnd; //The this game ends in seconds
	private float displayTime; //the currently remaining time
	
	public void  Start ()
    {
		//Up that for we have a crapload of those things
		Network.minimumAllocatableViewIDs = 800;
		timeEnd = Time.time + 30000; //2 minutes
	}
	
	/**
	 * Update the remaining time and return the current value
	 * @return
	 */
	public float updateTime ()
    {
		displayTime = timeEnd - Time.time;
		return displayTime;
	}
	
	/**
	 * Compiles the scores for all connected players into a string.
	 * @return
	 */
	public string compileScoreResults ()
    {
		string str ="";
		bool  first = true;
		foreach(C_PlayerManager player in playerTracker) 
        {
			BonusManager bonusMan = Utils.getTopmostParent(player.gameObject).GetComponentInChildren<BonusManager>();
			if (bonusMan == null) 
            {
				//FU!
				Debug.Log("No bonus manager on " + player.name);
			}
			else
            {
				
				if (!first)
                {
					str += ";";
				}
				else
                {
					first = false;
				}
				str += player.name + ":" + bonusMan.getPoints();
				
			}
		}
		Debug.Log("Compiled score list: " + str);
		return str;
	}
	
	/**
	 * Called on the server, and adds a player to a list of objects that need spawning.
	 * @param	player
	 */
	void OnPlayerConnected ( NetworkPlayer player  )
    {
		Debug.Log("Scheduling spawn for " + player);
		scheduledSpawns.Add(player);
		processSpawnRequests = true;
	}
	
	/**
	 * Called on the server, it disconnects the player, removes if from the tracking list
	 * and also destroys the player instance and removes RPC calls for all attached NetworkViews
	 * @param	player
	 */
	void OnPlayerDisconnected ( NetworkPlayer player  )
    {
		Debug.Log("Player " + player.guid + " disconnected. Cleaning up ...");
		C_PlayerManager found = null;
		foreach(C_PlayerManager man in playerTracker)
        {
			
			if (man.getOwner() == player) 
            {
				
				Debug.Log("Player ViewId: " + man.getParentNetworkView().viewID);
				Network.RemoveRPCs(man.getParentNetworkView().viewID);
				Network.RemoveRPCs(man.getNetworkView().viewID);
				//Please note: GameObjects are not iterable, however, their Transform components are
				//And every GO has one.
				GameObject groupGo = man.gameObject.transform.parent.gameObject; //That's the topmost container for a player
				//finally remove the top one
				Network.Destroy(groupGo);
				Network.CloseConnection(man.getOwner(), false);
				found = man;
			}
		}
		if (found) 
        {
			playerTracker.Remove(found);
		}
	}

	/**
	 * This is remotely called from the client and signals the server that the client
	 * would like to spawn its player object.
	 * @param	requester The client netId that requested the spawn
	 */
	[RPC]
	void  requestSpawn ( NetworkPlayer requester  )
    {
		//Called from client to the server to request a new entity
		if (Network.isClient) 
        {
			Debug.LogError("Client tried to spawn itself! Revise logic, this is critical!");
			return; //Get lost! This is server business
		}
		if (!processSpawnRequests)
        {
			return; //silently ignore this
		}
		//Process all scheduled players
		foreach(NetworkPlayer spawn in scheduledSpawns)
        {
			if (spawn == requester) { //That is the one, lets make him an entity!
				Debug.Log("Found a freshly spawned player as " + spawn);
				GameObject handle =  Network.Instantiate(player, transform.position, Quaternion.identity, (int)NetworkGroup.PLAYER) as GameObject;
				C_PlayerManager sc= handle.GetComponentInChildren<C_PlayerManager>();
				PlayerManager srv = handle.GetComponentInChildren<PlayerManager>();

				if (!sc)
                {
					Debug.LogError("No client script attached to player prefab!");
				}
				if (!srv)
                {
					Debug.LogError("No server script attached to player prefab!");
				}
				playerTracker.Add(sc);
				NetworkView netView = srv.GetComponent<NetworkView>();
				bool exists= NetworkView.Find(netView.viewID);
				if (exists)
                {
					netView.RPC("setOwner", RPCMode.AllBuffered, spawn);
					srv.setClient(sc);
				}
				else
                {
					Network.RemoveRPCs(requester);
					Network.CloseConnection(requester, true);
				}
				break;
			}
		}
		scheduledSpawns.Remove(requester); //Remove the guy form the list now
		if (scheduledSpawns.Count == 0)
        {
			Debug.Log("spawns is empty! stopping spawn request processing");
			//If we have no more scheduled spawns, stop trying to process spawn requests
			processSpawnRequests = false;
		}
	}
	
	public void  changeLevel ( string newLevel  )
    {
		foreach(C_PlayerManager player in playerTracker)
        {
			playerScores.Add(player.getOwner(), player.getParent().GetComponent<PlayerManager>().bonusMan.getPoints());
			player.getNetworkView().RPC("clientChangeLevel", RPCMode.Others, newLevel);
		}
		Application.LoadLevel(newLevel);
	}
}