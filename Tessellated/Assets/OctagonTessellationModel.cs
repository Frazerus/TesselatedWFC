using System;

public abstract class OctagonTessellationModel
{
    /// <summary>
    /// We can't use simple booleans, as we dont have a simple 2d array, therefore the objects need to know stuff themselves
    /// </summary>
    protected bool[][][] Wave;

    /// <summary>
    /// array dimensions:
    ///     main shape
    ///     neighboring shape
    ///     direction
    ///     main tile
    ///     corresponding possible tile
    /// </summary>
    protected int[][][][][] Propagator;

    protected readonly int ShapeCount;

    private int[][][][][] _compatible;

    protected int[][] Observed;

    private (int, int, int)[] _stack;
    private int _stackSize, _observedSoFar;

    protected readonly int Width;
    protected readonly int Height;
    protected int[] TotalPossibleStates;

    private readonly bool _periodic;

    protected double[][] Weights;
    private double[][] _weightDistributions, _sumsOfWeights, _sumsOfWeightLogWeights, _entropies;

    private double[][] _weightLogWeights;
    private double[] _startingEntropy, _sumOfWeights, _sumOfWeightLogWeights;

    private int[][] _sumsOfPossibleStates;

    private readonly Heuristic _heuristic;

    private int _currentObservedShape;
    protected int ShapeWithMostStates;


    protected OctagonTessellationModel(int octagonWidth, int octagonHeight, bool periodic, Heuristic heuristic, int shapeCount)
    {
        Width = octagonWidth;
        Height = octagonHeight;
        _periodic = periodic;
        _heuristic = heuristic;
        ShapeCount = shapeCount;
    }

    private void Init()
    {
        Wave = new bool[Width * Height][][];
        Observed = new int[Width * Height][];
        _sumsOfPossibleStates = new int [Wave.Length][];
        _sumsOfWeights = new double [Wave.Length][];
        _sumsOfWeightLogWeights = new double [Wave.Length][];
        _entropies = new double [Wave.Length][];

        for (var i = 0; i < Wave.Length; i++)
        {
            Wave[i] = new bool[ShapeCount][];

            for (var shape = 0; shape < ShapeCount; shape++)
            {
                Wave[i][shape] = new bool[TotalPossibleStates[shape]];
            }

            Observed[i] = new int[ShapeCount];

            _sumsOfPossibleStates[i] = new int[ShapeCount];
            _sumsOfWeights[i] = new double[ShapeCount];
            _sumsOfWeightLogWeights[i] = new double[ShapeCount];
            _entropies[i] = new double[ShapeCount];
        }

        _compatible = new int[Wave.Length][][][][];
        for (var i = 0; i < _compatible.Length; i++)
        {
            _compatible[i] = new int [ShapeCount][][][];
            for (var left = 0; left < ShapeCount; left++)
            {
                _compatible[i][left] = new int[ShapeCount][][];
                for (var right = 0; right < ShapeCount; right++)
                {
                    _compatible[i][left][right] = new int [TotalPossibleStates[left]][];
                    for (var j = 0; j < TotalPossibleStates[left]; j++)
                    {
                        _compatible[i][left][right][j] = new int [4];
                    }
                }
            }
        }


        _weightDistributions = new double[ShapeCount][];
        _sumOfWeights = new double [ShapeCount];
        _sumOfWeightLogWeights = new double[ShapeCount];

        _startingEntropy = new double[ShapeCount];

        _weightLogWeights = new double[ShapeCount][];
        for (var shape = 0; shape < ShapeCount; shape++)
        {
            _weightDistributions[shape] = new double[TotalPossibleStates[shape]];

            _sumOfWeights[shape] = 0;
            _sumOfWeightLogWeights[shape] = 0;

            _weightLogWeights[shape] = new double[TotalPossibleStates[shape]];

            for (var tile = 0; tile < TotalPossibleStates[shape]; tile++)
            {
                _weightLogWeights[shape][tile] = Weights[shape][tile] * Math.Log(Weights[shape][tile]);
                _sumOfWeights[shape] += Weights[shape][tile];
                _sumOfWeightLogWeights[shape] += _weightLogWeights[shape][tile];
            }

            _startingEntropy[shape] = Math.Log(_sumOfWeights[shape]) - _sumOfWeightLogWeights[shape] / _sumOfWeights[shape];
        }


        //TODO initialize compatability, weights, wightslogweights etc

        _stack = new (int, int, int)[Wave.Length * TotalPossibleStates[0] * TotalPossibleStates[1] * 4];
        _stackSize = 0;
    }

    /// <summary>
    /// Main running function of the algorithm, after pre-calculations have finished.
    /// </summary>
    public bool Run(int seed, int limit)
    {
        if (Wave == null)
            Init();

        Clear();
        Random random = new(seed);

        for (var iteration = 0; iteration < limit; iteration++)
        {
            var nodeData = NextUnobservedNode(random);
            if (nodeData.index >= 0)
            {
                Observe(nodeData.index, nodeData.shape, random);
                var success = Propagate();
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
        for (var i = 0; i < Wave!.Length; i++)
        {
            for (var shape = 0; shape < ShapeCount; shape++)
            {
                for (var observedState = 0; observedState < Wave[i][shape].Length; observedState++)
                {
                    if (Wave[i][shape][observedState])
                        Observed[i][shape] = observedState;
                }
            }
        }
    }

    private (int index, int shape) NextUnobservedNode(Random random)
    {
        if (_heuristic == Heuristic.Scanline)
        {
            //this should
            //too big probably
            for (var i = _observedSoFar; i < Wave.Length; i++)
            {
                for (var j = _currentObservedShape; j < ShapeCount; j++)
                {
                    if (_sumsOfPossibleStates[i][j] > 1)
                    {
                        _observedSoFar = i + 1;
                        _currentObservedShape = j + 1 % ShapeCount;
                        return (i, j);
                    }
                }
            }

            return (-1, -1);
        }

        var min = 1E+4;
        var minIndex = -1;
        var minShape = -1;
        for (var i = 0; i < Wave.Length * ShapeCount; i++)
        {
            var shape = i % ShapeCount;
            var index = i / ShapeCount;

            var remainingValues = _sumsOfPossibleStates[index][shape];

            double entropy;
            if (_heuristic == Heuristic.Entropy)
                entropy= _entropies[index][shape] / TotalPossibleStates[shape] * TotalPossibleStates[ShapeWithMostStates];
            else if (_heuristic == Heuristic.MRV)
                entropy = (double)remainingValues / TotalPossibleStates[shape] * TotalPossibleStates[ShapeWithMostStates];
            else
                throw new ArgumentException("No heuristic supplied");


            if (remainingValues > 1 && entropy <= min)
            {
                var noise = 1E-6 * random.NextDouble();
                if (entropy + noise < min)
                {
                    min = entropy + noise;
                    minIndex = index;
                    minShape = shape;
                }
            }
        }
        return (minIndex, minShape);
    }

    private void Observe(int index, int shape, Random random)
    {
        var possibleStates = Wave[index][shape];

        for (var t = 0; t < TotalPossibleStates[shape]; t++)
        {
            _weightDistributions[shape][t] = possibleStates[t] ? Weights[shape][t] : 0.0;
        }

        var r = _weightDistributions[shape].Random(random.NextDouble());

        for (var t = 0; t < TotalPossibleStates[shape]; t++)
        {
            if (possibleStates[t] != (t == r))
                Ban(index, shape, t);
        }
    }

    //Works only for squares and rectangles
    private bool Propagate()
    {
        while (_stackSize > 0)
        {
            var (index, part, tileState) = _stack[_stackSize - 1];
            _stackSize--;

            var x = index % Width;
            var y = index / Width;

            if (part == 0) //Main part
            {
                for (var d = 0; d < 8; d++)
                {
                    var xOther = x + dx[d];
                    var yOther = y + dy[d];

                    if (!_periodic && (xOther < 0 || yOther < 0 || xOther + 1 > Width || yOther + 1 > Height)) continue;

                    if (xOther < 0)
                        xOther += Width;
                    else if (xOther >= Width)
                        xOther -= Width;

                    if (yOther < 0)
                        yOther += Height;
                    else if (yOther >= Height)
                        yOther -= Height;

                    var indexOther = xOther + yOther * Width;
                    var otherPart = d < 4 ? 0 : 1;

                    //Which neighbors can local tile have?
                    var localPropagator = Propagator[part][otherPart][d % 4][tileState];

                    //which options does the other position have for tiles?
                    var tileCompatability = _compatible[indexOther][otherPart][part];

                    var direction = d % 4;

                    //Decrease the other positions possibility for each tile to occur, since it is not enabled through this position anymore
                    foreach (var otherTile in localPropagator)
                    {
                        var compatibilityForOtherTile = tileCompatability[otherTile];

                        compatibilityForOtherTile[direction]--;

                        //If the other position has no valid neighbors for a given tile from any direction, ban the tile
                        if (compatibilityForOtherTile[direction] == 0)
                            Ban(indexOther, otherPart, otherTile);
                    }
                }
            }
            else if (part == 1)
            {
                for (var d = 0; d < 4; d++)
                {
                    var xOther = x + sx[d];
                    var yOther = y + sy[d];

                    //TODO Periodicity error
                    if (!_periodic && (xOther < 0 || yOther < 0 || xOther + 1 > Width || yOther + 1 > Height))
                        continue;

                    if (xOther < 0)
                        xOther += Width;
                    else if (xOther >= Width)
                        xOther -= Width;

                    if (yOther < 0)
                        yOther += Height;
                    else if (yOther >= Height)
                        yOther -= Height;

                    var indexOther = xOther + yOther * Width;
                    var otherPart = 0; //squares only have octagon neighbors

                    var currentPropagator = Propagator[part][otherPart][d][tileState];

                    var tileCompatability = _compatible[indexOther][otherPart][part];

                    for (var i = 0; i < currentPropagator.Length; i++)
                    {
                        var otherTile = currentPropagator[i];
                        var comp = tileCompatability[otherTile];

                        comp[d]--;
                        if (comp[d] == 0)
                            Ban(indexOther, otherPart, otherTile);
                    }
                }
            }

        }

        return _sumsOfPossibleStates[0][0] > 0;
    }

    private void Clear()
    {
        for (var i = 0; i < Wave.Length; i++)
        {
            for (var shape = 0; shape < ShapeCount; shape++)
            {
                Observed[i][shape] = -1;
                _sumsOfPossibleStates[i][shape] = Weights[shape].Length;
                _sumsOfWeightLogWeights[i][shape] = _sumOfWeightLogWeights[shape];
                _sumsOfWeights[i][shape] = _sumOfWeightLogWeights[shape];
                _entropies[i][shape] = _startingEntropy[shape];
                for (var possibleState = 0; possibleState < TotalPossibleStates[shape]; possibleState++)
                {
                    Wave[i][shape][possibleState] = true;

                }
            }
            for (var d = 0; d < 4; d++)
            for (var left = 0; left < ShapeCount; left++)
            {
                for (var right = 0; right < ShapeCount; right++)
                {
                    for (var possibleState = 0; possibleState < TotalPossibleStates[left]; possibleState++)
                    {
                        _compatible[i][left][right][possibleState][d] = Propagator[left][right][_opposite[d]][possibleState].Length;
                    }
                }
            }
        }
        _currentObservedShape = 0;
    }

    private void Ban(int index, int shape, int tileState)
    {
        Wave[index][shape][tileState] = false;

        var  comp = _compatible[index][shape];
        for (var otherShape = 0; otherShape <  comp.Length; otherShape++)
        {
            for (var d = 0; d < 4; d++)
                comp[otherShape][tileState][d] = 0;
        }

        _stack[_stackSize] = (index, shape, tileState);
        _stackSize++;

        _sumsOfPossibleStates[index][shape] -= 1;
        _sumsOfWeights[index][shape] -= Weights[shape][tileState];
        _sumsOfWeightLogWeights[index][shape] -= _weightLogWeights[shape][tileState];

        double sum = _sumsOfWeights[index][shape];
        _entropies[index][shape] = Math.Log(sum) - _sumsOfWeightLogWeights[index][shape] / sum;
    }

    //left down right up, sw se ne nw
    private static int[] dx = { -1, 0, 1, 0, -1, 0, 0, -1 };
    private static int[] dy = { 0, 1, 0, -1, 1, 1, 0, 0 };

    private static readonly int[] _opposite = { 2, 3, 0, 1 };

    private static int[] sx = { 0, 1, 1, 0 };
    private static int[] sy = { 0, 0, -1, -1 };

}