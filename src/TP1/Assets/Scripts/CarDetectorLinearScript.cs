using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class CarDetectorLinearScript : CarDetectorScript {

	public bool inverse = false;
	// Get gaussian output value

	public override float GetOutput()
	{
		return 1.0f - output;
	}





}
