using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;

public class GazeGestureManager : MonoBehaviour {

    public static GazeGestureManager Instance { get; private set; }

    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }

	//Photo capture stuff
	Texture2D targetTexture = null;

    UnityEngine.XR.WSA.Input.GestureRecognizer recognizer;

    // Use this for initialization
    void Start()
    {
        Instance = this;

		// Create a PhotoCapture object
		UnityEngine.XR.WSA.WebCam.PhotoCapture photoCaptureObject = captureObject;
		UnityEngine.XR.WSA.WebCam.CameraParameters cameraParameters = new UnityEngine.XR.WSA.WebCam.CameraParameters();
		cameraParameters.hologramOpacity = 0.0f;
		cameraParameters.cameraResolutionWidth = cameraResolution.width;
		cameraParameters.cameraResolutionHeight = cameraResolution.height;
		cameraParameters.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.BGRA32;

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray, photoCapture) =>
        {
			// Activate the camera
			photoCapture.StartPhotoModeAsync(cameraParameters, delegate (UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result) {
				// Take a picture
				photoCapture.TakePhotoAsync(OnCapturedPhotoToMemory);
			});      
        };
        recognizer.StartCapturingGestures();
    }

    // Update is called once per frame
    void Update()
    {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;

        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
        }
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
    }

	void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {
		// Copy the raw image data into the target texture
		photoCaptureFrame.UploadImageDataToTexture(targetTexture);

		// Create a GameObject to which the texture can be applied
		GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;
		quadRenderer.material = new Material(Shader.Find("Custom/Unlit/UnlitTexture"));

		quad.transform.parent = this.transform;
		quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);

		quadRenderer.material.SetTexture("_MainTex", targetTexture);

		// Deactivate the camera
		photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
	}

	void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result) {
		// Shutdown the photo capture resource
		photoCaptureObject.Dispose();
		photoCaptureObject = null;
	}
}
