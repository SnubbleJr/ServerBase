using UnityEngine;
using System.Collections;

public class GroupManager : MonoBehaviour {

	private static int currentId = 10;
	public int groupId;
	
	public void  destroy (){
		if (Network.isClient) {
			return; //A client cannot destroy this
		}
		NetworkDestroyHelper.destroyObject(groupId, this);
		/*Network.RemoveRPCsInGroup(groupId);
		Network.Destroy(gameObject);*/
	}
	
	public static int nextGroupId (){
		int ret = currentId + 1;
		currentId++;
		return ret;
	}
}
