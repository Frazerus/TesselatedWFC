using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class SimpleOctagonTessellationModel : OctagonTessellationModel
{
    List<Tile>[] tiles;
    List<string>[] tilenames;
    int tilesize;

    public SimpleOctagonTessellationModel(string name, int octagonWidth, int octagonHeight, bool periodic,
        Heuristic heuristic, int shapeCount)
        : base(octagonWidth, octagonHeight, periodic, heuristic, shapeCount)
    {
        XElement xroot = XDocument.Load($"Assets/{name}.xml").Root;

        var weightList = new List<double>[shapeCount];

        tiles = new List<Tile>[shapeCount];
        tilenames = new List<string>[shapeCount];

        var action = new List<int[]>[shapeCount];
        var firstOccurrence = new Dictionary<string, int>[shapeCount];

        TotalPossibleStates = new int[shapeCount];

        for (int i = 0; i < shapeCount; i++)
        {
            weightList[i] = new List<double>();

            tiles[i] = new List<Tile>();
            tilenames[i] = new List<string>();


            action[i] = new List<int[]>();
            firstOccurrence[i] = new Dictionary<string, int>();
        }

        foreach (XElement xtile in xroot.Element("tiles").Elements("tile"))
        {
            string tilename = xtile.Get<string>("name");

            Func<int, int> a, b, c; //a is 90 degrees rotation, b is reflection according to the symmetry rules
            int cardinality;

            char sym = xtile.Get("symmetry", 'X');
            int shape = xtile.Get("shape", 0);

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
            else if (sym == 'C')
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
                b = i => i % 2 == 0 ? 8 - i - 2 : i + 4;
                c = i => 8 - i - 1;
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

            int[][] map = new int[cardinality][];
            for (int t = 0; t < cardinality; t++)
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

                for (int s = 0; s < 12; s++)
                    map[t][s] += TotalPossibleStates[shape];

                action[shape].Add(map[t]);
            }

            var currentTile = Resources.Load<GameObject>($"octagonSquares/{tilename}");

            var tileObject = new Tile(currentTile);

            tiles[shape].Add(tileObject);
            tilenames[shape].Add($"{tilename} 0");

            for (int t = 1; t < cardinality; t++)
            {
                if (t <= 3)
                    tiles[shape].Add(new Tile(currentTile, t, cardinality: t));
                if (t >= 4)
                    tiles[shape].Add(new Tile(currentTile, t - 4, t - 4, cardinality: t));

                tilenames[shape].Add($"{tilename} {t}");
            }

            for (int t = 0; t < cardinality; t++)
                weightList[shape].Add(xtile.Get("weight", 1.0));
        }

        Weights = new double[shapeCount][];

        Propagator = new int [shapeCount][][][][];

        for (int left = 0; left < shapeCount; left++)
        {
            TotalPossibleStates[left] = action[left].Count;
            Weights[left] = weightList[left].ToArray();
        }

        _shapeWithMostStates = 0;
        //var densePropagator = new bool [][TotalPossibleStates[0]][][];
        for (int i = 0; i < TotalPossibleStates.Length; i++)
        {
            if (TotalPossibleStates[i] > TotalPossibleStates[_shapeWithMostStates])
                _shapeWithMostStates = i;
        }

        for (int left = 0; left < shapeCount; left++)
        {
            Propagator[left] = new int[shapeCount][][][];
            for (int right = 0; right < shapeCount; right++)
            {
                Propagator[left][right] = new int[4][][];
                for (var d = 0; d < 4; d++) Propagator[left][right][d] = new int[TotalPossibleStates[_shapeWithMostStates]][];
            }
        }

        bool[][][][][] densePropagator = new bool[shapeCount][][][][];
        for(int left = 0; left < shapeCount; left++)
        {
            densePropagator[left] = new bool[shapeCount][][][];
            for (int right = 0; right < shapeCount; right++)
            {
                densePropagator[left][right] = new bool[4][][];
                for (int d = 0; d < 4; d++)
                {
                    densePropagator[left][right][d] = new bool[TotalPossibleStates[_shapeWithMostStates]][];
                    for (int states = 0; states < TotalPossibleStates[_shapeWithMostStates]; states++)
                    {
                        densePropagator[left][right][d][states] = new bool[TotalPossibleStates[_shapeWithMostStates]];
                    }
                }
            }
        }

        foreach (var xneighbor in xroot.Element("neighbors").Elements("neighbor"))
        {
            var left = xneighbor.Get<string>("left")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var right = xneighbor.Get<string>("right")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int shapeLeft = -1;
            int shapeRight = -1;

            for (int possibleShape = 0; possibleShape < shapeCount; possibleShape++)
            {
                if (firstOccurrence[possibleShape].ContainsKey(left[0]))
                    shapeLeft = possibleShape;
                if (firstOccurrence[possibleShape].ContainsKey(right[0]))
                    shapeRight = possibleShape;
            }

            if (shapeLeft == -1 || shapeRight == -1)
                continue;

            var L = action[shapeLeft][firstOccurrence[shapeLeft][left[0]]][left.Length == 1 ? 0 : int.Parse(left[1])];

            var D = action[shapeLeft][L][1];

            var R = action[shapeRight][firstOccurrence[shapeRight][right[0]]][right.Length == 1 ? 0 : int.Parse(right[1])];

            var U = action[shapeRight][R][1];

            var shapeLeftOffset = shapeLeft != shapeRight && shapeLeft < shapeRight ? 4 : 0;
            var shapeRightOffset = shapeLeft != shapeRight && shapeRight < shapeLeft ? 4 : 0;
            densePropagator[shapeRight][shapeLeft][0][R][L] = true; // 5 1

            var main = action[shapeRight][R][6 + shapeRightOffset];
            var allowed = action[shapeLeft][L][6 + shapeLeftOffset];
            densePropagator[shapeRight][shapeLeft][0][main][allowed] = true; // 7 -> 4

            main = action[shapeLeft][L][4 + shapeLeftOffset];
            allowed = action[shapeRight][R][4 + shapeRightOffset];
            densePropagator[shapeLeft][shapeRight][0][main][allowed] = true; // 2 -> 5

            main = action[shapeLeft][L][2];
            allowed = action[shapeRight][R][2];
            densePropagator[shapeLeft][shapeRight][0][main][allowed] = true; // 3 -> 7

            densePropagator[shapeRight][shapeLeft][1][U][D] = true;
            main = action[shapeRight][U][4 + shapeRightOffset];
            allowed = action[shapeLeft][D][4 + shapeLeftOffset];
            densePropagator[shapeRight][shapeLeft][1][main][allowed] = true;

            main = action[shapeLeft][D][6 + shapeLeftOffset];
            allowed = action[shapeRight][U][6 + shapeRightOffset];
            densePropagator[shapeLeft][shapeRight][1][main][allowed] = true;

            main = action[shapeLeft][D][2];
            allowed = action[shapeRight][U][2];
            densePropagator[shapeLeft][shapeRight][1][main][allowed] = true;
        }

        for (int right = 0; right < shapeCount; right++)
        {
            for (int left = 0; left < shapeCount; left++)
            {
                //TODO possible optimization
                for (var t2 = 0; t2 < TotalPossibleStates[_shapeWithMostStates]; t2++)
                for (var t1 = 0; t1 < TotalPossibleStates[_shapeWithMostStates]; t1++)
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

        var sparsePropagator = new List<int>[shapeCount][][][];
        for (int left = 0; left < shapeCount; left++)
        {
            sparsePropagator[left] = new List<int>[shapeCount][][];
            for (int right = 0; right < shapeCount; right++)
            {
                sparsePropagator[left][right] = new List<int>[4][];
                for (var d = 0; d < 4; d++)
                {
                    //TODO maybe left right issue here
                    sparsePropagator[left][right][d] = new List<int>[TotalPossibleStates[_shapeWithMostStates]];
                    for (var t = 0; t < TotalPossibleStates[_shapeWithMostStates]; t++)
                        sparsePropagator[left][right][d][t] = new List<int>();
                }

                for (var d = 0; d < 4; d++)
                for (var t1 = 0; t1 < TotalPossibleStates[_shapeWithMostStates]; t1++)
                {
                    var sparse = sparsePropagator[left][right][d][t1];
                    var dense = densePropagator[left][right][d][t1];

                    //Every real case is added to the sparse propagator, non available cases are ignored
                    for (var t2 = 0; t2 < TotalPossibleStates[_shapeWithMostStates]; t2++)
                        if (dense[t2])
                            sparse.Add(t2);

                    var ST = sparse.Count;
                    //if (ST == 0 && left != 1 && right != 1) Console.WriteLine($"ERROR: tile {tilenames[right][t1]} has no neighbors in direction {d}");
                    Propagator[left][right][d][t1] = new int[ST];
                    for (var st = 0; st < ST; st++)
                        Propagator[left][right][d][t1][st] = sparse[st];
                }
            }
        }
    }


    public void Save()
    {
        if (Observed[0][0] < 0) return;

        var tileSize = 10;
        var center = new Vector3(0, 0, 0);
        var halfSize = tileSize / 2;
        var halfNumberOfTiles = _width / 2;

        var leftTopCorner = new Vector3(center.x + halfSize - halfNumberOfTiles * tileSize, 0,
            center.z - halfSize + halfNumberOfTiles * tileSize);

        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
        {
            for (int shape = 0; shape < ShapeCount; shape++)
            {
                var tile = tiles[shape][Observed[x + y * _width][shape]];
                tile.Create(new Vector3(leftTopCorner.x + x * tileSize + shape * halfSize, 0, leftTopCorner.z - y * tileSize + shape * halfSize));
            }
        }
    }
}