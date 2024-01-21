// Copyright (C) 2016 Maxim Gumin, The MIT License (MIT)

using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;

class SimpleTiledModel : Model
{
    List<Tile> tiles;
    List<string> tilenames;
    int tilesize;

    public SimpleTiledModel(string name, string subsetName, int width, int height, bool periodic,
        Heuristic heuristic) : base(width, height, 1, periodic, heuristic)
    {
        XElement xroot = XDocument.Load($"Assets/{name}.xml").Root;
        bool unique = xroot.Get("unique", false);

        List<string> subset = null;
        if (subsetName != null)
        {
            XElement xsubset = xroot.Element("subsets").Elements("subset")
                .FirstOrDefault(x => x.Get<string>("name") == subsetName);
            if (xsubset == null) Console.WriteLine($"ERROR: subset {subsetName} is not found");
            else subset = xsubset.Elements("tile").Select(x => x.Get<string>("name")).ToList();
        }

        static int[] tile(Func<int, int, int> f, int size)
        {
            int[] result = new int[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                result[x + y * size] = f(x, y);
            return result;
        }

        static int[] rotate(int[] array, int size) => tile((x, y) => array[size - 1 - y + x * size], size);
        static int[] reflect(int[] array, int size) => tile((x, y) => array[size - 1 - x + y * size], size);

        tiles = new List<Tile>();
        tilenames = new List<string>();
        var weightList = new List<double>();

        var action = new List<int[]>();
        var firstOccurrence = new Dictionary<string, int>();

        foreach (XElement xtile in xroot.Element("tiles").Elements("tile"))
        {
            string tilename = xtile.Get<string>("name");
            if (subset != null && !subset.Contains(tilename)) continue;

            Func<int, int> a, b; //a is 90 degrees rotation, b is reflection
            int cardinality;

            char sym = xtile.Get("symmetry", 'X');
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

            T = action.Count;
            firstOccurrence.Add(tilename, T);

            int[][] map = new int[cardinality][];
            for (int t = 0; t < cardinality; t++)
            {
                map[t] = new int[8];

                map[t][0] = t;
                map[t][1] = a(t);
                map[t][2] = a(a(t));
                map[t][3] = a(a(a(t)));
                map[t][4] = b(t);
                map[t][5] = b(a(t));
                map[t][6] = b(a(a(t)));
                map[t][7] = b(a(a(a(t))));

                for (int s = 0; s < 8; s++) map[t][s] += T;

                action.Add(map[t]);
            }

            if (unique)
            {
                for (int t = 0; t < cardinality; t++)
                {
                    var currentTile = Resources.Load<GameObject>($"prefabs/{tilename}");

                    var tileObject = new Tile(currentTile);

                    tiles.Add(tileObject);
                    tilenames.Add($"{tilename} {t}");
                }
            }
            else
            {
                var currentTile = Resources.Load<GameObject>($"prefabs/{tilename}");

                var tileObject = new Tile(currentTile);

                tiles.Add(tileObject);
                tilenames.Add($"{tilename} 0");

                for (int t = 1; t < cardinality; t++)
                {
                    if (t <= 3)
                        tiles.Add(new Tile(currentTile, t));
                    if (t >= 4)
                        tiles.Add(new Tile(currentTile, t - 4, t - 4));

                    tilenames.Add($"{tilename} {t}");
                }
            }

            for (int t = 0; t < cardinality; t++) weightList.Add(xtile.Get("weight", 1.0));
        }

        T = action.Count;
        weights = weightList.ToArray();

        propagator = new int[4][][];
        var densePropagator = new bool[4][][];
        for (int d = 0; d < 4; d++)
        {
            densePropagator[d] = new bool[T][];
            propagator[d] = new int[T][];
            for (int t = 0; t < T; t++) densePropagator[d][t] = new bool[T];
        }

        foreach (XElement xneighbor in xroot.Element("neighbors").Elements("neighbor"))
        {
            string[] left = xneighbor.Get<string>("left")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] right = xneighbor.Get<string>("right")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (subset != null && (!subset.Contains(left[0]) || !subset.Contains(right[0]))) continue;

            int L = action[firstOccurrence[left[0]]][left.Length == 1 ? 0 : int.Parse(left[1])];
            // GrassL rotiert um 1?????
            var D = action[L][1];

            int R = action[firstOccurrence[right[0]]][right.Length == 1 ? 0 : int.Parse(right[1])];

            var U = action[R][1];

            //1 passt zu 5 -> GrassL passt zu GrassT

            //TODO Wofür ist dense propagator
            densePropagator[0][R][L] = true; // 5 1
            densePropagator[0][action[R][6]][action[L][6]] = true; // 7 -> 4
            densePropagator[0][action[L][4]][action[R][4]] = true; // 2 -> 5
            densePropagator[0][action[L][2]][action[R][2]] = true; // 3 -> 7

            densePropagator[1][U][D] = true;
            densePropagator[1][action[D][6]][action[U][6]] = true;
            densePropagator[1][action[U][4]][action[D][4]] = true;
            densePropagator[1][action[D][2]][action[U][2]] = true;
        }

        for (int t2 = 0; t2 < T; t2++)
        for (int t1 = 0; t1 < T; t1++)
        {
            densePropagator[2][t2][t1] = densePropagator[0][t1][t2];
            densePropagator[3][t2][t1] = densePropagator[1][t1][t2];
        }

        List<int>[][] sparsePropagator = new List<int>[4][];
        for (int d = 0; d < 4; d++)
        {
            sparsePropagator[d] = new List<int>[T];
            for (int t = 0; t < T; t++)
                sparsePropagator[d][t] = new List<int>();
        }

        for (int d = 0; d < 4; d++)
        for (int t1 = 0; t1 < T; t1++)
        {
            List<int> sp = sparsePropagator[d][t1];
            bool[] tp = densePropagator[d][t1];

            for (int t2 = 0; t2 < T; t2++)
                if (tp[t2])
                    sp.Add(t2);

            int ST = sp.Count;
            if (ST == 0) Console.WriteLine($"ERROR: tile {tilenames[t1]} has no neighbors in direction {d}");
            propagator[d][t1] = new int[ST];
            for (int st = 0; st < ST; st++)
                propagator[d][t1][st] = sp[st];
        }
    }

    public void Print()
    {

    }

    public override void Save(string filename)
    {
        var tileSize = 10;
        var center = new Vector3(0, 0, 0);
        var halfSize = tileSize / 2;
        var halfNumberOfTiles = MX / 2;

        var leftTopCorner = new Vector3(center.x + halfSize - halfNumberOfTiles * tileSize, 0, center.z - halfSize + halfNumberOfTiles * tileSize);


        //int[] bitmapData = new int[MX * MY * tilesize * tilesize];
        if (observed[0] >= 0)
        {
            for (int x = 0; x < MX; x++) for (int y = 0; y < MY; y++)
            {
                var tile = tiles[observed[x + y * MX]];
                tile.Create(new Vector3(leftTopCorner.x + x * tileSize, 0, leftTopCorner.z - y * tileSize));
            }
        }
        /*
        else
        {
            for (int i = 0; i < wave.Length; i++)
            {
                int x = i % MX, y = i / MX;
                if (blackBackground && sumsOfOnes[i] == T)
                    for (int yt = 0; yt < tilesize; yt++) for (int xt = 0; xt < tilesize; xt++)
                            bitmapData[x * tilesize + xt + (y * tilesize + yt) * MX * tilesize] = 255 << 24;
                else
                {
                    bool[] w = wave[i];
                    double normalization = 1.0 / sumsOfWeights[i];
                    for (int yt = 0; yt < tilesize; yt++) for (int xt = 0; xt < tilesize; xt++)
                        {
                            int idi = x * tilesize + xt + (y * tilesize + yt) * MX * tilesize;
                            double r = 0, g = 0, b = 0;
                            for (int t = 0; t < T; t++) if (w[t])
                                {
                                    int argb = tiles[t][xt + yt * tilesize];
                                    r += ((argb & 0xff0000) >> 16) * weights[t] * normalization;
                                    g += ((argb & 0xff00) >> 8) * weights[t] * normalization;
                                    b += (argb & 0xff) * weights[t] * normalization;
                                }
                            bitmapData[idi] = unchecked((int)0xff000000 | ((int)r << 16) | ((int)g << 8) | (int)b);
                        }
                }
            }
        }*/
        //BitmapHelper.SaveBitmap(bitmapData, MX * tilesize, MY * tilesize, filename);
    }

    public string TextOutput()
    {
        var result = new System.Text.StringBuilder();
        for (int y = 0; y < MY; y++)
        {
            for (int x = 0; x < MX; x++) result.Append($"{tilenames[observed[x + y * MX]]}, ");
            result.Append(Environment.NewLine);
        }
        return result.ToString();
    }
}