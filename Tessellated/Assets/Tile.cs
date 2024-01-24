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
        var initialRotation = tileInfo.transform.rotation.eulerAngles;

        var quaternion = Quaternion.Euler(new Vector3(initialRotation.x, -90 * leftRotations + initialRotation.y, initialRotation.z));

        Object.Instantiate(tileInfo, position, quaternion);
    }
}