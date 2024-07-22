using UnityEngine;
using UnityEngine.Serialization;

public class WFCModelRunner : MonoBehaviour
{
    public enum TileSets
    {
        octagon_standardImitation_GrassOnlySquares,
        octagon_standardImitation_NormalSquares,

    }

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
            var model = new SimpleOctagonTessellationModel(
                tileSet.ToString(),
                10,
                10,
                false,
                heuristic,
                2);

            var success = 0;
            for (int i = 0; i < testRuns; i++)
            {
                success += model.Run((int)(Random.value * 1379), 10000) ? 1 : 0;
            }

            print($"Worked {success} out of {testRuns} runs.");
            return;
        }



        if (originalModel)
        {
            var model = new SimpleTiledModel(originalWithOctagons ? "standard_octagon" : "standard", null, 10, 10, false,
                Model.Heuristic.MRV, originalWithOctagons ? "OctagonSquares" : "standard");

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
                10,
                10,
                false,
                heuristic,
                2);

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