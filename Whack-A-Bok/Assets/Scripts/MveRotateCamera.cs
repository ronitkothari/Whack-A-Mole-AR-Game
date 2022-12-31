using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MveRotateCamera : MonoBehaviour {
    public GameObject reference;
	// Update is called once per frame
	void Update () {

        transform.RotateAround(reference.transform.position, Vector3.up, 20 * Time.deltaTime);
		
	}
}
