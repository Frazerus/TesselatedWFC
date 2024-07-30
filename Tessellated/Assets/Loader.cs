using UnityEngine;

public static class Loader
{
    public static GameObject Load(string load)
    {
        return Resources.Load<GameObject>(load);
    }
}