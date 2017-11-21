using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float speedH = 2.0f;
	public float speedV = 2.0f;

	private float yaw = 0.0f;
	private float pitch = 0.0f;

	private bool lockedCursor = true;
	private float globalMultiplier = 1.0f;

	private Dictionary<string, Vector3> movementMap = new Dictionary<string, Vector3>();
	private List<string> globalMovementMap = new List<string>();

	private Dictionary<string, float> multiplierMap = new Dictionary<string, float>();

	public Texture crossHairTexture;

	public BoidController controller;

	public bool isWatching = false;
	public Boid watchingBoid;

	public bool lockOntoBoidRotation = false;

	void processKeyToCommand(bool compValue, Action command) {
		if (compValue)
			command();
	}

	void startSpectating() {
		Boid[] allBoids = controller.getAllBoids();

		// scna all boids for one whose halo is on
		foreach(Boid boid in allBoids) {
			if (boid.getHaloManager().isHaloOn) {
				boid.isBeingWatched = true;
				watchingBoid = boid;
				isWatching = true;
				break;
			}
		}

		// attempt to face the camera to the same direction as the selected boid
		//if (watchingBoid.rb.velocity != Vector3.zero)
		//	this.transform.rotation = Quaternion.LookRotation(watchingBoid.rb.velocity);
	}

	// Use this for initialization
	void Start () {
		movementMap.Add("w", Vector3.forward);
		movementMap.Add("s", Vector3.back);
		movementMap.Add("a", Vector3.left);
		movementMap.Add("d", Vector3.right);
		movementMap.Add("left shift", Vector3.down);
		movementMap.Add("space", Vector3.up);

		multiplierMap.Add("f1", 2.0f);
		multiplierMap.Add("f2", 0.5f);
		multiplierMap.Add("f3", 1.0f);
	}

	// Update is called once per frame
	void Update () {
		//change state of cursor if 'Escape' key is pressed once
		if (Input.GetKeyDown("`")) {
			lockedCursor = !lockedCursor;
		}

		if (lockedCursor) {
			//lock cursor to center of screen and hide it
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			// camera rotation controls (only active when cursor is locked)
			yaw += speedH * Input.GetAxis("Mouse X");
			pitch -= speedV * Input.GetAxis("Mouse Y");

			transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
		} else {
			//unlock cursor from center of screen and reveal it
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			return;
		}

		// toggle between first person view and third person view of BOIDS
		processKeyToCommand(!isWatching && Input.GetMouseButtonDown(0), startSpectating);
		/*if (!isWatching && Input.GetMouseButtonDown(0)) {
			startSpectating()
		}*/

		if (isWatching && Input.GetMouseButtonDown(1)) {
			watchingBoid.isBeingWatched = false;
			isWatching = false;
			watchingBoid = null;
		}

		if (Input.GetKeyDown("l"))
			lockOntoBoidRotation = !lockOntoBoidRotation;

		// light up the object the camera pointing at, if not in first person view
		if (!isWatching) {	
			Vector3 cameraCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
			RaycastHit hitInfo;
			if (Physics.Raycast(cameraCenter, transform.forward, out hitInfo, 10000)) {
				GameObject obj = hitInfo.transform.gameObject;
				if (obj.tag == "Boid") {
					Boid boid = obj.GetComponent<Boid>();
					boid.getHaloManager().TurnOnHaloFor(0.05f);
				}
			}
		}

		if (!isWatching) {
			// process modifier key commands
			foreach(KeyValuePair<string, float> pair in multiplierMap) {
				if (Input.GetKeyDown(pair.Key)) {
					globalMultiplier = pair.Value;
				}
			}

			// camera translation controls (similar to minecraft)
			float multiplier = Time.deltaTime * 50 * globalMultiplier;

			foreach(KeyValuePair<string, Vector3> pair in movementMap) {
				if (Input.GetKey(pair.Key)) {
					if (globalMovementMap.Contains(pair.Key)) {
						this.transform.Translate(pair.Value * multiplier, Camera.main.transform);
					} else {
						this.transform.Translate(pair.Value * multiplier);
					}
				}
			}
		} else {
			if (watchingBoid != null) {
				this.transform.position = watchingBoid.transform.position - watchingBoid.rb.velocity.normalized * 5;
				if (lockOntoBoidRotation && watchingBoid.rb.velocity != Vector3.zero)
					this.transform.rotation = Quaternion.LookRotation(watchingBoid.rb.velocity);
			}
		}
	}
}