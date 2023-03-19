using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class LightDetectorGaussScript : LightDetectorScript {

	public float stdDev = 1.0f; 
	public float mean = 0.0f; 
	public bool inverse = false;

	private float GetGaus() 
	{
		float eExpoent = ((float) -Math.Pow(output-mean, 2)) / (float) (2.0f * Math.Pow(stdDev, 2));
		float fDivision = 1.0f / (float) (stdDev * Math.Sqrt(2.0f * Math.PI));
		float value = fDivision * (float) Math.Pow(Math.E, eExpoent);

		return value;
	}

	private float GetLimiarLinearOutput() 
	{
		float energy = 0.0f;
		if (MinX <= output && output <= MaxX)
		{
			energy = output;
		}
		return energy;
	}

	private float GetThreasholdLinearOutput(float energy) 
	{
		if (energy >= MaxY)
			energy = MaxY;
		else if (energy <= MinY)
			energy = MinY;

		return energy;
	}

	// Get gaussian output value
	public override float GetOutput()
	{	
		// to take a circle:
		// right: std dev = 0.5; mean = 0.12
		// left: std dev = 1.0; mean 0.0
		float energy = GetGaus();
		if (ApplyLimits) 
		{
			energy = GetLimiarLinearOutput();
		}
		if (ApplyThresholds)
		{
			energy = GetThreasholdLinearOutput(energy);
		}
		if (inverse)
		{
			energy = 1.0f - energy;
		}
		print(energy);
		return energy;
	}
}
