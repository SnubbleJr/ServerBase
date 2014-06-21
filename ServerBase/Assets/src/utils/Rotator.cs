using UnityEngine;
using System.Collections;

/**
 * Rotate the attached object around its y axis and do some up and down bobbing.
 * Both can be used separately or together
 */
public class Rotator : MonoBehaviour {
    
    public float maxBobbleY = 1f; //max up/down movement bobble
    public float moveFactor = 2f; //how fast the indicator bobbles up and down
	public float spinSpeed = 0.3f;
	
	public bool  useBobbing;
	public bool  useRotation = true;
	
	
    private float currentRotation;
    private float maxOffset;
    private float minOffset;
    
    private enum Moving {
        UP, DOWN
    };
    private Moving currentMoving = Moving.UP;
	
	public void  Start (){
		maxOffset = (transform.position.y + maxBobbleY);
        minOffset = (transform.position.y - maxBobbleY);
	}
    
    public void  Update (){
		//Move up and down
        //process up/down bobble
        Vector3 newPos;

		if (useBobbing) {
			if(currentMoving == Moving.UP) {
				if(transform.position.y < maxOffset) {
                    newPos = transform.position;
                    newPos.y += moveFactor * Time.deltaTime;
                    transform.position = newPos;
				}
				if(transform.position.y >= maxOffset) {
					currentMoving = Moving.DOWN;
				}
			}
			
			if(currentMoving == Moving.DOWN) {
				if(transform.position.y > minOffset) {
                    newPos = transform.position;
                    newPos.y -= moveFactor * Time.deltaTime;
                    transform.position = newPos;
				}
				
				if(transform.position.y <= minOffset) {
					currentMoving = Moving.UP;
				}
			}
		} 
		//END move up and down
		
		//Rotate around
		if (useRotation) {
			float nx = transform.rotation.eulerAngles.x;
			float ny = transform.rotation.eulerAngles.y;
			float nz = transform.rotation.eulerAngles.z;
			
			ny += spinSpeed * Time.deltaTime;
			if (ny >= 360) {
				ny = 0;
			}
			transform.rotation = Quaternion.Euler(nx, ny, nz);
		}
    }
}