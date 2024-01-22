using UnityEngine;
using UnityEngine.UIElements;

public class OctagonSquare
{
    public bool[] mainStates;

    public bool[] sideStates;

    public bool[][] states;


    public OctagonSquare(int octagonStates, int squareStates)
    {
        states = new bool[2][];


        mainStates = new bool[octagonStates];

        sideStates = new bool[squareStates];

        states[0] = mainStates;
        states[1] = sideStates;
    }
}