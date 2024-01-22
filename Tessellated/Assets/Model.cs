// Copyright (C) 2016 Maxim Gumin, The MIT License (MIT)

using System;

abstract class Model
{
    protected bool[][] wave;


    //filled in left -> down -> right -> up fashion, the next index supplies the middle thing, and the values then supply what can go where
    protected int[][][] propagator;
    int[][][] compatible;
    protected int[] observed;

    (int, int)[] stack;
    int stacksize, observedSoFar;

    protected int MX, MY, T, N;
    protected bool periodic, ground;

    protected double[] weights;
    double[] weightLogWeights, weightDistributions;

    protected int[] sumsOfOnes;
    double sumOfWeights,  sumOfWeightLogWeights, startingEntropy;
    protected double[] sumsOfWeights, sumsOfWeightLogWeights, entropies;

    public enum Heuristic { Entropy, MRV, Scanline };
    Heuristic heuristic;

    protected Model(int width, int height, int N, bool periodic, Heuristic heuristic)
    {
        MX = width;
        MY = height;
        this.N = N;
        this.periodic = periodic;
        this.heuristic = heuristic;
    }

    void Init()
    {
        wave = new bool[MX * MY][];
        compatible = new int[wave.Length][][];
        for (int i = 0; i < wave.Length; i++)
        {
            wave[i] = new bool[T];
            compatible[i] = new int[T][];
            for (int t = 0; t < T; t++) compatible[i][t] = new int[4];
        }
        weightDistributions = new double[T];
        observed = new int[MX * MY];

        weightLogWeights = new double[T];
        sumOfWeights = 0;
        sumOfWeightLogWeights = 0;

        for (int t = 0; t < T; t++)
        {
            weightLogWeights[t] = weights[t] * Math.Log(weights[t]);
            sumOfWeights += weights[t];
            sumOfWeightLogWeights += weightLogWeights[t];
        }

        startingEntropy = Math.Log(sumOfWeights) - sumOfWeightLogWeights / sumOfWeights;

        sumsOfOnes = new int[MX * MY];
        sumsOfWeights = new double[MX * MY];
        sumsOfWeightLogWeights = new double[MX * MY];
        entropies = new double[MX * MY];

        stack = new (int, int)[wave.Length * T];
        stacksize = 0;
    }

    public bool Run(int seed, int limit)
    {
        if (wave == null) Init();

        Clear();
        Random random = new(seed);

        for (int l = 0; l < limit || limit < 0; l++)
        {
            int node = NextUnobservedNode(random);
            if (node >= 0)
            {
                Observe(node, random);
                bool success = Propagate();
                if (!success) return false;
            }
            else
            {
                for (int i = 0; i < wave.Length; i++) for (int t = 0; t < T; t++) if (wave[i][t]) { observed[i] = t; break; }
                return true;
            }
        }

        return true;
    }

    int NextUnobservedNode(Random random)
    {
        if (heuristic == Heuristic.Scanline)
        {
            for (int i = observedSoFar; i < wave.Length; i++)
            {
                if (!periodic && (i % MX + N > MX || i / MX + N > MY)) continue;
                if (sumsOfOnes[i] > 1)
                {
                    observedSoFar = i + 1;
                    return i;
                }
            }
            return -1;
        }

        double min = 1E+4;
        int argmin = -1;
        for (int i = 0; i < wave.Length; i++)
        {
            if (!periodic && (i % MX + N > MX || i / MX + N > MY)) continue;
            int remainingValues = sumsOfOnes[i];
            double entropy = heuristic == Heuristic.Entropy ? entropies[i] : remainingValues;
            if (remainingValues > 1 && entropy <= min)
            {
                double noise = 1E-6 * random.NextDouble();
                if (entropy + noise < min)
                {
                    min = entropy + noise;
                    argmin = i;
                }
            }
        }
        return argmin;
    }

    void Observe(int node, Random random)
    {
        bool[] possibleStates = wave[node];

        //calculates weights for current observation
        for (int t = 0; t < T; t++)
            weightDistributions[t] = possibleStates[t] ? weights[t] : 0.0;

        int r = weightDistributions.Random(random.NextDouble());

        for (int t = 0; t < T; t++)
            if (possibleStates[t] != (t == r)) //this only occurs when possible states = true due to above calculations
                Ban(node, t);
    }

    bool Propagate()
    {
        while (stacksize > 0)
        {
            (int i1, int tileState) = stack[stacksize - 1];
            stacksize--;

            int xCenter = i1 % MX;
            int yCenter = i1 / MX;

            for (int d = 0; d < 4; d++)
            {
                int xOuter = xCenter + dx[d];
                int yOuter = yCenter + dy[d];
                if (!periodic && (xOuter < 0 || yOuter < 0 || xOuter + N > MX || yOuter + N > MY)) continue;

                if (xOuter < 0) xOuter += MX;
                else if (xOuter >= MX) xOuter -= MX;
                if (yOuter < 0) yOuter += MY;
                else if (yOuter >= MY) yOuter -= MY;

                int outerIndex = xOuter + yOuter * MX;
                int[] p = propagator[d][tileState];

                //Compatability: tileLocation -> tileStates -> direction
                //Compatability: which tilelocation can become which tilestate given its neighbors limitations: if all tiles have a thing looking towards a center piece, it cannot become an empty grass tile
                int[][] compat = compatible[outerIndex];

                for (int l = 0; l < p.Length; l++)
                {
                    int t2 = p[l];
                    int[] comp = compat[t2];

                    comp[d]--;
                    if (comp[d] == 0) Ban(outerIndex, t2);
                }
            }
        }

        return sumsOfOnes[0] > 0;
    }

    void Ban(int index, int tileState)
    {
        wave[index][tileState] = false;

        int[] comp = compatible[index][tileState];
        for (int d = 0; d < 4; d++)
            comp[d] = 0;

        stack[stacksize] = (index, tileState);
        stacksize++;

        sumsOfOnes[index] -= 1;
        sumsOfWeights[index] -= weights[tileState];
        sumsOfWeightLogWeights[index] -= weightLogWeights[tileState];

        double sum = sumsOfWeights[index];
        entropies[index] = Math.Log(sum) - sumsOfWeightLogWeights[index] / sum;
    }

    void Clear()
    {
        for (int i = 0; i < wave.Length; i++)
        {
            for (int t = 0; t < T; t++)
            {
                wave[i][t] = true;
                for (int d = 0; d < 4; d++) compatible[i][t][d] = propagator[opposite[d]][t].Length;
            }

            sumsOfOnes[i] = weights.Length;
            sumsOfWeights[i] = sumOfWeights;
            sumsOfWeightLogWeights[i] = sumOfWeightLogWeights;
            entropies[i] = startingEntropy;
            observed[i] = -1;
        }
        observedSoFar = 0;

        if (ground)
        {
            for (int x = 0; x < MX; x++)
            {
                for (int t = 0; t < T - 1; t++) Ban(x + (MY - 1) * MX, t);
                for (int y = 0; y < MY - 1; y++) Ban(x + y * MX, T - 1);
            }
            Propagate();
        }
    }

    public abstract void Save(string filename);

    protected static int[] dx = { -1, 0, 1, 0 };
    protected static int[] dy = { 0, 1, 0, -1 };
    static int[] opposite = { 2, 3, 0, 1 };
}