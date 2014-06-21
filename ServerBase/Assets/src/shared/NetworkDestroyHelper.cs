using UnityEngine;
using System.Collections;

public class NetworkDestroyHelper : MonoBehaviour {
    	/**
	 * This is deployed to enable the null'ing of the networked object.
	 * @param	groupid
	 * @param	manager
	 */
	public static void destroyObject(int groupid, GroupManager manager)
    {
		Network.RemoveRPCsInGroup(groupid);
		Network.Destroy(manager.gameObject);
		manager = null;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
