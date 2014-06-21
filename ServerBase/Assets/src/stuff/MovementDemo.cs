using UnityEngine;
using System.Collections;

public class MovementDemo : MonoBehaviour {

    public CharacterController controller;
    public float speed = 20;

	// Use this for initialization
	void Start () {
        controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 vec = new Vector3(Input.GetAxis("Horizontal") * speed * Time.deltaTime, 0, Input.GetAxis("Vertical") * speed * Time.deltaTime);
        controller.Move(vec);
	}
}
