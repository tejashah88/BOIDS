using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour {
	public float radius;

	// Use this for initialization
	void Start () {
		this.transform.localScale = Vector3.one * radius;
	}

	public void setPosition(Vector3 pos) {
		this.transform.position = pos;
	}

	public void setRadius(float r) {
		this.radius = r;
		this.transform.localScale = Vector3.one * this.radius;
	}
}
