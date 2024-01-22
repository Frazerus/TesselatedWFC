﻿

using System;
using System.Text;
using UnityEngine.UIElements;

public abstract class OctagonTessellationModel
{
    /// <summary>
    /// We can't use simple booleans, as we dont have a simple 2d array, therefore the objects need to know stuff themselves
    /// </summary>
    protected OctagonSquare[] Wave;

    /// <summary>
    /// array:
    ///     mainstate & sidestates
    ///     direction
    ///     tileState
    ///     corresponding possible tiles
    /// </summary>
    protected int[][][][] Propagator;

    private int _countPerIndex;

    private int[][][][][] _compatible;

    /// <summary>
    /// Each observation counts as 5, the middle on
    /// </summary>
    protected OctagonSquareObservation[] Observed;

    (int, int, int)[] stack;
    int stacksize, observedSoFar;

    private readonly int _width;
    private readonly int _height;
    private int[] _totalPossibleStates;

    protected bool periodic, ground;

    protected double[][] Weights;
    private double[][] _weightDistributions;

    double[] weightLogWeights;

    protected OctagonSquareSumOfPossibleStates[] sumsOfPossibleStates;
    double sumOfWeights, sumOfWeightLogWeights, startingEntropy;
    protected double[] sumsOfWeights, sumsOfWeightLogWeights, entropies;

    public enum Heuristic { Entropy, MRV, Scanline };
    Heuristic heuristic;

    private int currentObservedPart;


    protected OctagonTessellationModel(int octagonWidth, int octagonHeight, bool periodic, Heuristic heuristic)
    {
        _width = octagonWidth;
        _height = octagonHeight;
        this.periodic = periodic;
        this.heuristic = heuristic;
    }

    void Init()
    {
        Wave = new OctagonSquare[_width * _height];
        Observed = new OctagonSquareObservation[_width * _height];

        for (int i = 0; i < Wave.Length; i++)
        {
            Wave[i] = new OctagonSquare(_totalPossibleStates[0], _totalPossibleStates[1]);
        }

        _weightDistributions = new double[2][];
        for (int i = 0; i < _weightDistributions.Length; i++)
        {
            _weightDistributions[i] = new double[_totalPossibleStates[i]];
        }
        //TODO initialize compatability, weights, wightslogweights etc


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
                    return false;
            }
            else
            {
                for (int i = 0; i < Wave!.Length; i++)
                {
                    for (int observedState = 0; observedState < Wave[i].mainStates.Length; observedState++)
                    {
                        if (Wave[i].mainStates[observedState])
                            Observed[i].main = observedState;
                    }
                    for (int observedState = 0; observedState < Wave[i].sideStates.Length; observedState++)
                    {
                        if (Wave[i].sideStates[observedState])
                            Observed[i].side = observedState;
                    }
                }

                return true;
            }
        }

        return true;
    }

    /// <summary>
    /// Part 0 = main part, 1 = firstChild, going from left against the clock
    /// </summary>
    /// <param name="random"></param>
    /// <returns></returns>
    (int index, int part) NextUnobservedNode(Random random)
    {
        if (heuristic == Heuristic.Scanline)
        {
            //this should
            //too big probably
            for (int i = observedSoFar; i < Wave.Length; i++)
            {
                for (int j = currentObservedPart; j < _countPerIndex; j++)
                {
                    if (sumsOfPossibleStates[i].count[j] > 1)
                    {
                        observedSoFar = i + 1;
                        currentObservedPart = j + 1 % _countPerIndex;
                        return (i, j);
                    }
                }
            }
        }

        throw new NotImplementedException();
    }

    void Observe(int index, int part, Random random)
    {
        bool[] possibleStates = Wave[index].states[part];

        for (int t = 0; t < _totalPossibleStates[part]; t++)
        {
            _weightDistributions[part][t] = possibleStates[t] ? Weights[part][t] : 0.0;
        }

        int r = _weightDistributions[part].Random(random.NextDouble());

        for (int t = 0; t < _totalPossibleStates[part]; t++)
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
                    int[] p = Propagator[part][d][tileState];

                    int otherPart = d < 4 ? 0 : 1;

                    //Could be improved
                    int[][] tileCompatability = _compatible[indexOther][otherPart][part];

                    int staggeredD = d % 4;

                    for (int l = 0; l < p.Length; l++)
                    {
                        int otherTile = p[l];
                        int[] comp = tileCompatability[otherTile];

                        comp[staggeredD]--;
                        if (comp[staggeredD] == 0)
                            Ban(indexOther, part, otherTile);
                    }
                }
            }
            else if (part == 1)
            {
                for (int d = 0; d < 4; d++)
                {
                    int xOther = x + sx[d];
                    int yOther = y + sy[d];

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

                    // 1x4directions_x#sideTilestates_x#CorrespondingMainTilestates
                    int[] currentPropagator = Propagator[part][d][tileState];

                    //other part can only be 0 because only main neighbors in this case
                    int[][] tileCompatability = _compatible[indexOther][0][part];

                    for (int i = 0; i < currentPropagator.Length; i++)
                    {
                        int otherTile = currentPropagator[i];
                        int[] comp = tileCompatability[otherTile];

                        comp[d]--;
                        if (comp[d] == 0)
                            Ban(indexOther, part, otherTile);
                    }
                }
            }

        }

        //Todo change this to 2D array
        return sumsOfPossibleStates[0].count[0] > 0;
    }

    void Clear()
    {
        currentObservedPart = 0;

    }

    void Ban(int index, int part, int tileState)
    {
        Wave[index].states[part][tileState] = false;

        //TODO compatible

        stack[stacksize] = (index, part, tileState);
        stacksize++;

        sumsOfPossibleStates[index].count[part] -= 1;
        //sumsOfWeights[index][part] -= Weights[part][tileState];
        //sumsOfWeightLogWeights[index][part] -= weightLogWeights[part][tileStates];
    }

    //left down right up, sw se ne nw
    protected static int[] dx = { -1, 0, 1, 0, 0, 1, 1, 0 };
    protected static int[] dy = { 0, 1, 0, -1, 0, 0, -1, -1 };

    protected static int[] sx = { -1, 0, 0, -1 };
    protected static int[] sy = { 1, 1, 0, 0 };

}