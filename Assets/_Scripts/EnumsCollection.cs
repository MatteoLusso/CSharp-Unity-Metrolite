public enum CardinalPoint{
    North,
    South,
    East,
    West,
    Random,
    None,
}

public enum Side{ 
    Left,
    Right,
    BothLeftAndRight,
    Top,
    Bottom,
}

public enum Orientation {
    Horizontal,
    Vertical,
}

public enum Position {
    Start,
    End,
}

public enum Direction
{
    None,
    Forward,
    Backward,
}

public enum ArrayType
{
    Line,
    Ray,
    Segment,
}

public enum Type {
    Tunnel,
    Station,
    Switch,
    MaintenanceJoint,
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
    LateralPlatform,
}

public enum MeshType {
    PerimeterWall,
    InternalWall,
    FoundationWall,
}

public enum Biome {
    Normal,
    Nature,
    Radiactive,
}

