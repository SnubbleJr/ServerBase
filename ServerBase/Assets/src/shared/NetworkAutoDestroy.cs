using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GroupManager))]
/**
 * This is specifically used on objects that appear only temporarily and
 * should not be re-spawned for newly connecting players, if they are removed already.
 * For instance: Bonus objects. Once collected they are gone.
 * This finds use in PlayerManager.
 */
public class NetworkAutoDestroy : MonoBehaviour {

    public float expireTime;
    public bool destroyThis;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Network.isServer)
        {
            if ((Time.time > expireTime || expireTime == -1) && destroyThis)
            {
                GetComponent<GroupManager>().destroy();
            }
        }
	}

    [RPC]
    public void PLAYoNcLIENT()
    {
        ParticleSystem particle = GetComponent<ParticleSystem>();
        particle.Play(); //Destruction is scheduled on the server already
    }
}