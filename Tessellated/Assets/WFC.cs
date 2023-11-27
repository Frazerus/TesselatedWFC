using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public struct Position
{
    public int x { get; set; }
    public int y { get; set; }
}


public class WFC : MonoBehaviour
{
    public TileObject[] TileObjects;

    public int tileSize;

    private int size = 10;

    private int numberOfPossibleStates;

    private bool[][] field;

    private Tile[][] tiles;

    private static int[] locationsX = { 0, 1, 0, -1 };
    private static int[] locationsY = { -1, 0, 1, 0 };

    private Queue<(Tile, int)> propagationQueue;

    private int unfinishedStates;

    // Start is called before the first frame update
    void Start()
    {
        numberOfPossibleStates = 4;
        Init();

        Run();
        Draw();
    }

    void Init()
    {
        unfinishedStates = size * size;
        propagationQueue = new Queue<(Tile, int)>();
        tiles = new Tile[size][];

        for (int x = 0; x < size; x++)
        {
            tiles[x] = new Tile[size];
            for (int y = 0; y < size; y++)
            {
                tiles[x][y] = new Tile(numberOfPossibleStates, new Position { x = x, y = y });
            }
        }
    }

    void Run()
    {
        while (true)
        {
            var nextTile = FindLowestEntropyScanline();

            Observe(nextTile);

            if (unfinishedStates <= 0)
            {
                break;
            }
        }
        Draw();
    }

    void Observe(Tile tile)
    {
        unfinishedStates -= 1;
        tile.ConvergeCompletelyRandom();
        AddToPropagationQueue(tile);
        FinishPropagation();
    }

    void Draw()
    {
        var center = gameObject.transform.position;
        var halfSize = tileSize / 2;
        var halfNumberOfTiles = size / 2;

        var rightTopCorner = new Vector3(center.x - halfSize + halfNumberOfTiles * tileSize, 0, center.z - halfSize + halfNumberOfTiles * tileSize);

        //Instantiate(TileObjects[0], center, Quaternion.identity);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                var tile = tiles[x][y];
                print(new Vector3(rightTopCorner.x - x * tileSize, 0, rightTopCorner.z - y * tileSize));
                Instantiate(
                        TileObjects[tile.finalState],
                        new Vector3(rightTopCorner.x - x * tileSize, 0, rightTopCorner.z - y * tileSize),
                        Quaternion.identity);
            }
        }
        {

        }
    }

    void AddToPropagationQueue(Tile tile)
    {
        for (int i = 0; i < 4; i++)
        {
            var x = tile.position.x + locationsX[i];
            var y = tile.position.y + locationsY[i];

            if (x >= size || y >= size || x < 0 || y < 0) continue;

            var element = tiles[x][y];
            if (element.finalState == -1)
            {
                propagationQueue.Enqueue((element, tile.finalState));
            }
        }
    }

    void FinishPropagation()
    {
        while (propagationQueue.Count != 0)
        {
            (var tile, var state) = propagationQueue.Dequeue();

            tile.RemoveState(state);
            var entropy = tile.RecalculateEntropy();

            if (entropy == 0)
            {
                unfinishedStates -= 1;
                AddToPropagationQueue(tile);
            }
        }
    }

    //O(size * size)
    Tile FindLowestEntropyScanline()
    {
        var lowestEntropy = 1f;
        Tile lowestEntropyTile = null;

        for (int x = 0; x < tiles.Length; x++)
        {
            for (int y = 0; y < tiles[x].Length; y++)
            {
                var tile = tiles[x][y];
                var entropy = tile.Entropy;
                if (entropy <= lowestEntropy && entropy > 0)
                {
                    lowestEntropyTile = tile;
                    lowestEntropy = entropy;
                }
            }
        }
        return lowestEntropyTile;
    }
}