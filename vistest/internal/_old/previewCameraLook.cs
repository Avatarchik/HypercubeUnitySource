using UnityEngine;
using System.Collections;

public class previewCameraLook : MonoBehaviour {

    public float keyboardLookSpeed = 30f;
    public float sensitivityX = 30f;
    public float sensitivityY = 30f;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationX = 0f;
    float rotationY = 0f;
    Quaternion originalRotation;

    bool invertMouse = false;

	void Start () {
        originalRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () 
    {
        //mouse look
        float xLook = 0;
        float yLook = 0;

        if (Input.GetKey(KeyCode.Mouse2) || Input.GetKey(KeyCode.Mouse1))
        {
            xLook = Input.GetAxis("Mouse X");
            yLook = Input.GetAxis("Mouse Y");
        }

        if (Input.GetKey(KeyCode.RightArrow))
            xLook = keyboardLookSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.LeftArrow))
            xLook = -keyboardLookSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow))
            yLook = keyboardLookSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.DownArrow))
            yLook = -keyboardLookSpeed * Time.deltaTime;


        if (invertMouse)
            yLook = -yLook;

        if (xLook != 0 || yLook != 0)
        {
            rotationX += xLook * sensitivityX * Time.deltaTime;
            rotationY += yLook * sensitivityY * Time.deltaTime;
            rotationX = hypercubeFpsControl.ClampAngle(rotationX, minimumX, maximumX);
            rotationY = hypercubeFpsControl.ClampAngle(rotationY, minimumY, maximumY);


            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            //moveNode.Rotate(yLook * sensitivityX * Time.deltaTime, xLook * sensitivityX * Time.deltaTime, roll * Time.deltaTime);
        }

        //other
        if (Input.GetKeyDown(KeyCode.I))
            invertMouse = !invertMouse;
	}
}
