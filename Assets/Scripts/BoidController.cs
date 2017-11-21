using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoidController : MonoBehaviour {
	public Boid boidPrefab;
	public Beacon beaconPrefab;

	private Beacon targetBeacon;

	public int numBoids;

	public float maxSpeed;
	public float maxForce;

	public float alignmentRadius;
	public float cohesionRadius;
	public float separationRadius;

	public float alignmentFactor = 1;
	public float cohesionFactor = 1;
	public float separationFactor = 1;
	public float targetLocationFactor = 1;

	private Boid[] allBoids;

	public Text txtDebugMain;
	public Text txtDebugMisc;

	public bool createBeacon;

	// Use this for initialization
	void Start () {
		initSimulation();
	}

	void Update() {
		// reset simulation w/o manual restarting via 'R' key
		if (Input.GetKeyDown("r")) {
			deinitSimulation();
			initSimulation();
		}
	}

	void initSimulation() {
		allBoids = new Boid[numBoids];

		for (int i = 0; i < numBoids; i++) {
			Boid boid = Instantiate(boidPrefab, transform.position + (Random.onUnitSphere * Random.Range(0, 20)), Quaternion.identity) as Boid;
			boid.transform.parent = transform;

			boid.id = i;
			//boid.debug = (i == 0);

			allBoids[i] = boid;
		}

		if (createBeacon) {
			generateBeacon();
		}
	}

	void generateBeacon() {
		targetBeacon = Instantiate(beaconPrefab, transform.position + Vector3.one * 250, Quaternion.identity) as Beacon;
		targetBeacon.setRadius(500.0f);
	}

	void destroyAllGameObjects(GameObject[] objs) {
		for(int i = 0; i < objs.Length; i++) {
			DestroyImmediate(objs[i]);
		}
	}

	void deinitSimulation() {
		GameObject[] boidsToRemove = GameObject.FindGameObjectsWithTag("Boid");
		destroyAllGameObjects(boidsToRemove);

		GameObject[] beaconToRemove = GameObject.FindGameObjectsWithTag("Beacon");
		destroyAllGameObjects(beaconToRemove);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float dTime = Time.deltaTime;
		foreach(Boid boid in allBoids) {
			if (boid != null && boid.rb != null) {
				Vector3 alignmentForce = calcAlignmentForce(boid) * alignmentFactor * dTime;
				Vector3 cohesionForce = calcCohesionForce(boid) * cohesionFactor * dTime;
				Vector3 separationForce = calcSeparationForce(boid) * separationFactor * dTime;
				Vector3 targetToBeaconForce = calcSeekToBeaconForce(boid) * targetLocationFactor * dTime;//calcSeekTowardForce(boid, Vector3.one * 250) * targetLocationFactor * dTime;

				Vector3 totalAdjustmentForce = alignmentForce + cohesionForce + separationForce + targetToBeaconForce;

				if (boid.isBeingWatched) {
					debugValues(
						txtDebugMain,
						new string[] { "ID", "Alignment Force", "Cohesion Force", "Separation Force", "Target Location Force",
													 "Position", "Velocity", "Velocity (Magnitude)", "Total Adjustment Force" },
						new object[] { boid.id, alignmentForce, cohesionForce, separationForce, targetToBeaconForce,
													 boid.transform.position, boid.rb.velocity, boid.rb.velocity.magnitude, totalAdjustmentForce }
					);
				}				

				boid.rb.AddForce(totalAdjustmentForce, ForceMode.Force);

				float velMag = boid.rb.velocity.magnitude;
				// limit if beacon present
				if (createBeacon) {
					float dist = Vector3.Distance(targetBeacon.transform.position, boid.transform.position);
					
					if (dist < targetBeacon.radius) {
						float trueMag = dist / 20;//map(dist, 0, 100, 0, maxSpeed);
						if (velMag > trueMag)
							boid.rb.velocity = boid.rb.velocity.normalized * trueMag;
					} else {
						if (velMag > maxSpeed)
							boid.rb.velocity = boid.rb.velocity.normalized * maxSpeed;
					}
				} else {
					if (velMag > maxSpeed)
						boid.rb.velocity = boid.rb.velocity.normalized * maxSpeed;
				}
			}
		}
	}

	public void debugValues(Text txtDebug, string[] keys, object[] values) {
		string result = "";
		if (keys.Length != values.Length) {
			Debug.Log("Error: keys length and values length are NOT equal.");
			return;
		}

		for (int i = 0; i < keys.Length; i++) {
			result += keys[i] + ": " + values[i] + "\n";
		}

		result.Trim();

		txtDebug.text = result;
	}

	public float map(float input, float from1, float to1, float from2, float to2){
		if(input <= from2) {
			return from1;
		} else if(input >= to2) {
			return to1;
		} else {
			return (to1 - from1) * ((input - from2) / (to2 - from2)) + from1;
		}
	}

	public Boid[] getAllBoids() {
		return allBoids;
	}

	public Boid[] getAllBoidsExcept(Boid boid) {
		List<Boid> listAllBoids = allBoids.ToList<Boid>();
		listAllBoids.Remove(boid);
		return listAllBoids.ToArray();
	}

	public Boid[] getNearbyBoids(Boid boid, float radius) {
		List<Boid> neighbors = new List<Boid>();
		Collider[] boidsInRange = Physics.OverlapSphere(boid.transform.position, radius);

		foreach(Collider boidCollider in boidsInRange) {
			// check if the collider refers to an actual boid AND that the collider is NOT refering to the original boid
			if (boidCollider.GetType() == typeof(SphereCollider) && boidCollider.gameObject != boid.gameObject) {
				neighbors.Add(boidCollider.gameObject.GetComponent<Boid>());
			}
		}

		return neighbors.ToArray();
	}

	public Vector3 limitVectorRange(Vector3 input, float minMag, float maxMag) {
		float magnitude = input.magnitude;

		if (magnitude < minMag) {
			return input.normalized * minMag;
		} else if (magnitude > maxMag) {
			return input.normalized * maxMag;
		} else {
			return input;
		}
	}

	public bool inRange(float target, float min, float max) {
		return target > min && target < max;
	}

	public Vector3 setMagnitude(Vector3 input, float targetMagnitude, bool modifyVector = false) {
		if (modifyVector) {
			input.Normalize();
			return input * targetMagnitude;
		} else {
			return input.normalized * targetMagnitude;
		}
	}

	public Vector3 calcAlignmentForce(Boid boid) {
		Boid[] allNearbyBoids = getAllBoids();
		Vector3 avgVelocity = Vector3.zero;
		int count = 0;

		foreach(Boid neighbor in allNearbyBoids) {
			float dist = Vector3.Distance(neighbor.transform.position, boid.transform.position);

			if (inRange(dist, 0, alignmentRadius)) {
				avgVelocity += neighbor.rb.velocity;
				count++;
			}
		}

		if (count > 0) {
			avgVelocity /= count;
			avgVelocity.Normalize();
			avgVelocity *= maxSpeed;

			Vector3 steer = avgVelocity - boid.rb.velocity;
			steer = Vector3.ClampMagnitude(steer, maxForce);

			return steer;
		} else {
			return Vector3.zero;
		}
	}

	// birds move towards center of flock
	public Vector3 calcCohesionForce(Boid boid) {
		Boid[] allNearbyBoids = getAllBoids();
		Vector3 centerOfMass = Vector3.zero;
		int count = 0;

		foreach(Boid neighbor in allNearbyBoids) {
			float dist = Vector3.Distance(neighbor.transform.position, boid.transform.position);

			if (inRange(dist, 0, cohesionRadius)) {
				centerOfMass += neighbor.transform.position;
				count++;
			}
		}

		if (count > 0) {
			centerOfMass /= count;

			return seek(boid, centerOfMass);
		} else {
			return Vector3.zero;
		}
	}

	public Vector3 calcSeparationForce(Boid boid) {
		Boid[] allNearbyBoids = getAllBoids();
		Vector3 neededSeparation = Vector3.zero;
		int count = 0;

		foreach(Boid neighbor in allNearbyBoids) {
			float dist = Vector3.Distance(neighbor.transform.position, boid.transform.position);

			if (inRange(dist, 0, separationRadius)) {
				Vector3 diff = boid.transform.position - neighbor.transform.position;
				diff.Normalize();
				diff /= dist;
				neededSeparation += diff;
				count++;
			}
		}

		if (count > 0) {
			neededSeparation /= count;
			neededSeparation.Normalize();
			neededSeparation *= maxSpeed;

			Vector3 steer = neededSeparation - boid.rb.velocity;
			steer = Vector3.ClampMagnitude(steer, maxForce);

			return steer;
		} else {
			return Vector3.zero;
		}
	}

	public Vector3 seek(Boid boid, Vector3 targetPosition) {
		Vector3 desired = targetPosition - boid.transform.position;
		desired.Normalize();
		desired *= maxSpeed;
		Vector3 steer = desired - boid.rb.velocity;
		steer = Vector3.ClampMagnitude(steer, maxForce);
		return steer;
	}

	public Vector3 calcSeekTowardForce(Boid boid, Vector3 position) {
		float dist = Vector3.Distance(position, boid.transform.position);
		Vector3 targetLocationForce = (position - boid.transform.position) / dist;
		return targetLocationForce;
	}

	public Vector3 calcSeekToBeaconForce(Boid boid) {
		if (createBeacon) {
			Vector3 desired = targetBeacon.transform.position - boid.transform.position;
			float dist = Vector3.Distance(targetBeacon.transform.position, boid.transform.position);
			desired.Normalize();

			if (dist < targetBeacon.radius) {
				float trueMag = map(dist, 0, 100, 0, maxSpeed / 5);
				desired *= trueMag;
			} else {
				desired *= maxSpeed;
			}

			Vector3 steer = desired - boid.rb.velocity;
			steer = Vector3.ClampMagnitude(steer, maxForce);
			return /*seek(boid, targetBeacon.transform.position);*/steer;
		} else {
			return Vector3.zero;
		}
	}
}
