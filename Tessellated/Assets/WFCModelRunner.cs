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
            var model = new SimpleTiledModel(originalWithOctagons ? "octagon_square" : "standard", null, 10, 10, false, Model.Heuristic.MRV, originalWithOctagons ? "OctagonSquares" : "standard");

            var output = model.Run(seed, 10000);

            if (!output)
                print("Didnt work.");
            else
                print("Worked!");

            model.Save();
        }
        else
        {
            var model = new SimpleOctagonTessellationModel("octagon", 10, 10, false, OctagonTessellationModel.Heuristic.MRV, 2);

            var output = model.Run(seed, 10000);

            if (!output)
                print("Didnt work.");
            else
                print("Worked!");

            model.Save();
        }






    }
}