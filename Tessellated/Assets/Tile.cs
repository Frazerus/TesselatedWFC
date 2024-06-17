using UnityEngine;

public class Tile
{
    public GameObject tileInfo;

    public int leftRotations;

    public int reflections;
    private readonly int _cardinality;

    public Tile(GameObject tile, int leftRotations = 0, int reflections = -1, int cardinality = -1)
    {
        tileInfo = tile;
        this.leftRotations = leftRotations;
        this.reflections = reflections;
        _cardinality = cardinality;
    }

    public void Create(Vector3 position)
    {
        var initialRotation = tileInfo.transform.rotation.eulerAngles;

        var quaternion = Quaternion.Euler(new Vector3(initialRotation.x, -90 * leftRotations + initialRotation.y, initialRotation.z));

        var newObject = Object.Instantiate(tileInfo, position, quaternion);
        var scale = reflections switch
        {
            0 => new Vector3( 1, 1, -1),
            1 => new Vector3(1, 1, -1),
            2 => new Vector3(1, 1, -1),
            3 => new Vector3(1, 1, -1),
            _ => newObject.transform.localScale
        };

        newObject.transform.localScale = scale;
        newObject.name += $" {_cardinality}";
    }
}