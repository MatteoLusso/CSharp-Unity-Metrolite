using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type {
    Tunnel,
    Station,
    Switch,
    Terminus,
}

public enum SwitchType {
    BiToBi,
    BiToMono,
    MonoToBi,
    MonoToNewMono,
    BiToNewMono,
}

public enum SwitchDirection {
    RightToRight,
    LeftToLeft,
    CenterToCenter,
    RightToLeft,
    LeftToRight,
    RightToCenter,
    LeftToCenter,
    CenterToNewLineForward,
    CenterToNewLineBackward,
}

public class LineSection
{
    public Type type { get; set; }
    public SwitchType switchType { get; set; }
    public bool bidirectional { get; set; }
    public bool newBidirectional { get; set; }
    public int number { get; set; }
    public List<Vector3> controlsPoints {get; set; }
    public List<Vector3> nextStartingDirections { get; set; }
    public List<Vector3> nextStartingPoints { get; set; }
    public List<Vector3> bezierCurveBase { get; set; }
    public List<Vector3> bezierCurveFixedLenght { get; set; }
    public List<Vector3> bezierCurveLimitedAngle { get; set; }
    public MeshGenerator.Floor floorPoints { get; set; }
    public Mesh floorMesh { get; set; }
    public SwitchDirection activeSwitch { get; set; }
    public Dictionary<SwitchDirection, List<GameObject>> switchLights { get; set; }
    public int curvePointsCount { get; set; }

}
