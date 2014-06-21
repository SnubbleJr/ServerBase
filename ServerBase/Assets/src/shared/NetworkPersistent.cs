using UnityEngine;
using System.Collections;

/**
 * makes sure the object this is attached to is persistent through network loads etc
 */

public class NetworkPersistent : MonoBehaviour {

    void Awake()
    {
        foreach (Transform child in transform) {
    		//print ("child: " + child.name);
	    	GameObject.DontDestroyOnLoad(child.gameObject);
    	}
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
