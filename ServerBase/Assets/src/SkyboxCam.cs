using UnityEngine;
using System.Collections;

/**
 * This class is based upon resources found here: http://forum.unity3d.com/threads/7998-Skybox-for-geometry-(AKA-3D-skybox)
 * It was modified to accomodate networking and stripped off some features that were unused.
 */
public class SkyboxCam : MonoBehaviour {

    public bool useRotation = true, useMovement = true;
    public float movementRatio = 0.01f;
    private Transform mainCam; //The transform for the main camera so we can adjust the rotation/movement 
    //Those 2 vectors have the origins of the skybox and the main scene cam stored.
    //It creates a distance relation used for parallax movement effects

    private Vector3 originPosition; //Where the skycam started at
    private Vector3 mainCamOriginPosition; //Where the main cam started
    private bool isPrepared;
	
	// Use this for initialization
	void Start () {
	
	}

    public void prepare(Transform playerCam)
    {
		mainCam = playerCam;
		mainCamOriginPosition = mainCam.position;
		originPosition = transform.position;
		isPrepared = true;
	}
    
    void Update(){
		if (!isPrepared || Network.isServer) {
			return;
		}
        if(useRotation) {
            transform.rotation = mainCam.rotation;
        }
        if(useMovement) {
            transform.position = originPosition + (mainCam.position - mainCamOriginPosition) * movementRatio;
        }
    }
}
