using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public float zoomSens = 1f;
	public float rotateSens = 400.0f;
    public Vector3 originPosition;
    public GameController gc;
    public Quaternion sharpRotation, roundRotation;
    public Transform originParent;
    bool isLastSharp, isTranslating;
    float elapsed, startTime;
    float rotationX, rotationY;
    Vector3 directionToOrigin, startPosition;
    Quaternion startRotation;
    public float transitionTime = 1f;

    void Start() {
        isLastSharp = true;
        if(gc.getWhosTurn() == StoneShape.Round) {
            originParent.rotation = new Quaternion(0,1,0,0);
            isLastSharp = false;
        }
    }

    void Update() {

        if(isTranslating) { // slerp the camera back to its spot
            elapsed = (Time.time - startTime)/transitionTime;
            if(elapsed > 1) {
                isTranslating = false;
            } else if(isLastSharp) {
                transform.localPosition = Vector3.Slerp(startPosition, originPosition, elapsed);
                originParent.rotation = Quaternion.Slerp(startRotation, sharpRotation, elapsed);
            } else {
                transform.localPosition = Vector3.Slerp(startPosition, originPosition, elapsed);
                originParent.rotation = Quaternion.Slerp(startRotation, roundRotation, elapsed);
            }
        } else {
            // Zoom Camera w/ scrollwheel
            float zoomInput = Input.GetAxis("Mouse ScrollWheel");
            directionToOrigin = originParent.position - transform.position;
    		transform.Translate(directionToOrigin * zoomInput * zoomSens, Space.World);

            // rotate camera w/ middle mouse down
            if (Input.GetMouseButton (2)) {
    			rotationX = Input.GetAxis ("Mouse X") * rotateSens * Time.deltaTime;
    			rotationY = Input.GetAxis ("Mouse Y") * rotateSens * Time.deltaTime;
    			originParent.localEulerAngles += new Vector3 (rotationY, rotationX, 0);
    		}

            if (Input.GetButtonUp("Reset")) { // press r to transform the camera back to the starting locaiton
                if(isLastSharp) {
                    setSharpCam();
                } else {
                    setRoundCam();
                }
            }
        }


    }

    public void setSharpCam() { // entry for slerp to sharp camera spot
        isLastSharp = true;
        startPosition = transform.localPosition;
        startRotation = originParent.rotation;
        startTime = Time.time;
        isTranslating = true;
    }

    public void setImmediateSharp() { // no slerp, just go right to the sharp camera spot
        transform.localPosition = originPosition;
        originParent.rotation = sharpRotation;
    }

    public void setRoundCam() { // entry for slerp to round camera spot
        isLastSharp = false;
        startPosition = transform.localPosition;
        startRotation = originParent.rotation;
        isTranslating = true;
        startTime = Time.time;
    }


}
