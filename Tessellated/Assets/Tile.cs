using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public PossibleState State { get; set; } = PossibleState.Grass | PossibleState.Path | PossibleState.Mountain | PossibleState.Snow;
}


