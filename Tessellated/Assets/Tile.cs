using UnityEngine;

public class Tile
{
    public GameObject tileInfo;

    public int leftRotations;

    public int reflections;

    public Tile(GameObject tile, int leftRotations = 0, int reflections = 0)
    {
        tileInfo = tile;
        this.leftRotations = leftRotations;
        this.reflections = reflections;
    }

    public void Create(Vector3 position)
    {
        var rotation = new Quaternion();
        rotation.eulerAngles = new Vector3(0, -90 * leftRotations, 0);

        Object.Instantiate(tileInfo, position, rotation);
    }
}