namespace Benchmarks;

public class Tile
{
    public int LeftRotations { get; }
    public int Reflections { get; }
    public int Cardinality { get; }
    private readonly object _tile;

    public Tile(object tile, int leftRotations = 0, int reflections = -1, int cardinality = -1)
    {
        LeftRotations = leftRotations;
        Reflections = reflections;
        Cardinality = cardinality;
        _tile = tile;
    }
    public void Create(float x, float y, float z) {}
}