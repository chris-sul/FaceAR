using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCursor : MonoBehaviour {

	private MeshRenderer meshRenderer;

	// Use this for initialization
	void Start () {
		//Grab the mesh rendere thats on the same object as this script
		meshRenderer = this.gameObject.GetComponentsInChildren<MeshRenderer>()
	}
	
	// Update is called once per frame
	void Update () {
		//Do a raycast into the world based on the users head position and orientation.
		var headPosition = Camera.main.transform.position;
		var gazeDirection = Camera.main.transform.forward;

		RaycastHit hitInfo;

		if (Physics.Raycast (headPosition, gazeDirection, out hitInfo)) {
			//If the raycaster hit a hologram 
		}
	}
}
