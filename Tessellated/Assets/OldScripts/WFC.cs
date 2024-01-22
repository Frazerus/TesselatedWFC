using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public struct Position
{
    public int X { get; set; }
    public int Y { get; set; }
}


public class WFC : MonoBehaviour
{
    public GameObject[] tileObjects;

    public string csvForExclusionRules;

    public int tileSize;

    private const int Size = 10;

    private int _numberOfPossibleStates;

    private PrevTile[][] _tiles;

    private static readonly int[] LocationsX = { 0, 1, 0, -1 };
    private static readonly int[] LocationsY = { -1, 0, 1, 0 };

    private Queue<(PrevTile, PrevTile)> _propagationQueue;

    private int _unfinishedStates;

    private int[][] _tileInclusionRules;

    // Start is called before the first frame update

    void Start()
    {
        _numberOfPossibleStates = 4;
        Init();

        Run();
        Draw();
    }

    void Init()
    {
        _unfinishedStates = Size * Size;
        //_propagationQueue = new Queue<(PrevTile, int)>();
        _tiles = new PrevTile[Size][];
        //_tileExclusionRules = CSVConverter.Read2DIntCSV(csvForExclusionRules);

    for (int x = 0; x < Size; x++)
        {
            _tiles[x] = new PrevTile[Size];
            for (int y = 0; y < Size; y++)
            {
                _tiles[x][y] = new PrevTile(_numberOfPossibleStates, new Position { X = x, Y = y });
            }
        }
    }

    void Run()
    {
        while (true)
        {
            var nextTile = FindLowestEntropyScanline();

            Observe(nextTile);

            if (_unfinishedStates <= 0)
            {
                break;
            }
        }
        Draw();
    }

    void Observe(PrevTile prevTile)
    {
        _unfinishedStates -= 1;
        prevTile.ConvergeCompletelyRandom();
        AddToPropagationQueue(prevTile);
        FinishPropagation();
    }

    void Draw()
    {
        var center = gameObject.transform.position;
        var halfSize = tileSize / 2;
        var halfNumberOfTiles = Size / 2;

        var rightTopCorner = new Vector3(center.x - halfSize + halfNumberOfTiles * tileSize, 0, center.z - halfSize + halfNumberOfTiles * tileSize);

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                var tile = _tiles[x][y];
                Instantiate(
                        tileObjects[tile.finalState],
                        new Vector3(rightTopCorner.x - x * tileSize, 0, rightTopCorner.z - y * tileSize),
                        Quaternion.identity);
            }
        }
    }

    void AddToPropagationQueue(PrevTile prevTile)
    {
        for (int i = 0; i < 4; i++)
        {
            var x = prevTile.position.X + LocationsX[i];
            var y = prevTile.position.Y + LocationsY[i];

            if (x >= Size || y >= Size || x < 0 || y < 0) continue;

            var element = _tiles[x][y];
            if (element.finalState == -1)
            {
                _propagationQueue.Enqueue((element, prevTile));
            }
        }
    }

    void FinishPropagation()
    {
        while (_propagationQueue.Count != 0)
        {
            var (tile, neighbor) = _propagationQueue.Dequeue();

            tile.AcknowledgeNeighbor(neighbor);
            var entropy = tile.RecalculateEntropy();

            if (entropy == 0)
            {
                _unfinishedStates -= 1;
                AddToPropagationQueue(tile);
            }
        }
    }

    //O(size * size)
    PrevTile FindLowestEntropyScanline()
    {
        var lowestEntropy = 1f;
        PrevTile lowestEntropyPrevTile = null;

        for (int x = 0; x < _tiles.Length; x++)
        {
            for (int y = 0; y < _tiles[x].Length; y++)
            {
                var tile = _tiles[x][y];
                var entropy = tile.Entropy;
                if (entropy <= lowestEntropy && entropy > 0)
                {
                    lowestEntropyPrevTile = tile;
                    lowestEntropy = entropy;
                }
            }
        }
        return lowestEntropyPrevTile;
    }
}