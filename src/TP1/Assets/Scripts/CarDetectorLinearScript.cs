using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class CarDetectorLinearScript : CarDetectorScript {

	public bool inverse = false;

	// return the output value between limits
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

	// return the inverted output value
	private float GetInvertedOutput() 
	{
		float energy = MinY;
		if ((1.0f-MaxX) <= output && output <= (1.0f-MinX))
		{
			energy = 1.0f - output;
			if (energy >= MaxY)
				energy = MaxY;
			else if (energy <= MinY)
				energy = MinY;
		}
		return energy;
	}

	public override float GetOutput()
	{
		float energy = output;
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
			print(energy);
		}
		return energy;
	}




}
