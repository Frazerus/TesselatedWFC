using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class WFCModelRunner : MonoBehaviour
{
    public enum TileSets
    {
        octagon_standardImitation_GrassOnlySquares,
        octagon_standardImitation_NormalSquares,

    }

    public int size = 50;
    public TileSets tileSet = TileSets.octagon_standardImitation_NormalSquares;
    public int seed = 37;
    public bool originalModel = false;
    public bool originalWithOctagons = false;
    public bool test = true;
    public int testRuns = 1000;
    [FormerlySerializedAs("Heuristic")] public Heuristic heuristic = Heuristic.MRV;


    // Start is called before the first frame update
    void Start()
    {
        if (test)
        {
            if (originalModel)
            {
                var model = new SimpleTiledModel(originalWithOctagons ? "standard_octagon" : "standard", null, size,
                    size, false,
                    heuristic, originalWithOctagons ? "OctagonSquares" : "standard", "./Assets/");

                var successes = 0;
                var workingSeed = new List<string>();
                for (int i = 0; workingSeed.Count < 40; i++)
                {
                    var seed = (int)(Random.value * 1379);
                    var successful = model.Run(seed, 10000);
                    if (successful) workingSeed.Add(seed.ToString());
                    successes += successful ? 1 : 0;
                }

                print($"Worked {successes} out of {testRuns} runs.");

                print(string.Join(", ", workingSeed.Take(40)));
            }
            else
            {
                var model = new SimpleOctagonTessellationModel(
                    tileSet.ToString(),
                    size,
                    size,
                    false,
                    heuristic,
                    2,
                    "Assets");

                var successes = 0;
                var workingSeed = new List<string>();
                for (int i = 0; workingSeed.Count < 40; i++)
                {
                    var seed = (int)(Random.value * 1379);
                    var successful = model.Run(seed, 10000);
                    if (successful) workingSeed.Add(seed.ToString());
                    successes += successful ? 1 : 0;
                }

                print($"Worked {successes} out of {testRuns} runs.");

                print(string.Join(", ", workingSeed.Take(40)));

                var a = new int[40];
                for (int i = 0; i < a.Length; i++)
                {
                    a[i] = i;
                }
                print(string.Join(", ", a));

                return;
            }


        }



        if (originalModel)
        {
            var model = new SimpleTiledModel(originalWithOctagons ? "standard_octagon" : "standard", null, size, size, false,
                heuristic, originalWithOctagons ? "OctagonSquares" : "standard", "./Assets/");

            var output = model.Run(seed, 10000);

            if (!output)
                print("Didnt work.");
            else
                print("Worked!");

            model.Save();
        }
        else
        {
            var model = new SimpleOctagonTessellationModel(
                tileSet.ToString(),
                size,
                size,
                false,
                heuristic,
                2,
                "Assets");

            var output = model.Run(seed, 10000);

            if (!output)
                print("Didnt work.");
            else
                print("Worked!");

            model.CreateOutput();
        }
    }
}
/*
    void CreateOctagonSquaredMap()
    {
        var root = XDocument.Load($"Assets/octagon.xml");

        var weightList = new List<double>[shapeCount];

        var tiles = new List<Tile>[shapeCount];
        var tilenames = new List<string>[shapeCount];

        var action = new List<int[]>[shapeCount];
        var firstOccurrence = new Dictionary<string, int>[shapeCount];

        var totalPossibleStates = new int[shapeCount];

        for (int i = 0; i < shapeCount; i++)
        {
            weightList[i] = new List<double>();

            tiles[i] = new List<Tile>();
            tilenames[i] = new List<string>();


            action[i] = new List<int[]>();
            firstOccurrence[i] = new Dictionary<string, int>();
        }
    }
}
*/