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
    BiToNewBi,
}

public enum StationType {
    CentralPlatform,
    LeftSidePlatform,
    RightSidePlatform,
    BothSidesPlatform,
}

public enum NewLineSide {
    Left,
    Right,
    Both,
}

public class LineStart{
    public NewLineSide newLineSide { get; set; }
    public CardinalPoint previousOrientation { get; set; }
    public CardinalPoint orientation { get; set; }
    public Vector3 dir { get; set; }
    public Vector3 pos { get; set; }
    public bool generated { get; set; }
    public string lineName { get; set; }

    public LineStart( CardinalPoint inputOrientation, NewLineSide newLineSide, Vector3 inputPos, Vector3 inputDir ) {
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
    CenterToEntranceLeft,
    CenterToEntranceRight,
    CenterToExitLeft,
    CenterToExitRight,

    LeftToEntranceLeft,
    RightToEntranceRight,
    LeftToExitLeft,
    RightToExitRight,
}

public class LineSection
{
    public Type type { get; set; }
    public CardinalPoint mainDir { get; set; }
    public SwitchType switchType { get; set; }
    public StationType stationType { get; set; }
    public NewLineSide newLineSide { get; set; }
    public bool bidirectional { get; set; }
    public bool newBidirectional { get; set; }
    public Vector3 centerCoords { get; set; }
    public int number { get; set; }
    public GameObject sectionObj { get; set; }
    public List<Vector3> controlsPoints {get; set; }
    public List<Vector3> nextStartingDirections { get; set; }
    public List<Vector3> nextStartingPoints { get; set; }
    public List<Vector3> bezierCurveBase { get; set; }
    public List<Vector3> bezierCurveFixedLenght { get; set; }
    public List<Vector3> bezierCurveLimitedAngle { get; set; }
    public List<Vector3> distanceControllingPoints { get; set; }
    public MeshGenerator.Floor floorPoints { get; set; }
    public Mesh floorMesh { get; set; }
    public MeshGenerator.PlatformSide platformSidesPoints { get; set; }
    public Mesh platformMeshLeft { get; set; }
    public Mesh platformMeshRight { get; set; }
    public SwitchDirection activeSwitch { get; set; }
    public bool ignoreSwitch { get; set; }
    public Dictionary<SwitchDirection, List<GameObject>> switchLights { get; set; }
    public int curvePointsCount { get; set; }
    public LineSection fromSection { get; set; }
    public CardinalPoint orientation { get; set; }
    public string lineName { get; set; }
    public Dictionary<NewLineSide, LineStart> newLinesStarts { get; set; }
    public int sectionIndex { get; set; }
    public string sectionName { get; set; }
    public GameObject indicatorObj { get; set; }

    public LineSection previousSection { get; set; }

    public List<Vector3> wallLeftLastProfile { get; set; }
    public List<Vector3> wallRightLastProfile { get; set; }

    public List<Vector3> sidePlatformFloorLeftLastProfile { get; set; }
    public List<Vector3> sidePlatformFloorRightLastProfile { get; set; }
}
