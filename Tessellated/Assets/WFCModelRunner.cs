using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WFCModelRunner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var model = new SimpleOctagonTessellationModel("octagon", 10, 10, false, OctagonTessellationModel.Heuristic.MRV, 2);

        var output = model.Run(37, 10000);

        if (!output)
        {
            print("Didnt work.");
        }
        else
        {
            print("Worked!");
            model.Save();
        }
    }
}