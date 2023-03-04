using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class CarDetectorScript : MonoBehaviour {

	public float angle = 360;
	public bool ApplyThresholds, ApplyLimits;
	public float MinX, MaxX, MinY, MaxY;
	private bool useAngle = true;

	public float output;
	public int numObjects;

	void Start()
	{
		output = 0;
		numObjects = 0;

		if (angle > 360)
		{
			useAngle = false;
		}
	}

	void Update()
	{
		GameObject[] cars;
		GameObject closestCar;
		float max;

		if (useAngle) {
			cars = GetVisibleCars();
		} else {
			cars = GetAllCars();
		}
		numObjects = cars.Length;
		closestCar = cars[0];
		max = (transform.position - closestCar.transform.position).sqrMagnitude;
		max = 1.0f / (max + 1);
		output = 0;

		foreach (GameObject car in cars) {
			// print (1 / (transform.position - car.transform.position).sqrMagnitude);

			float distance = (transform.position - car.transform.position).sqrMagnitude;
			output = 1.0f / (distance + 1);

			if (output > max)
			{
				closestCar = car;
				max = output;
			}

			Debug.DrawLine (transform.position, closestCar.transform.position, Color.green);
		}
		output = max;
	}

	public virtual float GetOutput() { throw new NotImplementedException(); }

	// Returns all "Car" tagged objects. The sensor angle is not taken into account.
	GameObject[] GetAllCars()
	{
		return GameObject.FindGameObjectsWithTag("CarToFollow");
	}

	GameObject[] GetVisibleCars()
	{
		ArrayList visibleCars = new ArrayList();
		float halfAngle = angle / 2.0f;

		GameObject[] cars = GetAllCars();

		foreach (GameObject car in cars) {
			Vector3 toVector = (car.transform.position - transform.position);
			Vector3 forward = transform.forward;
			toVector.y = 0;
			forward.y = 0;
			float angleToTarget = Vector3.Angle (forward, toVector);

			if (angleToTarget <= halfAngle) {
				visibleCars.Add (car);
			}
		}

		return (GameObject[])visibleCars.ToArray(typeof(GameObject));
	}
}
