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
        var model = new SimpleTiledModel("standard", null, 10, 10, false, Model.Heuristic.Entropy);




        var output = model.Run(37, 10000);

        if (!output)
        {
            print("Didnt work.");
        }
        else
        {
            print("Worked!");
            model.Save("asdf");
        }
    }
}