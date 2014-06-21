using UnityEngine;
using System.Collections;

[RequireComponent (typeof(NetworkAutoDestroy))]

/**
 * Make an object auto-destroy 
 */
public class DestroyOnHeight : MonoBehaviour {

    public float height; //The height at which this object should be destroyed

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
			if (transform.position.y >= height) {
			NetworkAutoDestroy nad  = GetComponent<NetworkAutoDestroy>();
			nad.destroyThis = true;
			nad.expireTime = -1;
		}
	}
}
