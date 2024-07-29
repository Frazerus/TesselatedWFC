using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;

public class SimpleOctagonTessellationModel : OctagonTessellationModel
{
    private readonly List<Tile>[] _tiles;
    private readonly List<string>[] _tileNames;
    private int _tilesize;

    public SimpleOctagonTessellationModel(
        string name,
        int octagonWidth,
        int octagonHeight,
        bool periodic,
        Heuristic heuristic,
        int shapeCount)
        : base(octagonWidth, octagonHeight, periodic, heuristic, shapeCount)
    {
        var xroot = XDocument.Load($"Assets/{name}.xml").Root;

        var weightList = new List<double>[shapeCount];

        var action = new List<int[]>[shapeCount];
        var firstOccurrence = new Dictionary<string, int>[shapeCount];


        _tiles = new List<Tile>[shapeCount];
        _tileNames = new List<string>[shapeCount];

        Initialize(shapeCount, weightList, action, firstOccurrence);

        CreateTilesAndActions(xroot, action, firstOccurrence, weightList);

        Weights = new double[shapeCount][];

        for (var left = 0; left < shapeCount; left++)
        {
            TotalPossibleStates[left] = action[left].Count;
            Weights[left] = weightList[left].ToArray();
        }

        ShapeWithMostStates = GetShapeWithMostStates(TotalPossibleStates);

        Propagator = new int [shapeCount][][][][];
        var densePropagator = new bool[shapeCount][][][][];

        InitializePropagators(shapeCount, densePropagator, Propagator);

        CreateNeighborRelationships(shapeCount, xroot, firstOccurrence, action, densePropagator);

        FillPropagatorWithNeighborInfo(shapeCount, densePropagator);
    }

    public void CreateOutput()
    {
        if (Observed[0][0] < 0) return;

        var tileSize = 10;
        var center = new Vector3(0, 0, 0);
        var halfSize = tileSize / 2;
        var halfNumberOfTiles = Width / 2;

        var leftTopCorner = new Vector3(center.X + halfSize - halfNumberOfTiles * tileSize, 0,
            center.Z - halfSize + halfNumberOfTiles * tileSize);

        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            for (int shape = 0; shape < ShapeCount; shape++)
            {
                var tile = _tiles[shape][Observed[x + y * Width][shape]];
                tile.Create(new Vector3(leftTopCorner.X + x * tileSize + shape * halfSize, 0, leftTopCorner.Z - y * tileSize + shape * halfSize));
            }
        }
    }

    private void Initialize(int shapeCount, List<double>[] weightList, List<int[]>[] action, Dictionary<string, int>[] firstOccurrence)
    {
        TotalPossibleStates = new int[shapeCount];

        for (var i = 0; i < shapeCount; i++)
        {
            weightList[i] = new List<double>();

            _tiles[i] = new List<Tile>();
            _tileNames[i] = new List<string>();

            action[i] = new List<int[]>();
            firstOccurrence[i] = new Dictionary<string, int>();
        }
    }

    private void InitializePropagators(int shapeCount, bool[][][][][] densePropagator, int[][][][][] propagator)
    {
        for (var left = 0; left < shapeCount; left++)
        {
            propagator[left] = new int[shapeCount][][][];
            for (var right = 0; right < shapeCount; right++)
            {
                propagator[left][right] = new int[4][][];
                for (var d = 0; d < 4; d++)
                    propagator[left][right][d] = new int[TotalPossibleStates[ShapeWithMostStates]][];
            }
        }

        for (var left = 0; left < shapeCount; left++)
        {
            densePropagator[left] = new bool[shapeCount][][][];
            for (var right = 0; right < shapeCount; right++)
            {
                densePropagator[left][right] = new bool[4][][];
                for (var d = 0; d < 4; d++)
                {
                    densePropagator[left][right][d] = new bool[TotalPossibleStates[ShapeWithMostStates]][];
                    for (var states = 0; states < TotalPossibleStates[ShapeWithMostStates]; states++)
                    {
                        densePropagator[left][right][d][states] = new bool[TotalPossibleStates[ShapeWithMostStates]];
                    }
                }
            }
        }
    }

    private void CreateTilesAndActions(XElement xroot, List<int[]>[] action, Dictionary<string, int>[] firstOccurrence, List<double>[] weightList)
    {
        foreach (var xtile in xroot.Element("tiles")!.Elements("tile"))
        {
            var tilename = xtile.Get<string>("name");

            Func<int, int> a, b, c; //a is 90 degrees rotation, b is reflection along y axis, c is reflection along axis defined by (0,0) and (1, -1)
            int cardinality;

            var sym = xtile.Get("symmetry", 'X');
            var shape = xtile.Get("shape", 0);

            if (sym == 'L')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i + 1 : i - 1;
                c = i => i;
            }
            else if (sym == 'T')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i : 4 - i;
                c = i => i;
            }
            else if (sym == 'I')
            {
                cardinality = 2;
                a = i => 1 - i;
                b = i => i;
                c = i => i;
            }
            else if (sym == '\\')
            {
                cardinality = 2;
                a = i => 1 - i;
                b = i => 1 - i;
                c = i => i;
            }
            else if (sym == 'F')
            {
                cardinality = 8;
                a = i => i < 4 ? (i + 1) % 4 : 4 + (i - 1) % 4;
                b = i => i < 4 ? i + 4 : i - 4;
                c = i => i;
            }
            else if (sym == 'Y')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? (i + 2) % 4 : i;
                c = i => 4 - i - 1;
            }
            else if (sym == 'V')
            {
                cardinality = 8;
                a = i => i < 4 ? (i + 1) % 4 : 4 + (i - 1) % 4;
                b = i => (i < 4 ? i + 4 : i - 4) % 8;
                c = i =>
                {
                    if (i < 4)
                        return (i % 2 == 0 ? 8 - i - 1 : 4 + i - 1) % 8;
                    else
                        return 8 - i - 1;
                };
            }
            else
            {
                cardinality = 1;
                a = i => i;
                b = i => i;
                c = i => i;
            }

            TotalPossibleStates[shape] = action[shape].Count;
            firstOccurrence[shape].Add(tilename, TotalPossibleStates[shape]);

            var map = new int[cardinality][];
            for (var t = 0; t < cardinality; t++)
            {
                map[t] = new int[12];

                map[t][0] = t;
                map[t][1] = a(t);
                map[t][2] = a(a(t));
                map[t][3] = a(a(a(t)));
                map[t][4] = b(t);
                map[t][5] = b(a(t));
                map[t][6] = b(a(a(t)));
                map[t][7] = b(a(a(a(t))));
                map[t][8] = c(t);
                map[t][9] = c(a(t));
                map[t][10] = c(a(a(t)));
                map[t][11] = c(a(a(a(t))));

                for (var s = 0; s < 12; s++)
                    map[t][s] += TotalPossibleStates[shape];

                action[shape].Add(map[t]);
            }

            var currentTile = Resources.Load<GameObject>($"octagonSquares/{tilename}");

            for (var t = 0; t < cardinality; t++)
            {
                if (t <= 3)
                    _tiles[shape].Add(new Tile(currentTile, t, cardinality: t));
                if (t >= 4)
                    _tiles[shape].Add(new Tile(currentTile, t - 4, t - 4, cardinality: t));

                _tileNames[shape].Add($"{tilename} {t}");
            }

            for (var t = 0; t < cardinality; t++)
                weightList[shape].Add(xtile.Get("weight", 1.0));
        }
    }

    private void CreateNeighborRelationships(int shapeCount, XElement xroot, Dictionary<string, int>[] firstOccurrence,
        List<int[]>[] action, bool[][][][][] densePropagator)
    {
        // Calculates the neighbor relationships for a given input relationship depending on the symmetries defined for the tiles and the specific shapes
        foreach (var xneighbor in xroot.Element("neighbors")!.Elements("neighbor"))
        {
            var leftTile = xneighbor.Get<string>("left")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var rightTile = xneighbor.Get<string>("right")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var shapeLeft = -1;
            var shapeRight = -1;

            for (var possibleShape = 0; possibleShape < shapeCount; possibleShape++)
            {
                if (firstOccurrence[possibleShape].ContainsKey(leftTile[0]))
                    shapeLeft = possibleShape;
                if (firstOccurrence[possibleShape].ContainsKey(rightTile[0]))
                    shapeRight = possibleShape;
            }

            if (shapeLeft == -1 || shapeRight == -1)
                continue;

            // the lower shape (0) is octagon, so if only one of them is an octagon, we add 4 to the reflection/rotation state lookup for it
            var shapeLeftOffset = shapeLeft != shapeRight && shapeLeft < shapeRight ? 4 : 0;
            var shapeRightOffset = shapeLeft != shapeRight && shapeRight < shapeLeft ? 4 : 0;


            var L = action[shapeLeft][firstOccurrence[shapeLeft][leftTile[0]]][
                leftTile.Length == 1 ? 0 : int.Parse(leftTile[1])];

            var D = action[shapeLeft][L][1];

            var R = action[shapeRight][firstOccurrence[shapeRight][rightTile[0]]][
                rightTile.Length == 1 ? 0 : int.Parse(rightTile[1])];



            var U = action[shapeRight][R][1];

            densePropagator[shapeRight][shapeLeft][0][R][L] = true; // 5 1

            var left = action[shapeLeft][L][6 + shapeLeftOffset];
            var right = action[shapeRight][R][6 + shapeRightOffset];
            densePropagator[shapeRight][shapeLeft][0][right][left] = true; // 7 -> 4

            left = action[shapeLeft][L][4 + shapeLeftOffset];
            right = action[shapeRight][R][4 + shapeRightOffset];
            densePropagator[shapeLeft][shapeRight][0][left][right] = true; // 2 -> 5

            left = action[shapeLeft][L][2];
            right = action[shapeRight][R][2];
            densePropagator[shapeLeft][shapeRight][0][left][right] = true; // 3 -> 7

            densePropagator[shapeRight][shapeLeft][1][U][D] = true;

            var up = action[shapeRight][U][4 + shapeRightOffset];
            var down = action[shapeLeft][D][4 + shapeLeftOffset];
            densePropagator[shapeRight][shapeLeft][1][up][down] = true;

            up = action[shapeRight][U][6 + shapeRightOffset];
            down = action[shapeLeft][D][6 + shapeLeftOffset];
            densePropagator[shapeLeft][shapeRight][1][down][up] = true;

            up = action[shapeRight][U][2];
            down = action[shapeLeft][D][2];
            densePropagator[shapeLeft][shapeRight][1][down][up] = true;
        }

        // Flips the neighbor relationships defined above for left-right and up-down to right-left and down-up
        for (var right = 0; right < shapeCount; right++)
        {
            for (var left = 0; left < shapeCount; left++)
            {
                for (var t2 = 0; t2 < TotalPossibleStates[ShapeWithMostStates]; t2++)
                for (var t1 = 0; t1 < TotalPossibleStates[ShapeWithMostStates]; t1++)
                {
                    if (left == right)
                    {
                        densePropagator[left][right][2][t2][t1] = densePropagator[left][right][0][t1][t2];
                        densePropagator[left][right][3][t2][t1] = densePropagator[left][right][1][t1][t2];
                    }
                    else
                    {
                        densePropagator[right][left][2][t2][t1] = densePropagator[left][right][0][t1][t2];
                        densePropagator[right][left][3][t2][t1] = densePropagator[left][right][1][t1][t2];
                    }
                }
            }
        }
    }

    private void FillPropagatorWithNeighborInfo(int shapeCount, bool[][][][][] densePropagator)
    {
        var sparsePropagator = new List<int>[shapeCount][][][];
        for (var shapeLeft = 0; shapeLeft < shapeCount; shapeLeft++)
        {
            sparsePropagator[shapeLeft] = new List<int>[shapeCount][][];
            for (var shapeRight = 0; shapeRight < shapeCount; shapeRight++)
            {
                sparsePropagator[shapeLeft][shapeRight] = new List<int>[4][];
                for (var d = 0; d < 4; d++)
                {
                    sparsePropagator[shapeLeft][shapeRight][d] = new List<int>[TotalPossibleStates[ShapeWithMostStates]];
                    for (var t = 0; t < TotalPossibleStates[ShapeWithMostStates]; t++)
                        sparsePropagator[shapeLeft][shapeRight][d][t] = new List<int>();
                }

                for (var d = 0; d < 4; d++)
                for (var left = 0; left < TotalPossibleStates[shapeLeft]; left++)
                {
                    var sparse = sparsePropagator[shapeLeft][shapeRight][d][left];
                    var dense = densePropagator[shapeLeft][shapeRight][d][left];

                    //Every real case is added to the sparse propagator, non available cases are ignored
                    for (var right = 0; right < TotalPossibleStates[shapeRight]; right++)
                        if (dense[right])
                            sparse.Add(right);

                    var possibleNeighborCount = sparse.Count;
                    //if (possibleNeighborCount == 0 && (shapeLeft != 1 || shapeRight != 1))
                    //    Debug.Log($"ERROR: tile {_tileNames[shapeLeft][left]} has no neighbors in direction {d}");

                    Propagator[shapeLeft][shapeRight][d][left] = sparse.ToArray();
                }
            }
        }
    }

    private int GetShapeWithMostStates(int[] totalPossibleStates)
    {
        var shapeWithMostStates = 0;
        for (var i = 0; i < totalPossibleStates.Length; i++)
        {
            if (totalPossibleStates[i] > totalPossibleStates[ShapeWithMostStates])
                shapeWithMostStates = i;
        }

        return shapeWithMostStates;
    }
}