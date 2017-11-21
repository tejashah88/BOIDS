using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour, IEquatable<Boid> {
	public float maxForce, maxSpeed;

	public Rigidbody rb;

	public bool debug = false;
	public bool isBeingWatched = false;

	public int id;

	private HaloManager haloManager;

	void Start() {
		rb = GetComponent<Rigidbody>();
		haloManager = new HaloManager(this);
		rb.velocity = Vector3.zero;
		//rb.velocity = Vector3.one * UnityEngine.Random.Range(-5.0f, 5.0f);//new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 0, UnityEngine.Random.Range(-5.0f, 5.0f));
	}

	public HaloManager getHaloManager() {
		return haloManager;
	}

	// Update is called once per frame
	void Update() {
		if (rb.velocity != Vector3.zero)
			this.transform.rotation = Quaternion.LookRotation(rb.velocity);

		haloManager.ProcessHaloFrame(Time.deltaTime);

		GameObject boundsSphere = this.transform.Find("Boundaries").gameObject;
		Transform boundsTransform = boundsSphere.transform;
		Renderer sphereRenderer = boundsSphere.GetComponent<Renderer>();

		if (isBeingWatched) {
			sphereRenderer.material.color = new Color(255f/255f, 30.0f/255.0f, 0f, 50f/255f); // faint red
			boundsTransform.localScale = Vector3.one * 2f;
		} else {
			sphereRenderer.material.color = new Color(0f, 200f/255f, 255f/255f, 50f/255f); // faint blue
			boundsTransform.localScale = Vector3.one * 0.5f;
		}
	}

	#region compare-operations
		public bool Equals(Boid other) {
			if (other == null)
				return false;

			return this.id == other.id;
		}

		public override bool Equals(System.Object obj) {
			if (obj == null)
				return false;

			Boid boidObj = obj as Boid;
			return (boidObj == null) ? false : Equals(boidObj);
		}

		public override int GetHashCode() {
			return this.id;
		}

		public static bool operator == (Boid boid1, Boid boid2) {
			if (((object)boid1) == null || ((object)boid2) == null)
				return System.Object.Equals(boid1, boid2);

			return boid1.Equals(boid2);
		}

		public static bool operator != (Boid boid1, Boid boid2) {
			if (((object)boid1) == null || ((object)boid2) == null)
				return !System.Object.Equals(boid1, boid2);

			return !boid1.Equals(boid2);
		}
	#endregion

	/*void FixedUpdate() {
		
	}*/

	/*public void showAlignmentDebug(Vector3 velocity) {
		Debug.DrawRay(transform.localPosition, velocity, Color.blue);
	}

	public void showCohesionDebug(Vector3 velocity) {
		Debug.DrawRay(transform.localPosition, velocity, Color.green);
	}

	public void showSeparationDebug(Vector3 velocity) {
		Debug.DrawRay(transform.localPosition, velocity, Color.red);
	}*/
}