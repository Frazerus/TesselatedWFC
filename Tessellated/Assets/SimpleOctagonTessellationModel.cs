using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

public class SimpleOctagonTessellationModel : OctagonTessellationModel
{
    List<Tile>[] tiles;
    List<string>[] tilenames;
    int tilesize;

    public SimpleOctagonTessellationModel(string name, int octagonWidth, int octagonHeight, bool periodic, Heuristic heuristic, int shapeCount)
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

            Func<int, int> a, b; //a is 90 degrees rotation, b is reflection according to the symmetry rules
            int cardinality;

            char sym = xtile.Get("symmetry", 'X');
            int shape = xtile.Get("shape", 0);

            if (sym == 'L')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i + 1 : i - 1;
            }
            else if (sym == 'T')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i : 4 - i;
            }
            else if (sym == 'I')
            {
                cardinality = 2;
                a = i => 1 - i;
                b = i => i;
            }
            else if (sym == '\\')
            {
                cardinality = 2;
                a = i => 1 - i;
                b = i => 1 - i;
            }
            else if (sym == 'F')
            {
                cardinality = 8;
                a = i => i < 4 ? (i + 1) % 4 : 4 + (i - 1) % 4;
                b = i => i < 4 ? i + 4 : i - 4;
            }
            else
            {
                cardinality = 1;
                a = i => i;
                b = i => i;
            }

            TotalPossibleStates[shape] = action[shape].Count;
            firstOccurrence[shape].Add(tilename, TotalPossibleStates[shape]);

            int[][] map = new int[cardinality][];
            for (int t = 0; t < cardinality; t++)
            {
                //TODO need more cardinality for octagons
                map[t] = new int[8];

                map[t][0] = t;
                map[t][1] = a(t);
                map[t][2] = a(a(t));
                map[t][3] = a(a(a(t)));
                map[t][4] = b(t);
                map[t][5] = b(a(t));
                map[t][6] = b(a(a(t)));
                map[t][7] = b(a(a(a(t))));

                for (int s = 0; s < 8; s++) map[t][s] += TotalPossibleStates[0];

                action[shape].Add(map[t]);
            }

            // if(unique)
            //{}
            //else
            {
                var currentTile = Resources.Load<GameObject>($"octagonSquares/{tilename}");

                var tileObject = new Tile(currentTile);

                tiles[shape].Add(tileObject);
                tilenames[shape].Add($"{tilename} 0");

                for (int t = 1; t < cardinality; t++)
                {
                    if (t <= 3)
                        tiles[shape].Add(new Tile(currentTile, t));
                    if (t >= 4)
                        tiles[shape].Add(new Tile(currentTile, t - 4, t - 4));

                    tilenames[shape].Add($"{tilename} {t}");
                }
            }

            for (int t = 0; t < cardinality; t++) weightList[shape].Add(xtile.Get("weight", 1.0));
        }

        Weights = new double[shapeCount][];

        Propagator = new int [shapeCount][][][];

        for (int possibleShape = 0; possibleShape < shapeCount; possibleShape++)
        {
            TotalPossibleStates[possibleShape] = action[possibleShape].Count;
            Weights[possibleShape] = weightList[possibleShape].ToArray();

            Propagator[possibleShape] = new int[4][][];
            for (var d = 0; d < 4; d++) Propagator[possibleShape][d] = new int[TotalPossibleStates[possibleShape]][];
        }

        var densePropagator = new bool [TotalPossibleStates[0]][][];

        for (var d = 0; d < 4; d++)
        {
            densePropagator[d] = new bool[TotalPossibleStates[0]][];
            for (var t = 0; t < TotalPossibleStates[0]; t++) densePropagator[d][t] = new bool[TotalPossibleStates[0]];
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

            var L = action[0][firstOccurrence[shapeLeft][left[0]]][left.Length == 1 ? 0 : int.Parse(left[1])];

            var D = action[0][L][1];

            var R = action[0][firstOccurrence[shapeRight][right[0]]][right.Length == 1 ? 0 : int.Parse(right[1])];

            var U = action[0][R][1];

            //1 passt zu 5 -> GrassL passt zu GrassT

            densePropagator[0][R][L] = true; // 5 1
            densePropagator[0][action[0][R][6]][action[0][L][6]] = true; // 7 -> 4
            densePropagator[0][action[0][L][4]][action[0][R][4]] = true; // 2 -> 5
            densePropagator[0][action[0][L][2]][action[0][R][2]] = true; // 3 -> 7

            densePropagator[1][U][D] = true;
            densePropagator[1][action[0][D][6]][action[0][U][6]] = true;
            densePropagator[1][action[0][U][4]][action[0][D][4]] = true;
            densePropagator[1][action[0][D][2]][action[0][U][2]] = true;
        }

        for (var t2 = 0; t2 < TotalPossibleStates[0]; t2++)
        for (var t1 = 0; t1 < TotalPossibleStates[0]; t1++)
        {
            densePropagator[2][t2][t1] = densePropagator[0][t1][t2];
            densePropagator[3][t2][t1] = densePropagator[1][t1][t2];
        }

        //TODO this is main tile only right now
        var sparsePropagator = new List<int>[4][];
        for (var d = 0; d < 4; d++)
        {
            sparsePropagator[d] = new List<int>[TotalPossibleStates[0]];
            for (var t = 0; t < TotalPossibleStates[0]; t++)
                sparsePropagator[d][t] = new List<int>();
        }

        for (var d = 0; d < 4; d++)
        for (var t1 = 0; t1 < TotalPossibleStates[0]; t1++)
        {
            var sp = sparsePropagator[d][t1];
            var tp = densePropagator[d][t1];

            //Every real case is added to the sparse propagator, non available cases are ignored
            for (var t2 = 0; t2 < TotalPossibleStates[0]; t2++)
                if (tp[t2])
                    sp.Add(t2);

            var ST = sp.Count;
            if (ST == 0) Console.WriteLine($"ERROR: tile {tilenames[t1]} has no neighbors in direction {d}");
            Propagator[0][d][t1] = new int[ST];
            for (var st = 0; st < ST; st++)
                Propagator[0][d][t1][st] = sp[st];
        }
    }


    public void Save()
    {
        var tileSize = 10;
        var center = new Vector3(0, 0, 0);
        var halfSize = tileSize / 2;
        var halfNumberOfTiles = _width / 2;

        var leftTopCorner = new Vector3(center.x + halfSize - halfNumberOfTiles * tileSize, 0,
            center.z - halfSize + halfNumberOfTiles * tileSize);


        //int[] bitmapData = new int[MX * MY * tilesize * tilesize];
        if (Observed[0].main >= 0)
        {
            for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
            {
                var tile = tiles[0][Observed[x + y * _width].main];
                tile.Create(new Vector3(leftTopCorner.x + x * tileSize, 0, leftTopCorner.z - y * tileSize));
            }
        }
    }
}