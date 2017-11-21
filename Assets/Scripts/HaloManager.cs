using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloManager {
	private Boid parentBoid;
	private Behaviour halo;

	private float timeCounter;
	private float seconds;

	public bool isStayingOn;
	public bool isHaloOn;

	public HaloManager(Boid pBoid) {
		parentBoid = pBoid;
		halo = (Behaviour) parentBoid.GetComponent("Halo");
		timeCounter = 0;
		seconds = 0;
		isStayingOn = false;
		isHaloOn = false;
	}

	public void TurnHaloOff() {
		halo.enabled = false;
		isHaloOn = false;
	}

	public void TurnHaloOn() {
		halo.enabled = true;
		isHaloOn = true;
	}

	public void TurnOnHaloFor(float seconds) {
		timeCounter = 0;
		TurnHaloOn();
		isStayingOn = true;
		this.seconds = seconds;
	}

	public void ProcessHaloFrame(float dTime) {
		timeCounter += dTime;
		if (isStayingOn && this.seconds < timeCounter) {
			isStayingOn = false;
			TurnHaloOff();
		}
	}
}