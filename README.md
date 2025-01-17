# Wavefunction collapse based on tesselated grids

This is a project created for my master thesis. The goal is to create an implementation of Maxim Gumin's [WaveFunctionCollapse](https://github.com/mxgmn/WaveFunctionCollapse) which can not only handle NxN grids made up by squares, but also more general tesselated grids based on regular shapes like hexagons, triangles and squares combined.

# Algorithm
There are two instances of the WFC collapse algorithm in the project. 

## Standard implementation 
The first one is the algorithm by Maxim Gumin fitted to Unity. The bitmap implementations have been replaced with objects containing the GameObject prefabs and their relevant orientation called [Tiles](/Tessellated/Assets/Tile.cs). This enables the standard behaviour of the algorithm without chanigng much, therefore showcasing how the standard way of the algorithm performs using the Unity engine. 

The WFC algorithm consists of 2 parts, the "OverlappingModel" creates new output based on visual input (in the base case, another image), the "SimpleTiledModel" creates output based on textual (xml) and object (in the Maxim Gumin's case, images) input. This work focusses on the SimpleTiledModel at first, whether the OverlappingModel can be recreated is dependant on deadlines and complexity.

There is already a unity plugin which also implements Maxim Gumin's algorithm in Unity, but for better comparability between this implementation and the new one, a custom one was created. Porting the algorithm to unity also has the upside of allowing me to analyze the algorithm, which enables me to implement the new functionality more easily.

## Tesselated grids implementation
The first implentation is based on a octagon and square tessellation shown below. This picture is a screenshot taken from 
[Wolfram Alpha's Semiregular Tessellation site](https://mathworld.wolfram.com/SemiregularTessellation.html).

The propagator in this case is a 5d Array filled with 2x4/8xNumberOfTilesxNumberOfTiles. 2x because every main instance has 2 parts, the octagon in the middle and the square on the side. In the case of octagon, we have the 8 sides of the octagon as the next count, 4 sides in the case of the square. Then we have the number of tilestates of the currently investigated instance, so if we have 4 different version of octagons, we would have a count of 4 here.


![Octagons and Squares](octasquares.png)


## Used version info
Unity: 2022.3
