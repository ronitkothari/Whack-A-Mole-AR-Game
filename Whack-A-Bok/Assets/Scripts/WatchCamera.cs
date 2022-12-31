using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchCamera : MonoBehaviour {

    Camera firstPersonCamera;
    // Use this for initialization
    void Start () {
        firstPersonCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 cameraPositionSameY = firstPersonCamera.transform.position;
        cameraPositionSameY.y = gameObject.transform.position.y;
        Debug.Log("Position of Camera "+firstPersonCamera.transform.position);
        gameObject.transform.LookAt(cameraPositionSameY);

    }
}
