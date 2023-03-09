using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class BlockDetectorLinearScript : BlockDetectorScript {

    public override float GetOutput()
    {
        return 1.0f - output;
    }
}
