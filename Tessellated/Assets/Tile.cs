using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int finalState = -1;
    public bool[] PossibleStates;

    public Position position;

    public float Entropy = 1;
    public Tile(int numberOfPossibleStates, Position position)
    {
        PossibleStates = new bool[numberOfPossibleStates];
        for (int i = 0; i < numberOfPossibleStates; i++)
        {
            PossibleStates[i] = true;
        }
        this.position = position;
    }

    public int ConvergeCompletelyRandom()
    {
        var state = Random.Range(0, PossibleStates.Length);
        while (!PossibleStates[state])
        {
            state = state + 1 % PossibleStates.Length;
        }

        finalState = state;
        Entropy = 0;

        return finalState;
    }

    public void RemoveState(int state)
    {
        PossibleStates[state] = false;
    }


    public float RecalculateEntropy()
    {
        var output = PossibleStates.Count(v => v);

        if (output == 1)
        {
            for (int i = 0; i < PossibleStates.Length; i++)
            {
                if (PossibleStates[i])
                {
                    finalState = i;
                    Entropy = 0;
                    return Entropy;
                }
            }
        }

        Entropy = output / PossibleStates.Length;
        return Entropy;
    }
}


