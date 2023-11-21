using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Flags]
public enum PossibleState : short
{
    Grass = 1,
    Path = 2,
    Mountain = 4,
    Snow = 8,
}

struct Position
{
    public int x { get; set; }
    public int y { get; set; }
}


public class WFC : MonoBehaviour
{
    public Tile grass;
    public Tile path;

    public Tile mountain;

    public Tile snow;

    private int size = 10;

    private int numberOfPossibleStates;

    private bool[][] field;

    private Tile[][] tiles;

    // Start is called before the first frame update
    void Start()
    {
        numberOfPossibleStates = 4;
        Init();
    }

    void Init()
    {
        field = new bool[size * size][];
        for (int i = 0; i < field.Length; i++)
        {
            field[i] = new[] { true, true, true, true }; // change this
        }
    }

    void Run()
    {
        while (true)
        {
            var nextTile = FindLowestEntropyScanline();

            Observe(nextTile);

        }
    }

    void Observe(Position pos)
    {
        double [] dist = new double[10];
        var x = dist.Random()
    }

    void Propagate()
    {

    }

    //O(n * n)
    Position FindLowestEntropyScanline()
    {
        var lowestEntropy = 1f;
        var lowestEntropyTile = new Position { x = -1, y = -1 };

        for (int x = 0; x < tiles.Length; x++)
        {
            for (int y = 0; y < tiles[x].Length; y++)
            {
                Tile tile = tiles[x][y];
                float entropy = Entropy(tile);
                if (entropy < lowestEntropy && entropy > 0)
                {
                    lowestEntropyTile = new Position { x = x, y = y };
                    lowestEntropy = entropy;
                }
            }
        }
        return lowestEntropyTile;
    }

    float Entropy(Tile tile)
    {
        var output = 0;
        var currentStates = (short)tile.State;

        for (int i = 0; i < numberOfPossibleStates; i++)
        {
            output += currentStates & 1;
            currentStates >>= 1;
        }
        return output / numberOfPossibleStates;
    }
}
