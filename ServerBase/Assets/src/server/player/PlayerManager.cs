using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NetworkView))]
[RequireComponent (typeof (CharacterController))]
/**
 * Serverside player management.
 * This class handles communication to its client-side, and will also
 * send data to all other peers if needed.
 * @author Chris
 */
public class PlayerManager : MonoBehaviour {

    public float speed = 10;
	
	public BonusManager bonusMan; //DO NOT SET IN EDITOR
	public Netman netman; //DO NOT SET IN EDITOR
	
	private CharacterController controller;

	private float horizontalMotion;
	private float verticalMotion;
	private Vector3 mouseTarget;

	private C_PlayerManager clientView;
	private bool  mouseDown;
	private Plane plane = new Plane(Vector3.up, Vector3.zero); //This is for mouse input processing
	private Rigidbody rbody;
	
	private bool  gameOver;
	
	/**
	 * Prepare the class by retrieving required components
	 */
	public void  Start (){
		if (Network.isServer) {
			controller = GetComponent<CharacterController>();
			bonusMan = GetComponent<BonusManager>();
			rbody = GetComponent<Rigidbody>();
			if (bonusMan == null) {
				Debug.LogError("No BonusManager on server side found!");
			}
			bonusMan.setPlayer(clientView.getOwner());
			netman = GameObject.FindWithTag("Netman").GetComponent<Netman>();
		}
	}
	
	/**
	 * Set the client-side object. This is not actually a networked object, however the data within, such
	 * as client id and network view id is needed.
	 * @param	client
	 */
	public void  setClient ( C_PlayerManager client  ){
		if (Network.isClient) {
			Debug.Log("That doesnt work like this, dude!");
			return;
		}
		Debug.Log("Setting client cam");
		if (client == null) {
			Debug.Log("Camera was not found here for last spawned client!");
		}
		clientView = client;
	}
	
	/**
	 * Process all input data that we currently have from the client
	 * aswell as check through the bonus manager for expired bonus stats.
	 */
	public void  Update (){
		if (Network.isClient) {
			return; //Nope
		}
		if (!gameOver) {
			processAnalogInput();
			processMouseInput();
			bonusMan.updateStats();
			bonusMan.updateTime(netman.updateTime());
			gameOver = bonusMan.isGameOver();
		}
		else {
			networkView.RPC("setGameOverOnClient", clientView.getOwner());
			networkView.RPC("setScoreListOnClient", clientView.getOwner(), netman.compileScoreResults());
		}
	}
	
	/**
	 * Handle the keyboard input
	 */
	private void  processAnalogInput (){
		if (Network.isClient) {
			return; //Get lost, this is the server-side!
		}
		//Debug.Log("Processing clients movement commands on server");
		//Limit vertical speed:
		float limitFactor = (float)((horizontalMotion != 0.0f && verticalMotion != 0.0f) ? .7071f : 1.0f);
		Vector3 motion = new Vector3((horizontalMotion * speed) * limitFactor * Time.deltaTime, 0, (verticalMotion * speed) * limitFactor * Time.deltaTime);
		
		controller.Move(motion);
		//Reset Y to 0 we don't move up or down
        Vector3 newPos = transform.position;
        newPos.y = 2f;
		transform.position = newPos;
		//Rotate into looking direction!
		if (motion != Vector3.zero) {
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation( motion ), 0.6f);
            transform.eulerAngles = new Vector3(transform.rotation.x - 90, transform.rotation.y, transform.rotation.z); //Workaround to make the mesh look right
		}
	}
	
	/**
	 * Handle the mouse input
	 */
	private void  processMouseInput (){
		if (clientView == null) {
			//Just in case so we won't go nuts
			return;
		}
		if (mouseDown) {
			//Get the movement delta
			Vector3 motion= C_PlayerManager.clampMotionVector(transform.position, mouseTarget, speed, 1.5f, 0f);
			transform.LookAt(mouseTarget);
			//transform.Translate(transform.position.forward * speed * Time.deltaTime);
			controller.Move(motion);
		}
		//fix angles:
        transform.eulerAngles = new Vector3(transform.rotation.x - 90, transform.rotation.y, transform.rotation.z); //Workaround to make the mesh look right
	}
	
	/**
	 * This is used to check if we collided with another Rigidbody/Collider.
	 * If so it will apply force to the object that was hit.
	 * @param	hit
	 */
	public void  OnControllerColliderHit ( ControllerColliderHit hit  ){
		if (Network.isClient) {
			return; //Only the server is allowed to do physics stuff!
		}
		Rigidbody body = hit.collider.attachedRigidbody;
		
		// do rigidbody collisions
		if (body != null && !body.isKinematic) {
			Vector3 pushDir= new Vector3 (hit.moveDirection.x, 0, hit.moveDirection.z);
			body.velocity = pushDir * rbody.mass; //currentSpeed what?
		}
	}
	
	/**
	 * Since bonus objects contain triggers colliders, this will be called when we hit a
	 * bonus object so we can add it to the bonus manager
	 * @param	hit
	 */
	public void  OnTriggerEnter ( Collider hit  ){
		if (Network.isClient) {
			return;
		}
		BonusHandler bonus = hit.collider.gameObject.GetComponent<BonusHandler>();
		
		if (bonus == null) {
			return;
		}
		bonusMan.addStat(bonus.getBonus(), bonus.timeToLife);
		//hit.collider.gameObject.networkView.RPC("collected", RPCMode.Others);
        var redy = Network.Instantiate(bonus.effectOnCollect as GameObject, transform.position, transform.rotation, (int)NetworkGroup.TMP) as GameObject;
		ParticleSystem particle = redy.GetComponent<ParticleSystem>();
		particle.Play();
		NetworkAutoDestroy nad = redy.GetComponent<NetworkAutoDestroy>();
		if (nad == null) {
			return; //eeek!
		}
		nad.networkView.RPC("playOnClient", RPCMode.Others);
		nad.expireTime = Time.time + particle.duration + 0.2f;
		nad.destroyThis = true;
		//Network.Destroy(hit.collider.gameObject.networkView.viewID);
		NetworkAutoDestroy collisionNad = hit.collider.gameObject.GetComponent<NetworkAutoDestroy>();
		collisionNad.expireTime = -1;
		collisionNad.destroyThis = true;
		//netman.networkDestroy(hit.collider.gameObject.networkView.viewID, RPCMode.AllBuffered);
	}

	/**
	 * The client calls this to notify the server about new motion data
	 * @param	motion
	 */
	[RPC]
	public void updateAnalogInput ( float hor ,   float vert  ){
		horizontalMotion = hor;
		verticalMotion = vert;
	}
	
	/**
	 * The client calls this to notify the server about the new mouse position.
	 * @param	mouse
	 * @param	mouseDown
	 */
	[RPC]
	public void  updateMouseInput ( Vector3 mouse ,   bool mouseDown  ){
		 mouseTarget = mouse;
		this.mouseDown = mouseDown;
	}
}
