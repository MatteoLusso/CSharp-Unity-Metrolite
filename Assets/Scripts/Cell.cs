using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{ 
    public enum Content {
        Empty,
        Proibited,
        Tunnel,
        Service,
        OutsideSwitch,
        InsideSwitch,
    }

    public Vector2 coords { get; set; }
    public Vector3 spatialCoords { get; set; }
    public Vector3 startPoint { get; set; }
    public Vector3 endPoint { get; set; }
    public Vector3 startDir { get; set; }
    public Vector3 endDir { get; set; }
    public List<Vector3> controlPoints { get; set; }
    public List<Cell> newLineCells { get; set; }
    public Cell previousCell { get; set; }
    public Cell nextCell { get; set; }
    public Content content { get; set; }

}
