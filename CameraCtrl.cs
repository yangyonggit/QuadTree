using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraCtrl : MonoBehaviour {
	public float speed = 1.0f;
	public float turnspeed = 1.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//speed = GameObject.Find ("cameraSpeedSlider").GetComponent<Slider> ().value;
		if (Input.GetKey (KeyCode.W)) {
			transform.position += transform.forward * speed;
		}

		if (Input.GetKey (KeyCode.S)) {
			transform.position -= transform.forward * speed;
		}

		if (Input.GetKey (KeyCode.A)) {
			transform.position -= transform.right * speed;
		}

		if (Input.GetKey (KeyCode.D)) {
			transform.position += transform.right * speed;
		}

		if (Input.GetMouseButton (0)) {
			float x = Input.GetAxis ("Mouse X");
			float y = Input.GetAxis ("Mouse Y");

			transform.eulerAngles += new Vector3 (0, x * turnspeed, 0);
			//Debug.Log (transform.eulerAngles.x);
			transform.eulerAngles += new Vector3 (-y * turnspeed, 0, 0);		
		}	
	}
}
