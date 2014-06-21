using UnityEngine;
using System.Collections;

/**
 * C_Netman is the client-side network manager.
 * In this class are ONLY functions that are called on or as the client
 */

public class C_Netman : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnConnectedToServer()
	{
		Debug.Log("Disabling message queue!");
		Network.isMessageQueueRunning = false;
		Application.LoadLevel(Netman.levelName);
	}
	
	void OnLevelWasLoaded(int level)
	{
		if (level != 0 && Network.isClient) //0 is my menu scene so ignore that.
		{
            Network.isMessageQueueRunning = true; //Was disabled when connecting
			Debug.Log("Level was loaded, requesting spawn - re-enabling message queue!");
            Debug.Log("Requesting Spawn ...");
			//Request a player instance form the server
			networkView.RPC("requestSpawn", RPCMode.Server, Network.player);
		}
	}
}
