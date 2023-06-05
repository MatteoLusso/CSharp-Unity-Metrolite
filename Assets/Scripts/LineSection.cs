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
}

public enum NewLineSide {
    Left,
    Right,
    Both,
}

public class LineStart{
    public NewLineSide newLineSide { get; set; }
    public MetroGenerator.Direction previousOrientation { get; set; }
    public MetroGenerator.Direction orientation { get; set; }
    public Vector3 dir { get; set; }
    public Vector3 pos { get; set; }
    public bool generated { get; set; }
    public string lineName { get; set; }

    public LineStart( MetroGenerator.Direction inputOrientation, NewLineSide newLineSide, Vector3 inputPos, Vector3 inputDir ) {
        this.generated = false;
        this.orientation = inputOrientation;
        this.pos = inputPos;
        this.dir = inputDir;
        this.newLineSide = newLineSide;
    }
}

public enum SwitchDirection {
    RightToRight,
    LeftToLeft,
    CenterToCenter,
    RightToLeft,
    LeftToRight,
    RightToCenter,
    LeftToCenter,
    CenterToNewLineLeftForward,
    CenterToNewLineLeftBackward,
    CenterToNewLineRightForward,
    CenterToNewLineRightBackward,

}

public class LineSection
{
    public Type type { get; set; }
    public SwitchType switchType { get; set; }
    public NewLineSide newLineSide { get; set; }
    public bool bidirectional { get; set; }
    public bool newBidirectional { get; set; }
    public int number { get; set; }
    public List<Vector3> controlsPoints {get; set; }
    public List<Vector3> nextStartingDirections { get; set; }
    public List<Vector3> nextStartingPoints { get; set; }
    public List<Vector3> bezierCurveBase { get; set; }
    public List<Vector3> bezierCurveFixedLenght { get; set; }
    public List<Vector3> bezierCurveLimitedAngle { get; set; }
    public List<Vector3> distanceControllingPoints { get; set; }
    public MeshGenerator.Floor floorPoints { get; set; }
    public Mesh floorMesh { get; set; }
    public SwitchDirection activeSwitch { get; set; }
    public Dictionary<SwitchDirection, List<GameObject>> switchLights { get; set; }
    public int curvePointsCount { get; set; }
    public LineSection fromSection { get; set; }
    public MetroGenerator.Direction orientation { get; set; }
    public string lineName { get; set; }
    public Dictionary<NewLineSide, LineStart> newLinesStarts { get; set; }
    public int sectionIndex { get; set; }

}
