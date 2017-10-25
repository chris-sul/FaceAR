using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCommands : MonoBehaviour {

	void onSelect() {
		if (!this.GetComponent<Rigidbody> ()) {
			var rigidbody = this.gameObject.AddComponent<Rigidbody> ();
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
