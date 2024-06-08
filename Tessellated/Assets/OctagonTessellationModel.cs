using System;

public abstract class OctagonTessellationModel
{
    /// <summary>
    /// We can't use simple booleans, as we dont have a simple 2d array, therefore the objects need to know stuff themselves
    /// </summary>
    protected bool[][][] Wave;

    /// <summary>
    /// array:
    ///     mainstate & sidestates
    ///     direction
    ///     tileState
    ///     corresponding possible tiles
    /// </summary>
    protected int[][][][][] Propagator;

    protected readonly int ShapeCount;

    private int[][][][][] _compatible;

    /// <summary>
    /// Each observation counts as 5, the middle on
    /// </summary>
    protected int[][] Observed;

    (int, int, int)[] stack;
    int stacksize, observedSoFar;

    protected readonly int _width;
    protected readonly int _height;
    protected int[] TotalPossibleStates;

    protected bool periodic, ground;

    protected double[][] Weights;
    private double[][] _weightDistributions;

    double[] weightLogWeights;

    protected int[][] sumsOfPossibleStates;
    double sumOfWeights, sumOfWeightLogWeights, startingEntropy;

    public enum Heuristic { Entropy, MRV, Scanline };
    Heuristic heuristic;

    private int currentObservedPart;
    protected int _shapeWithMostStates;


    protected OctagonTessellationModel(int octagonWidth, int octagonHeight, bool periodic, Heuristic heuristic, int shapeCount)
    {
        _width = octagonWidth;
        _height = octagonHeight;
        this.periodic = periodic;
        this.heuristic = heuristic;
        ShapeCount = shapeCount;
    }

    void Init()
    {
        Wave = new bool[_width * _height][][];
        Observed = new int[_width * _height][];
        sumsOfPossibleStates = new int [Wave.Length][];

        for (int i = 0; i < Wave.Length; i++)
        {
            Wave[i] = new bool[ShapeCount][];

            for (int shape = 0; shape < ShapeCount; shape++)
            {
                Wave[i][shape] = new bool[TotalPossibleStates[shape]];
            }

            Observed[i] = new int[ShapeCount];

            sumsOfPossibleStates[i] = new int[2];
        }

        _compatible = new int[Wave.Length][][][][];
        for (int i = 0; i < _compatible.Length; i++)
        {
            _compatible[i] = new int [ShapeCount][][][];
            for (int left = 0; left < ShapeCount; left++)
            {
                _compatible[i][left] = new int[ShapeCount][][];
                for (int right = 0; right < ShapeCount; right++)
                {
                    _compatible[i][left][right] = new int [TotalPossibleStates[left]][];
                    for (int j = 0; j < TotalPossibleStates[left]; j++)
                    {
                        _compatible[i][left][right][j] = new int [4];
                    }
                }
            }
        }

        _weightDistributions = new double[2][];
        for (int i = 0; i < _weightDistributions.Length; i++)
        {
            _weightDistributions[i] = new double[TotalPossibleStates[i]];
        }

        //TODO initialize compatability, weights, wightslogweights etc

        stack = new (int, int, int)[Wave.Length * TotalPossibleStates[0] * TotalPossibleStates[1] * 4];
        stacksize = 0;

    }

    public bool Run(int seed, int limit)
    {
        if (Wave == null)
            Init();

        Clear();
        Random random = new(seed);

        for (int iteration = 0; iteration < limit; iteration++)
        {
            var nodeData = NextUnobservedNode(random);
            if (nodeData.index >= 0)
            {
                Observe(nodeData.index, nodeData.part, random);
                bool success = Propagate();
                if (!success)
                {
                    return false;
                }
            }
            else
            {
                FinalizeObservation();
                return true;
            }
        }
        return true;
    }

    private void FinalizeObservation()
    {
        for (int i = 0; i < Wave!.Length; i++)
        {
            for (int shape = 0; shape < ShapeCount; shape++)
            {
                for (int observedState = 0; observedState < Wave[i][shape].Length; observedState++)
                {
                    if (Wave[i][shape][observedState])
                        Observed[i][shape] = observedState;
                }
            }
        }
    }

    /// <summary>
    /// Part 0 = main part, 1 = firstChild, going from left against the clock
    /// </summary>
    /// <param name="random"></param>
    /// <returns></returns>
    (int index, int part) NextUnobservedNode(Random random)
    {
        if (heuristic == Heuristic.Entropy)
            throw new NotImplementedException();

        if (heuristic == Heuristic.Scanline)
        {
            //this should
            //too big probably
            for (int i = observedSoFar; i < Wave.Length; i++)
            {
                for (int j = currentObservedPart; j < ShapeCount; j++)
                {
                    if (sumsOfPossibleStates[i][j] > 1)
                    {
                        observedSoFar = i + 1;
                        currentObservedPart = j + 1 % ShapeCount;
                        return (i, j);
                    }
                }
            }

            return (-1, -1);
        }

        double min = 1E+4;
        int minIndex = -1;
        int minShape = -1;
        for (int i = 0; i < Wave.Length * ShapeCount; i++)
        {
            int remainingValues = sumsOfPossibleStates[i / ShapeCount][i % ShapeCount];

            //double entropy = (double)remainingValues / TotalPossibleStates[i % ShapeCount] * TotalPossibleStates[_shapeWithMostStates];
            double entropy = remainingValues + (TotalPossibleStates[_shapeWithMostStates] *  (i % ShapeCount));
            if (remainingValues > 1 && entropy <= min)
            {
                var noise = 1E-6 * random.NextDouble();
                if (entropy + noise < min)
                {
                    min = entropy + noise;
                    minIndex = i / ShapeCount;
                    minShape = i % ShapeCount;
                }
            }
        }

        return (minIndex, minShape);
    }

    void Observe(int index, int part, Random random)
    {
        bool[] possibleStates = Wave[index][part];

        //TODO can make weight distributions just a single dimension?
        for (int t = 0; t < TotalPossibleStates[part]; t++)
        {
            _weightDistributions[part][t] = possibleStates[t] ? Weights[part][t] : 0.0;
        }

        int r = _weightDistributions[part].Random(random.NextDouble());

        for (int t = 0; t < TotalPossibleStates[part]; t++)
        {
            if (possibleStates[t] != (t == r))
                Ban(index, part, t);
        }
    }

    //Works only for squares and rectangles
    bool Propagate()
    {
        while (stacksize > 0)
        {
            (int index, int part, int tileState) = stack[stacksize - 1];
            stacksize--;

            int x = index % _width;
            int y = index / _width;

            if (part == 0) //Main part
            {
                //main octagons from 0-3 side squares 4-7
                //TODO fix for both
                for (int d = 0; d < 8; d++)
                {
                    int xOther = x + dx[d];
                    int yOther = y + dy[d];

                    if (!periodic && (xOther < 0 || yOther < 0 || xOther + 1 > _width || yOther + 1 > _height)) continue;

                    if (xOther < 0)
                        xOther += _width;
                    else if (xOther >= _width)
                        xOther -= _width;

                    if (yOther < 0)
                        yOther += _height;
                    else if (yOther >= _height)
                        yOther -= _height;

                    int indexOther = xOther + yOther * _width;
                    int otherPart = d < 4 ? 0 : 1;

                    //Which neighbors can local tile have?
                    int[] localPropagator = Propagator[part][otherPart][d % 4][tileState];

                    //which options does the other position have for tiles?
                    int[][] tileCompatability = _compatible[indexOther][otherPart][part];

                    int direction = d % 4;

                    //Decrease the other positions possibility for each tile to occur, since it is not enabled through this position anymore
                    for (int l = 0; l < localPropagator.Length; l++)
                    {
                        int otherTile = localPropagator[l];
                        int[] compatibilityForOtherTile = tileCompatability[otherTile];

                        compatibilityForOtherTile[direction]--;

                        //If the other position has a possibility for a tile to occur from any direction equal to 0, ban the tile
                        if (compatibilityForOtherTile[direction] == 0)
                            Ban(indexOther, otherPart, otherTile);
                    }
                }
            }
            else if (part == 1)
            {
                for (int d = 0; d < 4; d++)
                {
                    int xOther = x + sx[d];
                    int yOther = y + sy[d];

                    //TODO Periodicity error
                    if (!periodic && (xOther < 0 || yOther < 0 || xOther + 1 > _width || yOther + 1 > _height))
                        continue;

                    if (xOther < 0)
                        xOther += _width;
                    else if (xOther >= _width)
                        xOther -= _width;

                    if (yOther < 0)
                        yOther += _height;
                    else if (yOther >= _height)
                        yOther -= _height;

                    int indexOther = xOther + yOther * _width;
                    int otherPart = 0; //we only have main neighbors

                    // 1x4directions_x#sideTilestates_x#CorrespondingMainTilestates
                    int[] currentPropagator = Propagator[part][otherPart][d][tileState];

                    int[][] tileCompatability = _compatible[indexOther][otherPart][part];

                    for (int i = 0; i < currentPropagator.Length; i++)
                    {
                        int otherTile = currentPropagator[i];
                        int[] comp = tileCompatability[otherTile];

                        comp[d]--;
                        if (comp[d] == 0)
                            Ban(indexOther, otherPart, otherTile);
                    }
                }
            }

        }

        return sumsOfPossibleStates[0][0] > 0;
    }

    void Clear()
    {
        for (int i = 0; i < Wave.Length; i++)
        {
            for (int shape = 0; shape < ShapeCount; shape++)
            {
                Observed[i][shape] = -1;
                sumsOfPossibleStates[i][shape] = Weights[shape].Length;
                for (int possibleState = 0; possibleState < TotalPossibleStates[shape]; possibleState++)
                {
                    Wave[i][shape][possibleState] = true;

                }
            }
            for (int d = 0; d < 4; d++)
            for (int left = 0; left < ShapeCount; left++)
            {
                for (int right = 0; right < ShapeCount; right++)
                {
                    for (int possibleState = 0; possibleState < TotalPossibleStates[left]; possibleState++)
                    {
                        _compatible[i][left][right][possibleState][d] = Propagator[left][right][opposite[d]][possibleState].Length;
                    }
                }
            }
        }
        currentObservedPart = 0;
    }

    void Ban(int index, int part, int tileState)
    {
        Wave[index][part][tileState] = false;

        /*int[] comp = _compatible[index][0][0][tileState];
        for (int d = 0; d < 4; d++)
            comp[d] = 0;
*/
        //com [4][1][0][1][1]

        //es hat x nachbarn gegeben, die dass enabled haben
        int[][][]  comp = _compatible[index][part];
        for (int otherPart = 0; otherPart <  comp.Length; otherPart++)
        {
            for (int d = 0; d < 4; d++)
                comp[otherPart][tileState][d] = 0;
        }

        stack[stacksize] = (index, part, tileState);
        stacksize++;

        sumsOfPossibleStates[index][part] -= 1;
        //sumsOfWeights[index][part] -= Weights[part][tileState];
        //sumsOfWeightLogWeights[index][part] -= weightLogWeights[part][tileStates];
    }

    //left down right up, sw se ne nw
    protected static int[] dx = { -1, 0, 1, 0, 0, 1, 1, 0 };
    protected static int[] dy = { 0, 1, 0, -1, 0, 0, -1, -1 };

    static int[] opposite = { 2, 3, 0, 1 };

    protected static int[] sx = { -1, 0, 0, -1 };
    protected static int[] sy = { 1, 1, 0, 0 };

}