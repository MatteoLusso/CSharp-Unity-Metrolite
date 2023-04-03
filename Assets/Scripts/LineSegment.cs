using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type {
    Tunnel,
    Station,
    Intersection,
    Terminus,
}

public class LineSection
{
    public Type type { get; set; }
    public int number { get; set; }
    public List<Vector3> controlsPoints {get; set; }
    public List<Vector3> nextStartingDirections { get; set; }
    public List<Vector3> nextStartingPoints { get; set; }
    public List<Vector3> bezierCurveBase { get; set; }
    public List<Vector3> bezierCurveFixedLenght { get; set; }
    public List<Vector3> bezierCurveLimitedAngle { get; set; }
    public List<Vector3> floorLeftPoints { get; set; }
    public List<Vector3> floorRightPoints { get; set; }
    public Mesh floorMesh { get; set; }
}
