using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NetworkView))]
/**
 * Client-side PlayerMovement implementation, only send
 * motion data to the server.
 */
public class C_PlayerManager : MonoBehaviour {
    
	public float speed = 10f;
    public float positionErrorThreshold = 0.2f, hardCorrectionErrorThreshold = 5f;

    private Vector3 serverPos;
    private Quaternion serverRot;

    private GameObject skyboxCamera, myCam;
    private CharacterController controller;
    
	private Plane plane = new Plane(Vector3.up, Vector3.zero); //This is for mouse input processing

	//That's actually not the owner but the player,
	//the server instantiated the prefab for, where this script is attached
	private NetworkPlayer owner;
	
	//Those are stored to only send RPCs to the server when the 
	//data actually changed.
	private float lastMotionH, lastMotionV;

    private C_BonusManager bonusMan;

    private bool isGameOver;

    /**
     * Set up the cameras for the client aswell as the bonus manager
     */
    void Start() {
		if (Network.isServer) {
			return;
        }
        bonusMan = Utils.getTopmostParent(gameObject).GetComponentInChildren<C_BonusManager>();
        if (bonusMan == null)
        {
            Debug.LogError("Could not find BonusManager on client!!");
        }
        skyboxCamera = GameObject.FindWithTag("Skybox") as GameObject;
		if (skyboxCamera == null) {
			Debug.LogError("Failed to find the skybox camera");
			return; //done, fail
        }
        SkyboxCam camScript = skyboxCamera.transform.FindChild("Skybox Camera").GetComponent<SkyboxCam>();
        camScript.prepare(getCamera().transform);
	}
   
	/**
	 * Called from the Predictor to update the playermanager client about
	 * its real position on the server
	 */
    public void setServerPos(Vector3 pos)
    {
        serverPos = pos;
    }

    
	/**
	 * Called from the Predictor to update the playermanager client about
	 * its real rotation on the server
	 */
    public void setServerRot(Quaternion rot)
    {
        serverRot = rot;
    }

    /**
     * Called remotely from the server to set the client who "owns" this C_PlayerManager.
     * This is used to determine what a client will see and what it will be able to control
     */
    [RPC]
	public void setOwner(NetworkPlayer player)
	{
		Debug.Log("Setting the owner.");
		owner = player;

		if(player == Network.player)
		{
			//So it just so happens that WE are the player in question,
			//which means we can enable this control again
            enabled = true;
            controller = GetComponent<CharacterController>(); //Used for "prediction"
            bonusMan = Utils.getTopmostParent(gameObject).GetComponentInChildren<C_BonusManager>();
            if (bonusMan == null)
            {
                Debug.LogError("Could not find BonusManager on client!!");
            }

            Utils.getTopmostParent(gameObject).GetComponentInChildren<HudSpawner>().enabled = true;

            skyboxCamera = GameObject.FindWithTag("Skybox");
			if (skyboxCamera == null)
            {
				Debug.LogError("Failed to find the skybox camera");
				return; //done, fail
            }
            SkyboxCam camScript = skyboxCamera.transform.FindChild("Skybox Camera").GetComponent<SkyboxCam>();
			camScript.prepare(getCamera().transform);
		}
		else
		{
			Debug.Log("Disabling extra components, remote peer");
            GameObject ob = Utils.getTopmostParent(gameObject) as GameObject;
			//Disable a bunch of other things here that are not interesting:

            deactivate();
		}
	}

    /**
     * Returns the NetworkPlayer object that owns this manager
     */
	public NetworkPlayer getOwner()
	{
		return owner;
	}

    	/**
	 * Useed to get the network view on which the instantiation was executed
	 * This is not needed if there was no grouping object around player and camera.
	 * There is though
	 */
	public NetworkView getParentNetworkView()
    {
		return transform.parent.gameObject.networkView;
	}
	
	/**
	 * Get the network view of this object directly.
	 * Used by the server-side PlayerManager to remove RPCs invoked on it
	 */
	public NetworkView getNetworkView()
    {
		return gameObject.networkView;
	}
	
	/**
	 * Returns this clients camera
	 */
	public GameObject getCamera()
    {
		return transform.parent.FindChild("Player Camera").gameObject as GameObject;
	}
    
	/**
	 * Get the parent object of this manager
	 */
	public GameObject getParent()
    {
		return transform.parent.gameObject;
	}
	
	void Awake() {
		//Disable this by default for now
		//Just to make sure no one can use this until we didn't
		//find the right player. (see setOwner())
		if (Network.isClient) {
			enabled = false;
		}
		myCam = getCamera();
	}
		
	/**
	 * Call methods to update player input and send to server
	 */
	void Update () {
		if (Network.isServer) {
			return; //get lost, this is the client side!
		}
		//Check if this update applies for the current client
		if ((owner != null) && (Network.player == owner)) {
			if (!isGameOver) {
				processMouseInput();
				processAnalogInput(); //TODO: Input option!
			}
			else {
				//Display something
			}
		}
		lerpToTarget();
	}
	
	/**
	 * Handle mouse input and update the server with the new target position
	 */
	private void processMouseInput() {
		Vector3 target = myCam.camera.ScreenPointToRay(Input.mousePosition).GetPoint(100);
		if (Input.GetMouseButton(1))
        {
			networkView.RPC("updateMouseInput", RPCMode.Server, target, true);
			//Get the movement delta + some margin so max speed does not require 
			//The mouse to be almost outsode the screen!
			var motion = clampMotionVector(transform.position, target, speed, 1.5f, 0f);
			controller.Move(motion);
			//adjustError(); //
			lerpToTarget();
		} 
		else {
			networkView.RPC("updateMouseInput", RPCMode.Server, target, false);
		}
        //fix angles:
        transform.eulerAngles = new Vector3(transform.rotation.x -90, transform.rotation.y, transform.rotation.z); //Workaround to make the mesh look right
	}
	
	/**
	 * Handle keyboard input and update the server with the new input data
	 */
	private void processAnalogInput()
    {
		if (Input.GetMouseButton(1)) {
			return;
		}
		float motionH = Input.GetAxis("Horizontal");
		float motionV = Input.GetAxis("Vertical");
		networkView.RPC("updateAnalogInput", RPCMode.Server, motionH, motionV);
		//Simulate how we think the motion should come out
		float limitFactor = (motionH != 0.0f && motionV != 0.0f) ? .7071f : 1.0f;
		Vector3 motion = new Vector3((motionH * speed) * limitFactor * Time.deltaTime, 0, (motionV * speed) * limitFactor * Time.deltaTime);
		//if (motion != Vector3.zero) {
			//Debug.Log("an. motion: x: " + motion.x + ", z: " + motion.z);
		//}
		controller.Move(motion);
		transform.position = new Vector3 (transform.position.x, 2f, transform.position.z);
		transform.eulerAngles = new Vector3(transform.rotation.x, (transform.rotation.y + motionH), transform.rotation.z);

        if (motion != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(motion), 0.6f);
            transform.eulerAngles = new Vector3(transform.rotation.x - 90, transform.rotation.y, transform.rotation.z); //Workaround to make the mesh look right
        }
	}
	
	/**
	 * Used to normalize and adjust mouse input motion vectors,
	 * so mouse movement is not as much of a pain plus: applies speed bonus
	 * @param	from where we come from
	 * @param	to Where the mouse position is in the world
	 * @param	speed the speed by which to multiply the normalized motion vector
	 * @param	boost this is applied after normalizing to gain fine control over acceleration
	 * @return the clamped and prepared motion vector ready for character controller
	 */
	public static Vector3 clampMotionVector(Vector3 to, Vector3 from, float speed, float boost, float speedBonus)
    {
		Vector3 motion = ((to - from).normalized) * boost;
		motion.y = 0; //no motion on the y axis!
		if (motion.x > 1) {
			motion.x = 1;
		}
		if (motion.x < -1) {
			motion.x = -1;
		}
		
		if (motion.z > 1) {
			motion.z = 1;
		}
		if (motion.z < -1) {
			motion.z = -1;
		}
		float limitFactor = (motion.x != 0.0f && motion.z != 0.0f) ? 0.7f+speedBonus : 1.0f;
		//Modify with speed etc
		var tmpx = ((motion.x * (speed+speedBonus)) * limitFactor * Time.deltaTime * 3);
		var tmpz = ((motion.z * (speed+speedBonus)) * limitFactor * Time.deltaTime * 3);
		
		//motion.x = Mathf.Clamp(motion.x, (motion.x * speed) * limitFactor * Time.deltaTime * 3, 0.4 + speedBonus);
		//motion.z = Mathf.Clamp(motion.z, (motion.z * speed) * limitFactor * Time.deltaTime * 3, 0.4 + speedBonus);
		motion.x = tmpx;
		motion.z = tmpz;
		//motion.z = Mathf.Min((motion.z * speed) * limitFactor * Time.deltaTime * 3, 0.4 + speedBonus);
		//Vector3.Min(motion * speedBonus, motion);
		return motion;
	}
	
	/**
	 * Use this to smoothly correct the client position with the server position
	 */
	public void lerpToTarget()
    {
		//Debug.Log("Distance between client-server position: " + Vector3.Distance(transform.position, serverPos));
		var distance = Vector3.Distance(transform.position, serverPos);
		//only correct if the error margin (the distance) is too extreme, saves some processing time
		
		if (distance >= positionErrorThreshold) {
			var lerp = ((1 / distance) * speed) / 100;
			//Debug.Log("Lerp time: " + lerp);
			transform.position = Vector3.Lerp(transform.position, serverPos, lerp);
			transform.rotation = Quaternion.Lerp(transform.rotation, serverRot, lerp);
		}
	}
	
	[RPC]
	public void GameOverOnClient()
    {
		isGameOver = true;
		bonusMan.showScores();
	}
	
	[RPC]
	public void setScoreListOnClient(string strlist)
    {
		bonusMan.setScoreTable(strlist.Split(";"[0]));
	}

    public void deactivate()
    {
        //Deactivates cameras and the like
        getCamera().SetActive(false);
    }
}
