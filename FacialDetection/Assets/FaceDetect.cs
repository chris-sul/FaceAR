using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Emgu.CV;

public class FaceDetect : MonoBehaviour {
	
	private CascadeClassifier haarCascade;

	// Use this for initialization
	void Start () {
		CascadeClassifier Classifier = new CascadeClassifier("Assets/haarcascade_frontalface_default.xml");
		print ("here");

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
