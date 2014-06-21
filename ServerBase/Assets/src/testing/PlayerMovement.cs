using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    static float _motionVState = 0;
    static float _motionHState = 0;

    CharacterController _charControl = null;

    enum Axis
    {
        Vertical,
        Horizontal,
    }

    void Start()
    {
        Screen.lockCursor = true;
        _charControl = gameObject.GetComponent(typeof(CharacterController)) as CharacterController;
    }
	
	// Update is called once per frame
	void Update () {

        calculateMotionState(Axis.Vertical);
        calculateMotionState(Axis.Horizontal);
		Vector3 motion = transform.TransformDirection(new Vector3(_motionHState * Time.deltaTime, 0, _motionVState * Time.deltaTime));
        _charControl.Move(motion);

        float mouseMotionX = Input.GetAxis("Mouse X");
        transform.Rotate(new Vector3(0, 0.1f, 0), mouseMotionX);
	}

    static void calculateMotionState(Axis direction)
    {
        float motion = Input.GetAxis(direction.ToString());
        float motionState = 0;

        switch (direction)
        {
            case Axis.Vertical: motion *= 20 ; motionState = _motionVState; break;
            case Axis.Horizontal: motion *= 15; motionState = _motionHState; break;
        }

        if ((motion > 0 && motionState <= motion) ||
            (motion < 0 && motionState >= motion))
        {
            motionState = motion;
        }
        else if (motion >= 0 && motionState >= motion)
        {
            motionState -= 0.045f;
        }
        else if (motion <= 0 && motionState <= motion)
        {
            motionState += 0.045f;
        }
        else
        {
            motionState = 0;
        }

        switch (direction)
        {
            case Axis.Vertical: _motionVState = motionState; break;
            case Axis.Horizontal: _motionHState = motionState; break;
        }
    }
}