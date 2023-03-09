using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class BlockDetectorScript : MonoBehaviour {

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
        GameObject[] blocks;
        float closestBlock = 200.0f;

        if (useAngle) {
            blocks = GetVisibleBlocks();
        } else {
            blocks = GetAllBlocks();
        }

        output = 0;
        numObjects = blocks.Length;

		// detect the closest block
        foreach (GameObject block in blocks)
        {
            // formula da energia
            float distance = (transform.position - block.transform.position).sqrMagnitude;
            if (distance < closestBlock) 
            {
                closestBlock = distance;
            }
            // Debug.DrawLine(transform.position, block.transform.position, Color.green);
        }
        output = 1.0f / closestBlock;
    }

    public virtual float GetOutput() { throw new NotImplementedException(); }

    // Returns all "Block" tagged objects. The sensor angle is not taken into account.
    GameObject[] GetAllBlocks()
    {
        return GameObject.FindGameObjectsWithTag("Block");
    }

    // Returns all "Block" tagged objects that are within the view angle of the Sensor. 
    // Only considers the angle over the y axis. Does not consider objects blocking the view.
    GameObject[] GetVisibleBlocks()
    {
        ArrayList visibleBlocks = new ArrayList();
        float halfAngle = angle / 2.0f;

        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");

        foreach (GameObject block in blocks)
        {
            Vector3 toVector = (block.transform.position - transform.position);
            Vector3 forward = transform.forward;
            toVector.y = 0;
            forward.y = 0;
            float angleToTarget = Vector3.Angle(forward, toVector);

            if (angleToTarget <= halfAngle)
            {
                visibleBlocks.Add(block);
            }
        }

        return (GameObject[])visibleBlocks.ToArray(typeof(GameObject));
    }
}