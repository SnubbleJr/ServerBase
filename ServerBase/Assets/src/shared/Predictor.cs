using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]

/**
 * Used to predict movement data on the client, for remote client objects
 * Is contained within shared as this contains server aswell as client code.
 * The network view serialization call requires a script to work so here goes.
 */
public class Predictor : MonoBehaviour {

    public Transform observedTransform;
    public C_PlayerManager  receiver; //Guy who is receiving data
    public float  pingMargin = 0.5f; //ping top-margin

    private float clientPing;

    private  NetState[] localStateBuffer = new NetState[20], serverStateBuffer = new NetState[20];

	// Use this for initialization
	void Start () {
	
	}
	


    public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        var pos = observedTransform.position;
        var rot = observedTransform.rotation;
    
        if (stream.isWriting)
        {
            //Debug.Log("Server is writing");
            stream.Serialize(ref pos, 0.1f);
            stream.Serialize(ref rot, 0.1f);
        }
        else
        {
            //This code takes care of the local client!
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            receiver.setServerPos(pos);
            receiver.setServerRot(rot);
            //Smoothly correct clients position
            receiver.lerpToTarget();
			
            //Smoothly correct clients position
            receiver.lerpToTarget();
        
            //Take care of data for interpolating remote objects movements
            // Shift up the buffer
            for ( int i = serverStateBuffer.Length - 1; i >= 1; i-- )
            {
                serverStateBuffer[i] = serverStateBuffer[i-1];
            }

            //Override the first element with the latest server info
            serverStateBuffer[0] = new NetState( (float)info.timestamp, pos, rot);
        }
    }

    /**
     * Helper here, displays the player ping.
     * On the client this will only display the connection to the server
     * as all other peers are unknown. The server will see a list of all connected clients here
     */
    void OnGUI()
    {
        for (int i = 0; i < Network.connections.Length; i++)
        {
            GUILayout.Label("(Player " + i + 1 + ")Ping: " +
                Network.GetAveragePing(Network.connections[i]) + " ms");
        }
    }

    /**
 * This update will predict the movement of _remote_ peers for the _local_ client!
 * For this it makes use of the serverStateBuffer.
 * By checking how long the last tick on the local player took and how long the messages were on the network,
 * it will choose a buffer to play and interpolate between this and the last known good position.
 * This will likely only kick in on high-latency connections.
 * If the latency is small enough it will just "extrapolate" the local position with the most recent server position.
 */
	void Update () {

	    if ((Network.player == receiver.getOwner()) || Network.isServer)
        {
            return; //This is only for remote peers, get off
        }

        //client side has !!only the server connected!!
        clientPing = (Network.GetAveragePing(Network.connections[0]) / 100) + pingMargin;
        var interpolationTime = Network.time - clientPing;

        //ensure the buffer has at least one element:
        if (serverStateBuffer[0] == null)
        {
            serverStateBuffer[0] = new NetState(0, transform.position, transform.rotation);
        }

        //Try interpolation if possible. 
        //If the latest serverStateBuffer timestamp is smaller than the latency
        //we're not slow enough to really lag out and just extrapolate.
        if (serverStateBuffer[0].timestamp > interpolationTime)
        {
            for (int i = 0; i < serverStateBuffer.Length; i++)
            {
                if (serverStateBuffer[i] == null)
                {
                    continue;
                }
                // Find the state which matches the interp. time or use last state
                if (serverStateBuffer[i].timestamp <= interpolationTime || i == serverStateBuffer.Length - 1)
                {                
					// The state one slot newer (<clientPing+margin) than the best playback state
					NetState bestTarget = serverStateBuffer[Mathf.Max(i-1, 0)];
					// The best playback state (closest to clientPing+margin old (default time))
					NetState bestStart = serverStateBuffer[i];
					
					// Use the time between the two slots to determine if interpolation is necessary
					float length = bestTarget.timestamp - bestStart.timestamp;
					Debug.Log("Time between 2 selected states is " + length);
					float lerpTime = 0.0f;
					// As the time difference gets closer to 100 ms t gets closer to 1 in 
					// which case rhs is only used
					if (length > 0.0001f) {
						lerpTime = (float)((interpolationTime - bestStart.timestamp) / length);
					}
					
					// if t=0 => lhs is used directly
					Debug.Log("Interpolating with factor " + lerpTime);
					transform.position = Vector3.Lerp(bestStart.pos, bestTarget.pos, lerpTime);
					transform.rotation = Quaternion.Slerp(bestStart.rot, bestTarget.rot, lerpTime);
					//Okay found our way through to lerp the positions, lets return here
					return;
                }
            }
        }
        //so it appears there is no lag through latency.
        else
        {
            //Debug.Log("Xtrapolating!!!");
            if (Random.Range(0, 100) < 50)
            {
                Debug.Log("xtrapolating");
            }
            NetState latest = serverStateBuffer[0];
            transform.position = Vector3.Lerp(transform.position, latest.pos, 0.2f); //Lerp to 20%
            transform.rotation = Quaternion.Slerp(transform.rotation, latest.rot, 0.2f);
        }
	}

    /**
     * Data container that represents a state of a remote peer at a certain time
     */
    public class NetState {
	
	    public NetState() {
		    timestamp = 0.0f;
		    pos = Vector3.zero;
		    rot = Quaternion.identity;
	    }
	
	    public NetState(float time, Vector3 pos, Quaternion rot) {
		    timestamp = time;
		    this.pos = pos;
		    this.rot = rot;
	    }
	
	    public float timestamp; //The time this state occured on the network
	    public Vector3 pos; //Position of the attached object at that time
	    public Quaternion rot; //Rotation at that time
    }
}