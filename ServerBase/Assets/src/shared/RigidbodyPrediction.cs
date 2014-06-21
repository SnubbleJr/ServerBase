using UnityEngine;
using System.Collections;

/**
 * This class specifically handles the prediction of "unimportant" rigidbody objects by
 * interpolating the local position with the server positions.
 */
public class RigidbodyPrediction : MonoBehaviour {
	
	public Transform observed;
	
	private Vector3 serverPos;
	private Quaternion serverRot;
	
	private Rigidbody rbody;
	
	public void  Start (){
		if (Network.isServer) {
			rbody = GetComponent<Rigidbody>();
		}
		else {
			Rigidbody body = GetComponent<Rigidbody>();
			if (body) {
				body.isKinematic = true;
			}
		}
	}
	
	
	public void  OnSerializeNetworkView ( BitStream stream ,   NetworkMessageInfo info  ){
		//Sync positions!
		if (stream.isWriting) {
			//Server is going on
			serverPos = observed.position;
			stream.Serialize(ref serverPos, 0.01f); //Cut the value precision down we don't need that for positions ...
			serverRot = observed.rotation;
			stream.Serialize(ref serverRot, 0.01f); //... or rotations
		}
		else {
			//Client is going on
			
			stream.Serialize(ref serverPos);
			stream.Serialize(ref serverRot);
		}
	}
	
	public void  Update (){
		if (Network.isServer) {
			return; //Get off, this is for client-side visualisation
		}
		transform.position = Vector3.Lerp(transform.position, serverPos, 0.3f);
		transform.rotation = Quaternion.Slerp(transform.rotation, serverRot, 0.3f);
	}
}