using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBackground : MonoBehaviour {

	public float RotationSpeed = 2.0f;
	// Use this for initialization

	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (transform.root.position, Vector3.forward, RotationSpeed *Time.deltaTime);
	}
}
