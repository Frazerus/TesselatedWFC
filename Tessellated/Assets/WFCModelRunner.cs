using UnityEngine;

public class WFCModelRunner : MonoBehaviour
{
    public int seed = 37;
    public bool originalModel = false;
    public bool originalWithOctagons = false;


    // Start is called before the first frame update
    void Start()
    {
        if (originalModel)
        {
            var name = "standard";
            if (originalWithOctagons)
            {

                name = "octagon_squared_created";

            }

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
            var model = new SimpleOctagonTessellationModel("octagon_wholeSquares", 10, 10, false,
                OctagonTessellationModel.Heuristic.MRV, 2);

            var output = model.Run(seed, 10000);

            if (!output)
                print("Didnt work.");
            else
                print("Worked!");

            model.Save();
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