using UnityEngine;
using System.Collections;

/**
 * Follows the game object it is bound to
 */

public class C_FollowCam : MonoBehaviour {

    public Transform target;
    public float slack;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 pos = Vector3.Lerp(transform.position, target.position, slack * Time.deltaTime);
		pos.y = transform.position.y;
		transform.position = pos;
		//transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, slack * Time.deltaTime);
	}
}
