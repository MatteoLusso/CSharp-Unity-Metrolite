using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MetroGenerator : MonoBehaviour
{
    [ Header ( "Giocatore" ) ]
    public GameObject train;
    public GameObject mainCamera;
    public float trainHeightFromGround = 1.5f;

    [ Header ( "Parametri metropolitana" ) ]
    public int seed = 0;
    public bool randomSeed = false;
    public int controlPointsNumber = 3;
    public int distanceMultiplier = 25;
    public int sectionsNumber = 1;
    public float maxAngle = 2.5f;
    public int stationsDistanceMin = 4;
    public int stationsDistanceMax = 8;
    private int stationsDistance;
    public int switchDistanceMin = 2;
    public int switchDistanceMax = 10;
    private int switchDistance;
    public float stationLenght = 200.0f;
    public float stationExtensionLenght = 100.0f;
    public float stationExtensionHeight = 225.0f;
    public int stationExtensionCurvePoints = 10;
    public float switchLenght = 25.0f;
    public int baseBezierCurvePointsNumber = 50;
    public bool tunnelParabolic = false;
    public float tunnelStraightness = 0.5f;
    public bool startingBidirectional = true;
    public Dictionary<string, List<LineSection>> lines = new Dictionary<string, List<LineSection>>();
    public Dictionary<string, List<List<Vector3>>> proibitedAreas = new Dictionary<string, List<List<Vector3>>>();
    private int lineCounter = 0;
    public int lineTurnDistanceMin = 5;
    public int lineTurnDistanceMax = 20;
    private int lineTurnDistance;
    public float switchBracketsLenght = 50.0f;
    public bool newLineFromSwitch = false;

    [ Header ( "Parametri props" ) ]
    public GameObject pillar;
    public Vector3 pillarRotationCorrection = Vector3.zero;
    public Vector3 pillarPositionCorrection = Vector3.zero;
    public float pillarMinDistance = 5.0f;
    public float pillarMaxDistance = 10.0f;
    public GameObject fan;
    public Vector3 fanRotationCorrection = Vector3.zero;
    public Vector3 fanPositionCorrection = Vector3.zero;
    public float fanMinDistance = 100.0f;
    public float fanMaxDistance = 250.0f;
    public float tunnelWallHeight = 0.5f;
    public GameObject banister;
    public Vector3 banisterRotationCorrection = Vector3.zero;
    public Vector3 banisterPositionCorrectionLeft = Vector3.zero;
    public Vector3 banisterPositionCorrectionRight = Vector3.zero;
    public float banisterMinDistance = 8.5f;
    public float banisterMaxDistance = 25.0f;
    [ Header ( "Parametri tunnel" ) ]
    public float tunnelWidth = 5.0f;
    public float railsWidth = 3.0f;
    public float centerWidth = 5.0f;
    public float platformHeight = 0.5f;
    public float platformWidth = 3.5f;

    [ Header ( "Parametri muro tunnel" ) ]
    public List<Vector3> tunnelWallShape;
    public float tunnelWallShapeScale = 1.0f;
    public float tunnelWallSmoothFactor= 0.5f;
    public float tunnelWallShapeHorPosCorrection = 0.0f;

    [ Header ( "Parametri binario tunnel" ) ]
    public List<Vector3> railShape;
    public float railSmoothFactor= 0.5f;
    public float railShapeScale = 1.0f;
    public float railShapeHorPosCorrection = 0.0f;

    [ Header ( "Parametri pavimento banchina" ) ]
    public List<Vector3> platformFloorShape;
    public float platformFloorSmoothFactor= 0.5f;
    public float platformFloorShapeScale = 1.0f;
    public float platformFloorHorPosCorrection = 0.0f;

    [ Header ( "Parametri tombino scambio" ) ]

    public float grateWidth = 3.0f;
    
    [ Header ( "Texture" ) ]
    public Material tunnelRailTexture;
    public Vector2 tunnelRailTextureTilting;
    public Material platformSideTexture;
    public Vector2 platformSideTextureTilting;
    public Material platformFloorTexture;
    public Vector2 platformFloorTextureTilting;
    public Material tunnelWallTexture;
    public Vector2 tunnelWallTextureTilting;
    public Material railTexture;
    public Vector2 railTextureTilting;
    public Material switchRailTexture;
    public Vector2 switchRailTextureTilting;
    public Material switchGroundTexture;
    public Vector2 switchGroundTextureTilting;
    public Material centerTexture;
    public Vector2 centerTextureTilting;
    public Material grateTexture;
    public Vector2 grateTextureTilting;

    [ Header ( "Semafori scambi" ) ]
    public GameObject switchLight;
    public float switchLightDistance;
    public float switchLightHeight;
    public Vector3 switchLightRotation;

    [ Header ( "Debug" ) ]
    public bool lineGizmos = true;
    
    public enum Direction{
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
        Both,
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateMetro();
    }

    public void GenerateMetro() {
        
        this.seed = this.randomSeed ? ( int )Random.Range( 0, 999999 ) : this.seed;
        Random.InitState( seed ); 

        GenerateLine( "Linea " + lineCounter, Direction.None, Direction.Random, sectionsNumber, Vector3.zero, Vector3.zero, newLineFromSwitch, null );

        foreach( LineSection section in lines[ "Linea " + ( lineCounter - 1 ) ] ) {
            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                if( section.newLinesStarts != null ) {
                    List<NewLineSide> sides = new List<NewLineSide>( section.newLinesStarts.Keys );
                    for( int i = 0; i < sides.Count; i++ ) {
                        LineStart start = section.newLinesStarts[ sides[ i ] ];

                        if( !start.generated ) {
                            GenerateLine( "Linea " + lineCounter, start.previousOrientation, start.orientation, sectionsNumber/ 2, start.pos, start.dir, false, section );
                            start.generated = true;
                            start.lineName = "Linea " + ( lineCounter - 1 );

                            if( !lines.ContainsKey( "Linea " + ( lineCounter - 1 ) ) ) {
                                section.newLinesStarts.Remove( sides[ i ] );
                            }
                        }
                    }
                }
                /*foreach( NewLineSide lineStartSide in section.newLinesStarts.Keys ) {
                    if( !section.newLinesStarts[ lineStartSide ].generated ){
                        GenerateLine( "Linea " + lineCounter, section.newLinesStarts[ lineStartSide ].previousOrientation, section.newLinesStarts[ lineStartSide ].orientation, sectionsNumber/ 2, section.newLinesStarts[ lineStartSide ].pos, section.newLinesStarts[ lineStartSide ].dir, false );
                        section.newLinesStarts[ lineStartSide ].generated = true;
                        section.newLinesStarts[ lineStartSide ].lineName = "Linea " + ( lineCounter - 1 );
                    }
                }*/

                /*foreach( LineStart lineStart in section.newLinesStarts ) {
                    if( !lineStart.generated ){
                        GenerateLine( "Linea " + lineCounter, lineStart.previousOrientation, lineStart.orientation, sectionsNumber/ 4, lineStart.pos, lineStart.dir );
                        lineStart.generated = true;
                    }
                }*/
            }
        }

        GenerateMeshes();
        AddProps();
        InstantiateTrain();
    }

    private void GenerateTunnelFloorMesh( LineSection section, GameObject sectionGameObj ) {
        MeshGenerator.Floor floorVertexPoints = new MeshGenerator.Floor();
        if( section.bidirectional ) {
            Mesh leftFloorMesh = new Mesh();
            Mesh centerFloorMesh = new Mesh();
            Mesh rightFloorMesh = new Mesh();

            floorVertexPoints = MeshGenerator.CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( section, this.centerWidth, this.tunnelWidth, this.railsWidth );

            leftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.leftL, floorVertexPoints.leftR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
            GameObject leftFloorGameObj = new GameObject( "Pavimento binari sinistra" );
            leftFloorGameObj.transform.parent = sectionGameObj.transform;
            leftFloorGameObj.transform.position = Vector3.zero;
            leftFloorGameObj.AddComponent<MeshFilter>();
            leftFloorGameObj.AddComponent<MeshRenderer>();
            leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
            leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

            centerFloorMesh = MeshGenerator.GeneratePlanarMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.centerL, floorVertexPoints.centerR ), false, centerTextureTilting.x, centerTextureTilting.y );
            GameObject centerFloorGameObj = new GameObject( "Divisore centrale" );
            centerFloorGameObj.transform.parent = sectionGameObj.transform;
            centerFloorGameObj.transform.position = Vector3.zero;
            centerFloorGameObj.AddComponent<MeshFilter>();
            centerFloorGameObj.AddComponent<MeshRenderer>();
            centerFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
            centerFloorGameObj.GetComponent<MeshRenderer>().material = centerTexture;

            rightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.rightL, floorVertexPoints.rightR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
            GameObject rightFloorGameObj = new GameObject( "Pavimento binari destra" );
            rightFloorGameObj.transform.parent = sectionGameObj.transform;
            rightFloorGameObj.transform.position = Vector3.zero;
            rightFloorGameObj.AddComponent<MeshFilter>();
            rightFloorGameObj.AddComponent<MeshRenderer>();
            rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
            rightFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

            MeshGenerator.ExtrudedMesh railLeftLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, floorVertexPoints.railLeftL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
            MeshGenerator.ExtrudedMesh railLeftRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, floorVertexPoints.railLeftR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
            MeshGenerator.ExtrudedMesh railRightLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, floorVertexPoints.railRightL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
            MeshGenerator.ExtrudedMesh railRightRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, floorVertexPoints.railRightR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );

            GameObject railLeftLGameObj = new GameObject( "Binario sinistro L" );
            railLeftLGameObj.transform.parent = sectionGameObj.transform;
            railLeftLGameObj.transform.position = Vector3.zero;
            railLeftLGameObj.AddComponent<MeshFilter>();
            railLeftLGameObj.AddComponent<MeshRenderer>();
            railLeftLGameObj.GetComponent<MeshFilter>().sharedMesh = railLeftLMesh.mesh;
            railLeftLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;

            GameObject railLeftRGameObj = new GameObject( "Binario sinistro R" );
            railLeftRGameObj.transform.parent = sectionGameObj.transform;
            railLeftRGameObj.transform.position = Vector3.zero;
            railLeftRGameObj.AddComponent<MeshFilter>();
            railLeftRGameObj.AddComponent<MeshRenderer>();
            railLeftRGameObj.GetComponent<MeshFilter>().sharedMesh = railLeftRMesh.mesh;
            railLeftRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;

            GameObject railRightLGameObj = new GameObject( "Binario destro L" );
            railRightLGameObj.transform.parent = sectionGameObj.transform;
            railRightLGameObj.transform.position = Vector3.zero;
            railRightLGameObj.AddComponent<MeshFilter>();
            railRightLGameObj.AddComponent<MeshRenderer>();
            railRightLGameObj.GetComponent<MeshFilter>().sharedMesh = railRightLMesh.mesh;
            railRightLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;

            GameObject railRightRGameObj = new GameObject( "Binario destro R" );
            railRightRGameObj.transform.parent = sectionGameObj.transform;
            railRightRGameObj.transform.position = Vector3.zero;
            railRightRGameObj.AddComponent<MeshFilter>();
            railRightRGameObj.AddComponent<MeshRenderer>();
            railRightRGameObj.GetComponent<MeshFilter>().sharedMesh = railRightRMesh.mesh;
            railRightRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;

            section.bidirectional = true;
        }
        else {
            Mesh floorMesh = new Mesh();

            Vector3 startingDir = section.controlsPoints[ 1 ] - section.bezierCurveLimitedAngle[ 0 ];

            floorVertexPoints = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.bezierCurveLimitedAngle, startingDir, tunnelWidth, tunnelParabolic, this.railsWidth );

            floorMesh = MeshGenerator.GeneratePlanarMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.centerL, floorVertexPoints.centerR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );

            GameObject floorGameObj = new GameObject( "Pavimmento binari centrali" );
            floorGameObj.transform.parent = sectionGameObj.transform;
            floorGameObj.transform.position = Vector3.zero;
            floorGameObj.AddComponent<MeshFilter>();
            floorGameObj.AddComponent<MeshRenderer>();
            floorGameObj.GetComponent<MeshFilter>().sharedMesh = floorMesh;
            floorGameObj.GetComponent<MeshRenderer>().material = this.tunnelRailTexture;

            MeshGenerator.ExtrudedMesh railCenterLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, floorVertexPoints.railCenterL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
            MeshGenerator.ExtrudedMesh railCenterRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, floorVertexPoints.railCenterR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );

            GameObject railCenterLGameObj = new GameObject( "Binario centrale L" );
            railCenterLGameObj.transform.parent = sectionGameObj.transform;
            railCenterLGameObj.transform.position = Vector3.zero;
            railCenterLGameObj.AddComponent<MeshFilter>();
            railCenterLGameObj.AddComponent<MeshRenderer>();
            railCenterLGameObj.GetComponent<MeshFilter>().sharedMesh = railCenterLMesh.mesh;
            railCenterLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;

            GameObject railCenterRGameObj = new GameObject( "Binario centrale R" );
            railCenterRGameObj.transform.parent = sectionGameObj.transform;
            railCenterRGameObj.transform.position = Vector3.zero;
            railCenterRGameObj.AddComponent<MeshFilter>();
            railCenterRGameObj.AddComponent<MeshRenderer>();
            railCenterRGameObj.GetComponent<MeshFilter>().sharedMesh = railCenterRMesh.mesh;
            railCenterRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;

            section.bidirectional = false;
        }

        // Update dettagli LineSection 
        section.floorPoints = floorVertexPoints;
    }
    
    private void GenerateSwitchMeshes( string lineName, int i, LineSection section, GameObject sectionGameObj ) {
        SwitchPath switchPath = SwitchPath.CreateInstance( railsWidth, switchLenght, switchBracketsLenght, centerWidth, tunnelWidth, switchLightDistance, switchLightHeight, baseBezierCurvePointsNumber, switchLightRotation, switchLight );

        bool previousBidirectional = this.lines[ lineName ][ i - 1 ].bidirectional;

        LineSection switchSection = new LineSection();

        //Debug.Log( "previousBidirectional: " + previousBidirectional );

        if( previousBidirectional ) {
            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                switchSection = switchPath.generateBiToNewBiSwitch( section, this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
            }
            else {

                if( Random.Range( 0, 2 ) == 0 ) {
                    switchSection = switchPath.generateBiToBiSwitch( this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                }
                else{
                    switchSection = switchPath.generateBiToMonoSwitch( this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                }
            }
        }
        else {
            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                switchSection = switchPath.generateMonoToNewMonoSwitch( section, this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
            }
            else {

                if( Random.Range( 0, 2 ) == 0 || lineCounter > 0 ) {
                    switchSection = switchPath.generateMonoToBiSwitch( this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                }
            }
        }

        section.floorMesh = switchSection.floorMesh;
        section.activeSwitch = switchSection.activeSwitch;
        section.type = switchSection.type;
        section.switchType = switchSection.switchType;
        section.bidirectional = switchSection.bidirectional;
        section.switchLights = switchSection.switchLights;
        section.nextStartingDirections = switchSection.nextStartingDirections;
        section.nextStartingPoints = switchSection.nextStartingPoints;
        section.curvePointsCount = switchSection.curvePointsCount;
        section.floorPoints = switchSection.floorPoints;

        switch( section.switchType ) {
            case SwitchType.MonoToBi:
            case SwitchType.BiToMono:       // Generazione mesh pavimento binari
                                            MeshGenerator.Floor centerToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftCenterLine, null, tunnelWidth, false, this.railsWidth );
                                            MeshGenerator.Floor centerToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightCenterLine, null, tunnelWidth, false, this.railsWidth );

                                            Mesh centerToLeftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.leftCenterLine, MeshGenerator.ConvertListsToMatrix_2xM( centerToLeftFloor.centerL, centerToLeftFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                            GameObject centerToLeftFloorGameObj = new GameObject( "Pavimento Scambio Binario Centrale - Sinistro" );
                                            centerToLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                            centerToLeftFloorGameObj.transform.position = Vector3.zero;
                                            centerToLeftFloorGameObj.AddComponent<MeshFilter>();
                                            centerToLeftFloorGameObj.AddComponent<MeshRenderer>();
                                            centerToLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToLeftFloorMesh;
                                            centerToLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                            Mesh centerToRightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.rightCenterLine, MeshGenerator.ConvertListsToMatrix_2xM( centerToRightFloor.centerL, centerToRightFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                            GameObject centerToRightFloorGameObj = new GameObject( "Pavimento Scambio Binario Centrale - Destro" );
                                            centerToRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                            centerToRightFloorGameObj.transform.position = Vector3.zero;
                                            centerToRightFloorGameObj.AddComponent<MeshFilter>();
                                            centerToRightFloorGameObj.AddComponent<MeshRenderer>();
                                            centerToRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToRightFloorMesh;
                                            centerToRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                            // Generazione mesh binari
                                            MeshGenerator.ExtrudedMesh railSwitchRightCenterLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, centerToRightFloor.railCenterL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railSwitchRightCenterRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, centerToRightFloor.railCenterR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railSwitchLeftCenterLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, centerToLeftFloor.railCenterL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railSwitchLeftCenterRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, centerToLeftFloor.railCenterR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );

                                            GameObject railLeftCenterLGameObj = new GameObject( "Binario sinistra-centro L" );
                                            railLeftCenterLGameObj.transform.parent = sectionGameObj.transform;
                                            railLeftCenterLGameObj.transform.position = Vector3.zero;
                                            railLeftCenterLGameObj.AddComponent<MeshFilter>();
                                            railLeftCenterLGameObj.AddComponent<MeshRenderer>();
                                            railLeftCenterLGameObj.GetComponent<MeshFilter>().sharedMesh = railSwitchLeftCenterLMesh.mesh;
                                            railLeftCenterLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railLeftCenterLGameObj.AddComponent<RailHighlighter>();
                                            railLeftCenterLGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railLeftCenterLGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railLeftCenterLGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.LeftToCenter;

                                            GameObject railLeftCenterRGameObj = new GameObject( "Binario sinistra-centro R" );
                                            railLeftCenterRGameObj.transform.parent = sectionGameObj.transform;
                                            railLeftCenterRGameObj.transform.position = Vector3.zero;
                                            railLeftCenterRGameObj.AddComponent<MeshFilter>();
                                            railLeftCenterRGameObj.AddComponent<MeshRenderer>();
                                            railLeftCenterRGameObj.GetComponent<MeshFilter>().sharedMesh = railSwitchLeftCenterRMesh.mesh;
                                            railLeftCenterRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railLeftCenterRGameObj.AddComponent<RailHighlighter>();
                                            railLeftCenterRGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railLeftCenterRGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railLeftCenterRGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.LeftToCenter;

                                            GameObject railRightCenterLGameObj = new GameObject( "Binario destra-centro L" );
                                            railRightCenterLGameObj.transform.parent = sectionGameObj.transform;
                                            railRightCenterLGameObj.transform.position = Vector3.zero;
                                            railRightCenterLGameObj.AddComponent<MeshFilter>();
                                            railRightCenterLGameObj.AddComponent<MeshRenderer>();
                                            railRightCenterLGameObj.GetComponent<MeshFilter>().sharedMesh = railSwitchRightCenterLMesh.mesh;
                                            railRightCenterLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railRightCenterLGameObj.AddComponent<RailHighlighter>();
                                            railRightCenterLGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railRightCenterLGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railRightCenterLGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.RightToCenter;

                                            GameObject railRightCenterRGameObj = new GameObject( "Binario destra-centro R" );
                                            railRightCenterRGameObj.transform.parent = sectionGameObj.transform;
                                            railRightCenterRGameObj.transform.position = Vector3.zero;
                                            railRightCenterRGameObj.AddComponent<MeshFilter>();
                                            railRightCenterRGameObj.AddComponent<MeshRenderer>();
                                            railRightCenterRGameObj.GetComponent<MeshFilter>().sharedMesh = railSwitchRightCenterRMesh.mesh;
                                            railRightCenterRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railRightCenterRGameObj.AddComponent<RailHighlighter>();
                                            railRightCenterRGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railRightCenterRGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railRightCenterRGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.RightToCenter;

                                            // Generazione mesh piattaforma laterale
                                            Mesh platformSideLeftMesh = new Mesh();
                                            Mesh platformSideRightMesh = new Mesh();
                                            MeshGenerator.ExtrudedMesh platformFloorLeftMesh = new MeshGenerator.ExtrudedMesh();
                                            MeshGenerator.ExtrudedMesh platformFloorRightMesh = new MeshGenerator.ExtrudedMesh();

                                            MeshGenerator.PlatformSide platformSidesLeftVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( centerToLeftFloor.centerLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );
                                            MeshGenerator.PlatformSide platformSidesRightVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( centerToRightFloor.centerLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );

                                            platformSideLeftMesh = MeshGenerator.GeneratePlanarMesh(centerToLeftFloor.centerLine, MeshGenerator.ConvertListsToMatrix_2xM( platformSidesLeftVertexPoints.leftUp, platformSidesLeftVertexPoints.leftDown ), false, this.platformSideTextureTilting.x, this.platformSideTextureTilting.y );
                                            platformSideRightMesh = MeshGenerator.GeneratePlanarMesh( centerToRightFloor.centerLine, MeshGenerator.ConvertListsToMatrix_2xM( platformSidesRightVertexPoints.rightDown, platformSidesRightVertexPoints.rightUp ), false, this.platformSideTextureTilting.x, this.platformSideTextureTilting.y );

                                            MeshGenerator.SpecularBaseLine platformFloorBaseLines = new MeshGenerator.SpecularBaseLine(); 

                                            float distanceFromCenter = section.bidirectional ? ( this.tunnelWidth + ( this.centerWidth / 2 ) ) : ( this.tunnelWidth / 2 );

                                            platformFloorLeftMesh = MeshGenerator.GenerateExtrudedMesh( this.platformFloorShape, this.platformFloorShapeScale, section.previousSection != null ? section.previousSection.sidePlatformFloorLeftLastProfile : null, platformSidesLeftVertexPoints.leftFloorRight, this.platformFloorHorPosCorrection, true, this.platformFloorTextureTilting.x, this.platformFloorTextureTilting.y, 0.0f, this.platformFloorSmoothFactor );
                                            platformFloorRightMesh = MeshGenerator.GenerateExtrudedMesh( this.platformFloorShape, this.platformFloorShapeScale, section.previousSection != null ? section.previousSection.sidePlatformFloorRightLastProfile : null, platformSidesRightVertexPoints.rightFloorLeft, this.platformFloorHorPosCorrection, false, this.platformFloorTextureTilting.x, this.platformFloorTextureTilting.y, 180.0f, this.platformFloorSmoothFactor );

                                            GameObject platformSideLeftGameObj = new GameObject( "Piattaforma sinistra (lato)" );
                                            platformSideLeftGameObj.transform.parent = sectionGameObj.transform;
                                            platformSideLeftGameObj.transform.position = Vector3.zero;
                                            platformSideLeftGameObj.AddComponent<MeshFilter>();
                                            platformSideLeftGameObj.AddComponent<MeshRenderer>();
                                            platformSideLeftGameObj.GetComponent<MeshFilter>().sharedMesh = platformSideLeftMesh;
                                            platformSideLeftGameObj.GetComponent<MeshRenderer>().material = platformSideTexture;

                                            GameObject platformSideRightGameObj = new GameObject( "Piattaforma destra (lato)" );
                                            platformSideRightGameObj.transform.parent = sectionGameObj.transform;
                                            platformSideRightGameObj.transform.position = Vector3.zero;
                                            platformSideRightGameObj.AddComponent<MeshFilter>();
                                            platformSideRightGameObj.AddComponent<MeshRenderer>();
                                            platformSideRightGameObj.GetComponent<MeshFilter>().sharedMesh = platformSideRightMesh;
                                            platformSideRightGameObj.GetComponent<MeshRenderer>().material = platformSideTexture;

                                            GameObject platformFloorLeftGameObj = new GameObject( "Piattaforma sinistra (pavimento)" );
                                            platformFloorLeftGameObj.transform.parent = sectionGameObj.transform;
                                            platformFloorLeftGameObj.transform.position = Vector3.zero;
                                            platformFloorLeftGameObj.AddComponent<MeshFilter>();
                                            platformFloorLeftGameObj.AddComponent<MeshRenderer>();
                                            platformFloorLeftGameObj.GetComponent<MeshFilter>().sharedMesh = platformFloorLeftMesh.mesh;
                                            platformFloorLeftGameObj.GetComponent<MeshRenderer>().material = platformFloorTexture;

                                            GameObject platformFloorRightGameObj = new GameObject( "Piattaforma destra (pavimento)" );
                                            platformFloorRightGameObj.transform.parent = sectionGameObj.transform;
                                            platformFloorRightGameObj.transform.position = Vector3.zero;
                                            platformFloorRightGameObj.AddComponent<MeshFilter>();
                                            platformFloorRightGameObj.AddComponent<MeshRenderer>();
                                            platformFloorRightGameObj.GetComponent<MeshFilter>().sharedMesh = platformFloorRightMesh.mesh;
                                            platformFloorRightGameObj.GetComponent<MeshRenderer>().material = platformFloorTexture;

                                            section.sidePlatformFloorLeftLastProfile = platformFloorLeftMesh.lastProfileVertex;
                                            section.sidePlatformFloorRightLastProfile = platformFloorRightMesh.lastProfileVertex;

                                            // Generazione mesh muri

                                            MeshGenerator.ExtrudedMesh wallLeftMesh = new MeshGenerator.ExtrudedMesh();
                                            MeshGenerator.ExtrudedMesh wallRightMesh = new MeshGenerator.ExtrudedMesh();

                                            float baseWidth = ( this.tunnelWidth / 2 ) + this.platformWidth;

                                            float angleFromCenter = Mathf.Atan( this.platformHeight / baseWidth ) * Mathf.Rad2Deg;
                                            distanceFromCenter = Mathf.Sqrt( Mathf.Pow( this.platformHeight, 2 ) + Mathf.Pow( baseWidth, 2 ) );

                                            Vector3? lastDir = section.previousSection == null ? null : ( Vector3? )( section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 1 ] - section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 2 ] );
                                            MeshGenerator.SpecularBaseLine wallBaseLinesLeft = MeshGenerator.CalculateBaseLinesFromCurve( centerToLeftFloor.centerLine, lastDir, distanceFromCenter, angleFromCenter );
                                            MeshGenerator.SpecularBaseLine wallBaseLinesRight = MeshGenerator.CalculateBaseLinesFromCurve( centerToRightFloor.centerLine, lastDir, distanceFromCenter, angleFromCenter );

                                            wallLeftMesh = MeshGenerator.GenerateExtrudedMesh( this.tunnelWallShape, this.tunnelWallShapeScale, section.previousSection != null ? section.previousSection.wallLeftLastProfile : null, wallBaseLinesLeft.left, 0.0f, true, this.tunnelWallTextureTilting.x, this.tunnelWallTextureTilting.y, 0.0f, this.tunnelWallSmoothFactor );
                                            wallRightMesh = MeshGenerator.GenerateExtrudedMesh( this.tunnelWallShape, this.tunnelWallShapeScale, section.previousSection != null ? section.previousSection.wallRightLastProfile : null, wallBaseLinesRight.right, 0.0f, false, this.tunnelWallTextureTilting.x, this.tunnelWallTextureTilting.y, 180.0f, this.tunnelWallSmoothFactor );

                                            GameObject wallLeftGameObj = new GameObject( "Muro sinistro" );
                                            wallLeftGameObj.transform.parent = sectionGameObj.transform;
                                            wallLeftGameObj.transform.position = Vector3.zero;
                                            wallLeftGameObj.AddComponent<MeshFilter>();
                                            wallLeftGameObj.AddComponent<MeshRenderer>();
                                            wallLeftGameObj.GetComponent<MeshFilter>().sharedMesh = wallLeftMesh.mesh;
                                            wallLeftGameObj.GetComponent<MeshRenderer>().material = this.tunnelWallTexture;

                                            GameObject wallRightGameObj = new GameObject( "Muro destro" );
                                            wallRightGameObj.transform.parent = sectionGameObj.transform;
                                            wallRightGameObj.transform.position = Vector3.zero;
                                            wallRightGameObj.AddComponent<MeshFilter>();
                                            wallRightGameObj.AddComponent<MeshRenderer>();
                                            wallRightGameObj.GetComponent<MeshFilter>().sharedMesh = wallRightMesh.mesh;
                                            wallRightGameObj.GetComponent<MeshRenderer>().material = this.tunnelWallTexture;

                                            section.wallLeftLastProfile = wallLeftMesh.lastProfileVertex;
                                            section.wallRightLastProfile = wallRightMesh.lastProfileVertex;

                                            // Generazione mesh tombini

                                            Vector3 dirLeft0 = ( platformSidesLeftVertexPoints.leftFloorLeft[ 1 ] - platformSidesLeftVertexPoints.leftFloorLeft[ 0 ] ).normalized;
                                            Vector3 dirRight0 = ( platformSidesRightVertexPoints.rightFloorRight[ 1 ] - platformSidesRightVertexPoints.rightFloorRight[ 0 ] ).normalized;
                                            Vector3 dirLeft1 = ( platformSidesLeftVertexPoints.leftFloorLeft[ platformSidesLeftVertexPoints.leftFloorLeft.Count - 1 ] - platformSidesLeftVertexPoints.leftFloorLeft[ platformSidesLeftVertexPoints.leftFloorLeft.Count - 2 ] ).normalized;
                                            Vector3 dirRight1 = ( platformSidesRightVertexPoints.rightFloorRight[ platformSidesRightVertexPoints.rightFloorRight.Count - 1 ] - platformSidesRightVertexPoints.rightFloorRight[ platformSidesRightVertexPoints.rightFloorRight.Count -2 ] ).normalized;

                                            List<Vector3> grate0Up_Mono = new List<Vector3>();
                                            grate0Up_Mono.Add( centerToRightFloor.centerR[ 0 ] );
                                            grate0Up_Mono.Add( centerToLeftFloor.centerL[ 0 ] );
                                            List<Vector3> grate0Down_Mono = new List<Vector3>();
                                            grate0Down_Mono.Add( grate0Up_Mono[ 0 ] + ( dirLeft0 * this.grateWidth ) );
                                            grate0Down_Mono.Add( grate0Up_Mono[ 1 ] + ( dirRight0 * this.grateWidth ) );
                                            Mesh grate0Mesh_Mono = MeshGenerator.GeneratePlanarMesh( grate0Down_Mono, MeshGenerator.ConvertListsToMatrix_2xM( grate0Up_Mono, grate0Down_Mono ), false, this.grateTextureTilting.x, this.grateTextureTilting.y );
                                            GameObject grate0GameObj_Mono = new GameObject( "Tombino iniziale" );
                                            grate0GameObj_Mono.transform.parent = sectionGameObj.transform;
                                            grate0GameObj_Mono.transform.position = new Vector3( 0.0f, 0.0f, -0.01f );
                                            grate0GameObj_Mono.AddComponent<MeshFilter>();
                                            grate0GameObj_Mono.AddComponent<MeshRenderer>();
                                            grate0GameObj_Mono.GetComponent<MeshFilter>().sharedMesh = grate0Mesh_Mono;
                                            grate0GameObj_Mono.GetComponent<MeshRenderer>().material = this.grateTexture;

                                            List<Vector3> grate1Down_Mono = new List<Vector3>();
                                            grate1Down_Mono.Add( centerToRightFloor.centerR[ centerToRightFloor.centerR.Count - 1 ] );
                                            grate1Down_Mono.Add( centerToLeftFloor.centerL[ centerToLeftFloor.centerL.Count - 1 ] );
                                            List<Vector3> grate1Up_Mono = new List<Vector3>();
                                            grate1Up_Mono.Add( grate1Down_Mono[ 0 ] - ( dirLeft1 * this.grateWidth ) );
                                            grate1Up_Mono.Add( grate1Down_Mono[ 1 ] - ( dirRight1 * this.grateWidth ) );
                                            Mesh grate1Mesh_Mono = MeshGenerator.GeneratePlanarMesh( grate1Down_Mono, MeshGenerator.ConvertListsToMatrix_2xM( grate1Up_Mono, grate1Down_Mono ), false, this.grateTextureTilting.x, this.grateTextureTilting.y );
                                            GameObject grate1GameObj_Mono = new GameObject( "Tombino finale" );
                                            grate1GameObj_Mono.transform.parent = sectionGameObj.transform;
                                            grate1GameObj_Mono.transform.position = new Vector3( 0.0f, 0.0f, -0.01f );
                                            grate1GameObj_Mono.AddComponent<MeshFilter>();
                                            grate1GameObj_Mono.AddComponent<MeshRenderer>();
                                            grate1GameObj_Mono.GetComponent<MeshFilter>().sharedMesh = grate1Mesh_Mono;
                                            grate1GameObj_Mono.GetComponent<MeshRenderer>().material = this.grateTexture;

                                            // Generazione mesh terreno

                                            List<Vector3> groundDown_Mono = new List<Vector3>();
                                            List<Vector3> groundUp_Mono = new List<Vector3>();

                                            groundDown_Mono.Add( grate0Down_Mono[ 0 ] );
                                            groundUp_Mono.Add( grate0Down_Mono[ 1 ] );

                                            for( int k = 1; k < centerToRightFloor.centerR.Count - 2; k++ ) {
                                                groundDown_Mono.Add( centerToRightFloor.centerR[ k ] );
                                                groundUp_Mono.Add( centerToLeftFloor.centerL[ k ] );
                                            }

                                            groundUp_Mono.Add( grate1Up_Mono[ 1 ] );
                                            groundDown_Mono.Add( grate1Up_Mono[ 0 ] );

                                            Mesh groundMesh_Mono = MeshGenerator.GeneratePlanarMesh( groundDown_Mono, MeshGenerator.ConvertListsToMatrix_2xM( groundUp_Mono, groundDown_Mono ), true, this.switchGroundTextureTilting.x, this.switchGroundTextureTilting.y );
                                            GameObject groundGameObj_Mono = new GameObject( "Pavimentazione base scambio" );
                                            groundGameObj_Mono.transform.parent = sectionGameObj.transform;
                                            groundGameObj_Mono.transform.position = new Vector3( 0.0f, 0.0f, 0.01f );
                                            groundGameObj_Mono.AddComponent<MeshFilter>();
                                            groundGameObj_Mono.AddComponent<MeshRenderer>();
                                            groundGameObj_Mono.GetComponent<MeshFilter>().sharedMesh = groundMesh_Mono;
                                            groundGameObj_Mono.GetComponent<MeshRenderer>().material = this.switchGroundTexture;
                                            
                                            break;

            case SwitchType.BiToBi:         MeshGenerator.Floor leftToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, null, tunnelWidth, false, this.railsWidth );
                                            MeshGenerator.Floor rightToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, null, tunnelWidth, false, this.railsWidth );
                                            MeshGenerator.Floor leftToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftRightLine, null, tunnelWidth, false, this.railsWidth );
                                            MeshGenerator.Floor rightToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLeftLine, null, tunnelWidth, false, this.railsWidth );
                                            

                                            Mesh leftToLeftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.leftLine, MeshGenerator.ConvertListsToMatrix_2xM( leftToLeftFloor.centerL, leftToLeftFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                            GameObject leftToLeftFloorGameObj = new GameObject( "Pavimento Scambio Binario Sinistro - Sinistro" );
                                            leftToLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                            leftToLeftFloorGameObj.transform.position = Vector3.zero;
                                            leftToLeftFloorGameObj.AddComponent<MeshFilter>();
                                            leftToLeftFloorGameObj.AddComponent<MeshRenderer>();
                                            leftToLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftToLeftFloorMesh;
                                            leftToLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                            Mesh rightToRightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.rightLine, MeshGenerator.ConvertListsToMatrix_2xM( rightToRightFloor.centerL, rightToRightFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                            GameObject rightToRightFloorGameObj = new GameObject( "Pavimento Scambio Binario Destro - Destro" );
                                            rightToRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                            rightToRightFloorGameObj.transform.position = Vector3.zero;
                                            rightToRightFloorGameObj.AddComponent<MeshFilter>();
                                            rightToRightFloorGameObj.AddComponent<MeshRenderer>();
                                            rightToRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightToRightFloorMesh;
                                            rightToRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                            Mesh leftToRightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.leftRightLine, MeshGenerator.ConvertListsToMatrix_2xM( leftToRightFloor.centerL, leftToRightFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                            GameObject leftToRightFloorGameObj = new GameObject( "Pavimento Scambio Binario Sinistro - Destro" );
                                            leftToRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                            leftToRightFloorGameObj.transform.position = Vector3.zero;
                                            leftToRightFloorGameObj.AddComponent<MeshFilter>();
                                            leftToRightFloorGameObj.AddComponent<MeshRenderer>();
                                            leftToRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftToRightFloorMesh;
                                            leftToRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                            Mesh rightToLeftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.rightLeftLine, MeshGenerator.ConvertListsToMatrix_2xM( rightToLeftFloor.centerL, rightToLeftFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                            GameObject rightToLeftFloorGameObj = new GameObject( "Pavimento Scambio Binario Destro - Sinistro" );
                                            rightToLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                            rightToLeftFloorGameObj.transform.position = Vector3.zero;
                                            rightToLeftFloorGameObj.AddComponent<MeshFilter>();
                                            rightToLeftFloorGameObj.AddComponent<MeshRenderer>();
                                            rightToLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightToLeftFloorMesh;
                                            rightToLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                            MeshGenerator.ExtrudedMesh railLeftLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, section.floorPoints.railLeftL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railLeftRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, section.floorPoints.railLeftR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railRightLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, section.floorPoints.railRightL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railRightRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, section.floorPoints.railRightR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railSwitchRightLeftLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, section.floorPoints.railSwitchRightLeftL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railSwitchRightLeftRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, section.floorPoints.railSwitchRightLeftR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railSwitchLeftRightLMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, section.floorPoints.railSwitchLeftRightL, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );
                                            MeshGenerator.ExtrudedMesh railSwitchLeftRightRMesh = MeshGenerator.GenerateExtrudedMesh( this.railShape, this.railShapeScale, null, section.floorPoints.railSwitchLeftRightR, this.railShapeHorPosCorrection, true, this.railTextureTilting.x, this.railTextureTilting.y, 0.0f, this.railSmoothFactor );

                                            GameObject railLeftLGameObj = new GameObject( "Binario sinistro L" );
                                            railLeftLGameObj.transform.parent = sectionGameObj.transform;
                                            railLeftLGameObj.transform.position = Vector3.zero;
                                            railLeftLGameObj.AddComponent<MeshFilter>();
                                            railLeftLGameObj.AddComponent<MeshRenderer>();
                                            railLeftLGameObj.GetComponent<MeshFilter>().sharedMesh = railLeftLMesh.mesh;
                                            railLeftLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railLeftLGameObj.AddComponent<RailHighlighter>();
                                            railLeftLGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railLeftLGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railLeftLGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.LeftToLeft;

                                            GameObject railLeftRGameObj = new GameObject( "Binario sinistro R" );
                                            railLeftRGameObj.transform.parent = sectionGameObj.transform;
                                            railLeftRGameObj.transform.position = Vector3.zero;
                                            railLeftRGameObj.AddComponent<MeshFilter>();
                                            railLeftRGameObj.AddComponent<MeshRenderer>();
                                            railLeftRGameObj.GetComponent<MeshFilter>().sharedMesh = railLeftRMesh.mesh;
                                            railLeftRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railLeftRGameObj.AddComponent<RailHighlighter>();
                                            railLeftRGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railLeftRGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railLeftRGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.LeftToLeft;

                                            GameObject railRightLGameObj = new GameObject( "Binario destro L" );
                                            railRightLGameObj.transform.parent = sectionGameObj.transform;
                                            railRightLGameObj.transform.position = Vector3.zero;
                                            railRightLGameObj.AddComponent<MeshFilter>();
                                            railRightLGameObj.AddComponent<MeshRenderer>();
                                            railRightLGameObj.GetComponent<MeshFilter>().sharedMesh = railRightLMesh.mesh;
                                            railRightLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railRightLGameObj.AddComponent<RailHighlighter>();
                                            railRightLGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railRightLGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railRightLGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.RightToRight;

                                            GameObject railRightRGameObj = new GameObject( "Binario destro R" );
                                            railRightRGameObj.transform.parent = sectionGameObj.transform;
                                            railRightRGameObj.transform.position = Vector3.zero;
                                            railRightRGameObj.AddComponent<MeshFilter>();
                                            railRightRGameObj.AddComponent<MeshRenderer>();
                                            railRightRGameObj.GetComponent<MeshFilter>().sharedMesh = railRightRMesh.mesh;
                                            railRightRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railRightRGameObj.AddComponent<RailHighlighter>();
                                            railRightRGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railRightRGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railRightRGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.RightToRight;

                                            GameObject railSwitchRightLeftRGameObj = new GameObject( "Binario destra-sinistra R" );
                                            railSwitchRightLeftRGameObj.transform.parent = sectionGameObj.transform;
                                            railSwitchRightLeftRGameObj.transform.position = Vector3.zero;
                                            railSwitchRightLeftRGameObj.AddComponent<MeshFilter>();
                                            railSwitchRightLeftRGameObj.AddComponent<MeshRenderer>();
                                            railSwitchRightLeftRGameObj.GetComponent<MeshFilter>().sharedMesh = railSwitchRightLeftRMesh.mesh;
                                            railSwitchRightLeftRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railSwitchRightLeftRGameObj.AddComponent<RailHighlighter>();
                                            railSwitchRightLeftRGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railSwitchRightLeftRGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railSwitchRightLeftRGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.RightToLeft;

                                            GameObject railSwitchRightLeftLGameObj = new GameObject( "Binario destra-sinistra L" );
                                            railSwitchRightLeftLGameObj.transform.parent = sectionGameObj.transform;
                                            railSwitchRightLeftLGameObj.transform.position = Vector3.zero;
                                            railSwitchRightLeftLGameObj.AddComponent<MeshFilter>();
                                            railSwitchRightLeftLGameObj.AddComponent<MeshRenderer>();
                                            railSwitchRightLeftLGameObj.GetComponent<MeshFilter>().sharedMesh = railSwitchRightLeftLMesh.mesh;
                                            railSwitchRightLeftLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railSwitchRightLeftLGameObj.AddComponent<RailHighlighter>();
                                            railSwitchRightLeftLGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railSwitchRightLeftLGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railSwitchRightLeftLGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.RightToLeft;

                                            GameObject railSwitchLeftRightRGameObj = new GameObject( "Binario sinistra-destra R" );
                                            railSwitchLeftRightRGameObj.transform.parent = sectionGameObj.transform;
                                            railSwitchLeftRightRGameObj.transform.position = Vector3.zero;
                                            railSwitchLeftRightRGameObj.AddComponent<MeshFilter>();
                                            railSwitchLeftRightRGameObj.AddComponent<MeshRenderer>();
                                            railSwitchLeftRightRGameObj.GetComponent<MeshFilter>().sharedMesh = railSwitchLeftRightRMesh.mesh;
                                            railSwitchLeftRightRGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railSwitchLeftRightRGameObj.AddComponent<RailHighlighter>();
                                            railSwitchLeftRightRGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railSwitchLeftRightRGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railSwitchLeftRightRGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.LeftToRight;

                                            GameObject railSwitchLeftRightLGameObj = new GameObject( "Binario sinistra-destra L" );
                                            railSwitchLeftRightLGameObj.transform.parent = sectionGameObj.transform;
                                            railSwitchLeftRightLGameObj.transform.position = Vector3.zero;
                                            railSwitchLeftRightLGameObj.AddComponent<MeshFilter>();
                                            railSwitchLeftRightLGameObj.AddComponent<MeshRenderer>();
                                            railSwitchLeftRightLGameObj.GetComponent<MeshFilter>().sharedMesh = railSwitchLeftRightLMesh.mesh;
                                            railSwitchLeftRightLGameObj.GetComponent<MeshRenderer>().material = this.railTexture;
                                            railSwitchLeftRightLGameObj.AddComponent<RailHighlighter>();
                                            railSwitchLeftRightLGameObj.GetComponent<RailHighlighter>().line = section.lineName;
                                            railSwitchLeftRightLGameObj.GetComponent<RailHighlighter>().index = section.sectionIndex;
                                            railSwitchLeftRightLGameObj.GetComponent<RailHighlighter>().direction = SwitchDirection.LeftToRight;

                                            Vector3 dir = ( section.floorPoints.leftLine[ section.floorPoints.leftLine.Count - 1 ] - section.floorPoints.leftLine[ section.floorPoints.leftLine.Count - 2 ] ).normalized;

                                            List<Vector3> grate0Up = new List<Vector3>();
                                            grate0Up.Add( rightToRightFloor.centerR[ 0 ] );
                                            grate0Up.Add( leftToLeftFloor.centerL[ 0 ] );
                                            List<Vector3> grate0Down = new List<Vector3>();
                                            grate0Down.Add( grate0Up[ 0 ] + ( dir * this.grateWidth ) );
                                            grate0Down.Add( grate0Up[ 1 ] + ( dir * this.grateWidth ) );
                                            Mesh grate0Mesh = MeshGenerator.GeneratePlanarMesh( grate0Down, MeshGenerator.ConvertListsToMatrix_2xM( grate0Up, grate0Down ), false, this.grateTextureTilting.x, this.grateTextureTilting.y );
                                            GameObject grate0GameObj = new GameObject( "Tombino iniziale" );
                                            grate0GameObj.transform.parent = sectionGameObj.transform;
                                            grate0GameObj.transform.position = new Vector3( 0.0f, 0.0f, -0.01f );
                                            grate0GameObj.AddComponent<MeshFilter>();
                                            grate0GameObj.AddComponent<MeshRenderer>();
                                            grate0GameObj.GetComponent<MeshFilter>().sharedMesh = grate0Mesh;
                                            grate0GameObj.GetComponent<MeshRenderer>().material = this.grateTexture;

                                            List<Vector3> grate1Down = new List<Vector3>();
                                            grate1Down.Add( rightToRightFloor.centerR[ rightToRightFloor.centerR.Count - 1 ] );
                                            grate1Down.Add( leftToLeftFloor.centerL[ leftToLeftFloor.centerL.Count - 1 ] );
                                            List<Vector3> grate1Up = new List<Vector3>();
                                            grate1Up.Add( grate1Down[ 0 ] - ( dir * this.grateWidth ) );
                                            grate1Up.Add( grate1Down[ 1 ] - ( dir * this.grateWidth ) );
                                            Mesh grate1Mesh = MeshGenerator.GeneratePlanarMesh( grate1Down, MeshGenerator.ConvertListsToMatrix_2xM( grate1Up, grate1Down ), false, this.grateTextureTilting.x, this.grateTextureTilting.y );
                                            GameObject grate1GameObj = new GameObject( "Tombino finale" );
                                            grate1GameObj.transform.parent = sectionGameObj.transform;
                                            grate1GameObj.transform.position = new Vector3( 0.0f, 0.0f, -0.01f );
                                            grate1GameObj.AddComponent<MeshFilter>();
                                            grate1GameObj.AddComponent<MeshRenderer>();
                                            grate1GameObj.GetComponent<MeshFilter>().sharedMesh = grate1Mesh;
                                            grate1GameObj.GetComponent<MeshRenderer>().material = this.grateTexture;

                                            List<Vector3> groundDown = new List<Vector3>();
                                            groundDown.Add( grate0Down[ 0 ] );
                                            groundDown.Add( grate1Up[ 0 ] );
                                            List<Vector3> groundUp = new List<Vector3>();
                                            groundUp.Add( grate0Down[ 1 ] );
                                            groundUp.Add( grate1Up[ 1 ] );
                                            Mesh groundMesh = MeshGenerator.GeneratePlanarMesh( groundDown, MeshGenerator.ConvertListsToMatrix_2xM( groundUp, groundDown ), false, this.switchGroundTextureTilting.x, this.switchGroundTextureTilting.y );
                                            GameObject groundGameObj = new GameObject( "Pavimentazione base scambio" );
                                            groundGameObj.transform.parent = sectionGameObj.transform;
                                            groundGameObj.transform.position = new Vector3( 0.0f, 0.0f, 0.01f );;
                                            groundGameObj.AddComponent<MeshFilter>();
                                            groundGameObj.AddComponent<MeshRenderer>();
                                            groundGameObj.GetComponent<MeshFilter>().sharedMesh = groundMesh;
                                            groundGameObj.GetComponent<MeshRenderer>().material = this.switchGroundTexture;

                                            GenerateTunnelSidePlatformMesh( section, sectionGameObj );
                                            GenerateTunnelWallMesh( section, sectionGameObj );
                                            
                                            break;

            case SwitchType.MonoToNewMono:  MeshGenerator.Floor centerToCenterFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerLine, null, tunnelWidth, false, this.railsWidth );
                                            
                                            Mesh centerToCenterFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.centerLine, MeshGenerator.ConvertListsToMatrix_2xM( centerToCenterFloor.centerL, centerToCenterFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                            GameObject centerToCenterFloorGameObj = new GameObject( "Scambio Binario Centrale - Centrale" );
                                            centerToCenterFloorGameObj.transform.parent = sectionGameObj.transform;
                                            centerToCenterFloorGameObj.transform.position = Vector3.zero;
                                            centerToCenterFloorGameObj.AddComponent<MeshFilter>();
                                            centerToCenterFloorGameObj.AddComponent<MeshRenderer>();
                                            centerToCenterFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToCenterFloorMesh;
                                            centerToCenterFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Left ) ) {
                                                MeshGenerator.Floor centerToEntranceLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerEntranceLeft, null, tunnelWidth, false, this.railsWidth );
                                                Mesh centerToEntranceLeftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.centerEntranceLeft, MeshGenerator.ConvertListsToMatrix_2xM( centerToEntranceLeftFloor.centerL, centerToEntranceLeftFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject centerToEntranceLeftFloorGameObj = new GameObject( "Scambio Binario Centrale - Ingresso Sinistro" );
                                                centerToEntranceLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                centerToEntranceLeftFloorGameObj.transform.position = Vector3.zero;
                                                centerToEntranceLeftFloorGameObj.AddComponent<MeshFilter>();
                                                centerToEntranceLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                centerToEntranceLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToEntranceLeftFloorMesh;
                                                centerToEntranceLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                MeshGenerator.Floor centerToExitLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerExitLeft, null, tunnelWidth, false, this.railsWidth );
                                                Mesh centerToExitLeftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.centerExitLeft, MeshGenerator.ConvertListsToMatrix_2xM( centerToExitLeftFloor.centerL, centerToExitLeftFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject centerToExitLeftFloorGameObj = new GameObject( "Scambio Binario Centrale - Uscita Sinistra" );
                                                centerToExitLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                centerToExitLeftFloorGameObj.transform.position = Vector3.zero;
                                                centerToExitLeftFloorGameObj.AddComponent<MeshFilter>();
                                                centerToExitLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                centerToExitLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToExitLeftFloorMesh;
                                                centerToExitLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                            }

                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Right ) ) {
                                                MeshGenerator.Floor centerToEntranceRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerEntranceRight, null, tunnelWidth, false, this.railsWidth );
                                                Mesh centerToEntranceRightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.centerEntranceRight, MeshGenerator.ConvertListsToMatrix_2xM( centerToEntranceRightFloor.centerL, centerToEntranceRightFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject centerToEntranceRightFloorGameObj = new GameObject( "Scambio Binario Centrale - Ingresso Destro" );
                                                centerToEntranceRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                centerToEntranceRightFloorGameObj.transform.position = Vector3.zero;
                                                centerToEntranceRightFloorGameObj.AddComponent<MeshFilter>();
                                                centerToEntranceRightFloorGameObj.AddComponent<MeshRenderer>();
                                                centerToEntranceRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToEntranceRightFloorMesh;
                                                centerToEntranceRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                MeshGenerator.Floor centerToExitRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerExitRight, null, tunnelWidth, false, this.railsWidth );
                                                Mesh centerToExitRightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.centerExitRight, MeshGenerator.ConvertListsToMatrix_2xM( centerToExitRightFloor.centerL, centerToExitRightFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject centerToExitRightFloorGameObj = new GameObject( "Scambio Binario Centrale - Uscita Destra" );
                                                centerToExitRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                centerToExitRightFloorGameObj.transform.position = Vector3.zero;
                                                centerToExitRightFloorGameObj.AddComponent<MeshFilter>();
                                                centerToExitRightFloorGameObj.AddComponent<MeshRenderer>();
                                                centerToExitRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToExitRightFloorMesh;
                                                centerToExitRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                            }
                                            
                                            break;

            case SwitchType.BiToNewBi:      MeshGenerator.Floor centerFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerLine, null, centerWidth, false, this.railsWidth );
                                            Mesh centerFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.centerLine, MeshGenerator.ConvertListsToMatrix_2xM( centerFloor.centerL, centerFloor.centerR ), false, centerTextureTilting.x, centerTextureTilting.y );
                                            GameObject centerFloorGameObj = new GameObject( "Divisore centrale" );
                                            centerFloorGameObj.transform.parent = sectionGameObj.transform;
                                            centerFloorGameObj.transform.position = Vector3.zero;
                                            centerFloorGameObj.AddComponent<MeshFilter>();
                                            centerFloorGameObj.AddComponent<MeshRenderer>();
                                            centerFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
                                            centerFloorGameObj.GetComponent<MeshRenderer>().material = centerTexture;

                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Left ) ) {
                                                MeshGenerator.Floor leftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, null, tunnelWidth, false, this.railsWidth );
                                                Mesh leftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.leftLine, MeshGenerator.ConvertListsToMatrix_2xM( leftFloor.centerL, leftFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject leftFloorGameObj = new GameObject( "Scambio Binario Sinistro - Sinistro" );
                                                leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                leftFloorGameObj.transform.position = Vector3.zero;
                                                leftFloorGameObj.AddComponent<MeshFilter>();
                                                leftFloorGameObj.AddComponent<MeshRenderer>();
                                                leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                                leftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                MeshGenerator.Floor leftToEntranceLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftEntranceLeft, null, tunnelWidth, false, this.railsWidth );
                                                Mesh leftToEntranceLeftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.leftEntranceLeft, MeshGenerator.ConvertListsToMatrix_2xM( leftToEntranceLeftFloor.centerL, leftToEntranceLeftFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject leftToEntranceLeftFloorGameObj = new GameObject( "Scambio Binario Sinistro - Ingresso Sinistro" );
                                                leftToEntranceLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                leftToEntranceLeftFloorGameObj.transform.position = Vector3.zero;
                                                leftToEntranceLeftFloorGameObj.AddComponent<MeshFilter>();
                                                leftToEntranceLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                leftToEntranceLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftToEntranceLeftFloorMesh;
                                                leftToEntranceLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                MeshGenerator.Floor leftToExitLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftExitLeft, null, tunnelWidth, false, this.railsWidth );
                                                Mesh leftToExitLeftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.leftExitLeft, MeshGenerator.ConvertListsToMatrix_2xM( leftToExitLeftFloor.centerL, leftToExitLeftFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject leftToExitLeftFloorGameObj = new GameObject( "Scambio Binario Sinistro - Uscita Sinistra" );
                                                leftToExitLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                leftToExitLeftFloorGameObj.transform.position = Vector3.zero;
                                                leftToExitLeftFloorGameObj.AddComponent<MeshFilter>();
                                                leftToExitLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                leftToExitLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftToExitLeftFloorMesh;
                                                leftToExitLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                            }
                                            else {
                                                MeshGenerator.Floor leftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, null, tunnelWidth, false, this.railsWidth );
                                                Mesh leftFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.leftLine, MeshGenerator.ConvertListsToMatrix_2xM( leftFloor.centerL, leftFloor.centerR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                GameObject leftFloorGameObj = new GameObject( "Binario Sinistro" );
                                                leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                leftFloorGameObj.transform.position = Vector3.zero;
                                                leftFloorGameObj.AddComponent<MeshFilter>();
                                                leftFloorGameObj.AddComponent<MeshRenderer>();
                                                leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                                leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;
                                            }

                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Right ) ) {
                                                MeshGenerator.Floor rightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, null, tunnelWidth, false, this.railsWidth );
                                                Mesh rightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.rightLine, MeshGenerator.ConvertListsToMatrix_2xM( rightFloor.centerL, rightFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject rightFloorGameObj = new GameObject( "Scambio Binario Destro - Destro" );
                                                rightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                rightFloorGameObj.transform.position = Vector3.zero;
                                                rightFloorGameObj.AddComponent<MeshFilter>();
                                                rightFloorGameObj.AddComponent<MeshRenderer>();
                                                rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
                                                rightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                MeshGenerator.Floor rightToEntranceRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightEntranceRight, null, tunnelWidth, false, this.railsWidth );
                                                Mesh rightToEntranceRightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.rightEntranceRight, MeshGenerator.ConvertListsToMatrix_2xM( rightToEntranceRightFloor.centerL, rightToEntranceRightFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject rightToEntranceRightFloorGameObj = new GameObject( "Scambio Binario Destro - Ingresso Destro" );
                                                rightToEntranceRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                rightToEntranceRightFloorGameObj.transform.position = Vector3.zero;
                                                rightToEntranceRightFloorGameObj.AddComponent<MeshFilter>();
                                                rightToEntranceRightFloorGameObj.AddComponent<MeshRenderer>();
                                                rightToEntranceRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightToEntranceRightFloorMesh;
                                                rightToEntranceRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                MeshGenerator.Floor rightToExitRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightExitRight, null, tunnelWidth, false, this.railsWidth );
                                                Mesh rightToExitRightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.rightExitRight, MeshGenerator.ConvertListsToMatrix_2xM( rightToExitRightFloor.centerL, rightToExitRightFloor.centerR ), false, switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                GameObject rightToExitRightFloorGameObj = new GameObject( "Scambio Binario Destro - Uscita Destra" );
                                                rightToExitRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                rightToExitRightFloorGameObj.transform.position = Vector3.zero;
                                                rightToExitRightFloorGameObj.AddComponent<MeshFilter>();
                                                rightToExitRightFloorGameObj.AddComponent<MeshRenderer>();
                                                rightToExitRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightToExitRightFloorMesh;
                                                rightToExitRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                            }
                                            else {
                                                MeshGenerator.Floor rightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, null, tunnelWidth, false, this.railsWidth );
                                                Mesh rightFloorMesh = MeshGenerator.GeneratePlanarMesh( section.floorPoints.rightLine, MeshGenerator.ConvertListsToMatrix_2xM( rightFloor.centerL, rightFloor.centerR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                GameObject rightFloorGameObj = new GameObject( "Binario Destro" );
                                                rightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                rightFloorGameObj.transform.position = Vector3.zero;
                                                rightFloorGameObj.AddComponent<MeshFilter>();
                                                rightFloorGameObj.AddComponent<MeshRenderer>();
                                                rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
                                                rightFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;
                                            }
                                            
                                            break;

            
        }
        
        GameObject indicatorObj = GameObject.CreatePrimitive( PrimitiveType.Cube );
        indicatorObj.name = "Indicatore Scambio";
        indicatorObj.transform.position = section.centerCoords;
        indicatorObj.transform.parent = sectionGameObj.transform;
        indicatorObj.AddComponent<DynamicIcons>();

        DynamicIcons indicatorScript = indicatorObj.GetComponent<DynamicIcons>();
        indicatorScript.ICON_Texture2D = Resources.Load( "Indicatori/Texture2D/Wrong_Way" ) as Texture2D;
        indicatorScript.ICON_Color = Color.red;
        indicatorScript.ICON_Fade = true;
        indicatorScript.ICON_Rotation = false;
        indicatorScript.TARGET_MaxVisibleDistance = 2000.0f;
        indicatorScript.TARGET_MinVisibleDistanceOnScreen = 500.0f;
        indicatorScript.ICON_Scale = false;
        indicatorScript.ICON_Visibility = Icon.Show.Always;
        indicatorScript.ICON_MaxSize = 0.8f;
        indicatorScript.ICON_Active = true;
        indicatorScript.ICON_RotationCorrection = 270.0f;
        indicatorScript.ICON_DistanceFromBorderInPixels = Mathf.Sqrt( Mathf.Pow( Screen.width, 2 ) + Mathf.Pow( Screen.height, 2 ) ) * 0.025f;

        section.indicatorObj = indicatorObj;
    }

    private void GenerateTunnelSidePlatformMesh( LineSection section, GameObject sectionGameObj ) {
        MeshGenerator.PlatformSide platformSidesVertexPoints = new MeshGenerator.PlatformSide();

        Mesh platformSideLeftMesh = new Mesh();
        Mesh platformSideRightMesh = new Mesh();
        MeshGenerator.ExtrudedMesh platformFloorLeftMesh = new MeshGenerator.ExtrudedMesh();
        MeshGenerator.ExtrudedMesh platformFloorRightMesh = new MeshGenerator.ExtrudedMesh();

        float sectionWidth = section.bidirectional ? ( ( tunnelWidth * 2 ) + centerWidth ) : tunnelWidth;
        Vector3 startingDir = section.controlsPoints[ 1 ] - section.bezierCurveLimitedAngle[ 0 ];
        platformSidesVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( section.bezierCurveLimitedAngle, startingDir, sectionWidth, tunnelParabolic, platformHeight, platformWidth );

        platformSideLeftMesh = MeshGenerator.GeneratePlanarMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( platformSidesVertexPoints.leftFloorRight, platformSidesVertexPoints.leftDown ), false, this.platformSideTextureTilting.x, this.platformSideTextureTilting.y );
        platformSideRightMesh = MeshGenerator.GeneratePlanarMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( platformSidesVertexPoints.rightDown, platformSidesVertexPoints.rightUp ), false, this.platformSideTextureTilting.x, this.platformSideTextureTilting.y );

        MeshGenerator.SpecularBaseLine platformFloorBaseLines = new MeshGenerator.SpecularBaseLine(); 

        float distanceFromCenter = section.bidirectional ? ( this.tunnelWidth + ( this.centerWidth / 2 ) ) : ( this.tunnelWidth / 2 );

        platformFloorLeftMesh = MeshGenerator.GenerateExtrudedMesh( this.platformFloorShape, this.platformFloorShapeScale, section.previousSection != null ? section.previousSection.sidePlatformFloorLeftLastProfile : null, platformSidesVertexPoints.leftFloorRight, this.platformFloorHorPosCorrection, true, this.platformFloorTextureTilting.x, this.platformFloorTextureTilting.y, 0.0f, this.platformFloorSmoothFactor );
        platformFloorRightMesh = MeshGenerator.GenerateExtrudedMesh( this.platformFloorShape, this.platformFloorShapeScale, section.previousSection != null ? section.previousSection.sidePlatformFloorRightLastProfile : null, platformSidesVertexPoints.rightFloorLeft, this.platformFloorHorPosCorrection, false, this.platformFloorTextureTilting.x, this.platformFloorTextureTilting.y, 180.0f, this.platformFloorSmoothFactor );

        GameObject platformSideLeftGameObj = new GameObject( "Piattaforma sinistra (lato)" );
        platformSideLeftGameObj.transform.parent = sectionGameObj.transform;
        platformSideLeftGameObj.transform.position = Vector3.zero;
        platformSideLeftGameObj.AddComponent<MeshFilter>();
        platformSideLeftGameObj.AddComponent<MeshRenderer>();
        platformSideLeftGameObj.GetComponent<MeshFilter>().sharedMesh = platformSideLeftMesh;
        platformSideLeftGameObj.GetComponent<MeshRenderer>().material = platformSideTexture;

        GameObject platformSideRightGameObj = new GameObject( "Piattaforma destra (lato)" );
        platformSideRightGameObj.transform.parent = sectionGameObj.transform;
        platformSideRightGameObj.transform.position = Vector3.zero;
        platformSideRightGameObj.AddComponent<MeshFilter>();
        platformSideRightGameObj.AddComponent<MeshRenderer>();
        platformSideRightGameObj.GetComponent<MeshFilter>().sharedMesh = platformSideRightMesh;
        platformSideRightGameObj.GetComponent<MeshRenderer>().material = platformSideTexture;

        GameObject platformFloorLeftGameObj = new GameObject( "Piattaforma sinistra (pavimento)" );
        platformFloorLeftGameObj.transform.parent = sectionGameObj.transform;
        platformFloorLeftGameObj.transform.position = Vector3.zero;
        platformFloorLeftGameObj.AddComponent<MeshFilter>();
        platformFloorLeftGameObj.AddComponent<MeshRenderer>();
        platformFloorLeftGameObj.GetComponent<MeshFilter>().sharedMesh = platformFloorLeftMesh.mesh;
        platformFloorLeftGameObj.GetComponent<MeshRenderer>().material = platformFloorTexture;

        GameObject platformFloorRightGameObj = new GameObject( "Piattaforma destra (pavimento)" );
        platformFloorRightGameObj.transform.parent = sectionGameObj.transform;
        platformFloorRightGameObj.transform.position = Vector3.zero;
        platformFloorRightGameObj.AddComponent<MeshFilter>();
        platformFloorRightGameObj.AddComponent<MeshRenderer>();
        platformFloorRightGameObj.GetComponent<MeshFilter>().sharedMesh = platformFloorRightMesh.mesh;
        platformFloorRightGameObj.GetComponent<MeshRenderer>().material = platformFloorTexture;

        section.sidePlatformFloorLeftLastProfile = platformFloorLeftMesh.lastProfileVertex;
        section.sidePlatformFloorRightLastProfile = platformFloorRightMesh.lastProfileVertex;

        // Update dettagli LineSection 
        section.platformSidesPoints = platformSidesVertexPoints;
    }

    private void GenerateTunnelWallMesh( LineSection section, GameObject sectionGameObj ) {
        MeshGenerator.SpecularBaseLine wallBaseLines = new MeshGenerator.SpecularBaseLine();

        MeshGenerator.ExtrudedMesh wallLeftMesh = new MeshGenerator.ExtrudedMesh();
        MeshGenerator.ExtrudedMesh wallRightMesh = new MeshGenerator.ExtrudedMesh();

        float baseWidth = section.bidirectional ? ( this.tunnelWidth + ( this.centerWidth / 2 ) + this.platformWidth ) : ( ( this.tunnelWidth / 2 ) + this.platformWidth );

        float angleFromCenter = Mathf.Atan( this.platformHeight / baseWidth ) * Mathf.Rad2Deg;
        float distanceFromCenter = Mathf.Sqrt( Mathf.Pow( this.platformHeight, 2 ) + Mathf.Pow( baseWidth, 2 ) );

        Vector3? lastDir = section.previousSection == null ? null : ( Vector3? )( section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 1 ] - section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 2 ] );
        wallBaseLines = MeshGenerator.CalculateBaseLinesFromCurve( section.bezierCurveLimitedAngle, lastDir, distanceFromCenter, angleFromCenter );

        wallLeftMesh = MeshGenerator.GenerateExtrudedMesh( this.tunnelWallShape, this.tunnelWallShapeScale, section.previousSection != null ? section.previousSection.wallLeftLastProfile : null, wallBaseLines.left, 0.0f, true, this.tunnelWallTextureTilting.x, this.tunnelWallTextureTilting.y, 0.0f, this.tunnelWallSmoothFactor );
        wallRightMesh = MeshGenerator.GenerateExtrudedMesh( this.tunnelWallShape, this.tunnelWallShapeScale, section.previousSection != null ? section.previousSection.wallRightLastProfile : null, wallBaseLines.right, 0.0f, false, this.tunnelWallTextureTilting.x, this.tunnelWallTextureTilting.y, 180.0f, this.tunnelWallSmoothFactor );

        GameObject wallLeftGameObj = new GameObject( "Muro sinistro" );
        wallLeftGameObj.transform.parent = sectionGameObj.transform;
        wallLeftGameObj.transform.position = Vector3.zero;
        wallLeftGameObj.AddComponent<MeshFilter>();
        wallLeftGameObj.AddComponent<MeshRenderer>();
        wallLeftGameObj.GetComponent<MeshFilter>().sharedMesh = wallLeftMesh.mesh;
        wallLeftGameObj.GetComponent<MeshRenderer>().material = this.tunnelWallTexture;

        GameObject wallRightGameObj = new GameObject( "Muro destro" );
        wallRightGameObj.transform.parent = sectionGameObj.transform;
        wallRightGameObj.transform.position = Vector3.zero;
        wallRightGameObj.AddComponent<MeshFilter>();
        wallRightGameObj.AddComponent<MeshRenderer>();
        wallRightGameObj.GetComponent<MeshFilter>().sharedMesh = wallRightMesh.mesh;
        wallRightGameObj.GetComponent<MeshRenderer>().material = this.tunnelWallTexture;

        section.wallLeftLastProfile = wallLeftMesh.lastProfileVertex;
        section.wallRightLastProfile = wallRightMesh.lastProfileVertex;

        // Update dettagli LineSection 
        //section.platformSidesPoints = platformSidesVertexPoints;
    }

    private void AddProps() {
        foreach( string lineName in this.lines.Keys ) {
            
            GameObject lineGameObj = new GameObject( lineName );
            Vector2 banisterOffsets = Vector2.zero;
            float pillarOffset = 0.0f;
            float fanOffset = 0.0f;
            for( int i = 0; i < this.lines[ lineName ].Count; i++ ) { 
                
                LineSection section = this.lines[ lineName ][ i ];
                
                banisterOffsets = AddSidePlatformBanisters( section, banisterOffsets, this.banisterMinDistance, this.banisterMaxDistance, this.banisterRotationCorrection, this.banisterPositionCorrectionLeft, this.banisterPositionCorrectionRight );
                pillarOffset = AddPillars( section, pillarOffset, this.pillarMinDistance, this.pillarMaxDistance, this.pillarRotationCorrection, this.pillarPositionCorrection );
                fanOffset = AddCeilingFans( section, fanOffset, this.fanMinDistance, this.fanMaxDistance, ( this.centerWidth + this.tunnelWidth ) / 2 , this.fanRotationCorrection, this.fanPositionCorrection );
            }
        }
    }

    private float AddPillars( LineSection section, float previousOffset, float minDistance, float maxDistance, Vector3 rotationCorrection, Vector3 positionCorrection ) {

        float distance = Random.Range( minDistance, maxDistance );

        float offset = previousOffset;

        if( section.type == Type.Tunnel && section.bidirectional ) {

            GameObject pillarsParent = new GameObject( "Pilatri centrali" );
            pillarsParent.transform.parent = section.sectionObj.transform;

            int c = 0;

            for( int i = 1; i < section.bezierCurveLimitedAngle.Count; i++ ) {
                Vector3 m0 = section.bezierCurveLimitedAngle[ i - 1 ];
                Vector3 m1 =  section.bezierCurveLimitedAngle[ i ];

                float lenght = ( m1 - m0 ).magnitude;
                Vector3 dir = ( m1 - m0 ).normalized;

                Vector3 pp = m0 + ( dir * offset );

                if( offset < lenght ) {
                    if( distance < ( lenght - offset ) ) { 
                        
                        float remaingDistance = ( m1 - pp ).magnitude;

                        while( remaingDistance > distance ) {

                            GameObject pillar = GameObject.Instantiate( this.pillar, pp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                            pillar.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            pillar.transform.Translate( positionCorrection, Space.Self );
                            pillar.isStatic = true;
                            pillar.name = "Pilastro " + c; 
                            pillar.transform.parent = pillarsParent.transform;

                            pp += ( dir * distance );
                            remaingDistance = ( m1 - pp ).magnitude;
                            c++;
                        }

                        GameObject lastPillar = GameObject.Instantiate( this.pillar, pp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                        lastPillar.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        lastPillar.transform.Translate( positionCorrection, Space.Self );
                        lastPillar.isStatic = true;
                        lastPillar.name = "Pilastro" + c;
                        lastPillar.transform.parent = pillarsParent.transform;

                        offset = distance - remaingDistance;
                    }
                    else {

                        GameObject pillar = GameObject.Instantiate( this.pillar, pp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                        pillar.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        pillar.transform.Translate( positionCorrection, Space.Self );
                        pillar.isStatic = true;
                        pillar.name = "Pilastro " + c; 
                        pillar.transform.parent = pillarsParent.transform;

                        offset += distance - lenght;
                    }
                }
                else {

                    offset -= lenght;
                }
            }
        }
        else {
            return 0.0f;
        }

        return offset;
    }

    private Vector2 AddSidePlatformBanisters( LineSection section, Vector2 previousOffsets, float minDistance, float maxDistance, Vector3 rotationCorrection, Vector3 positionCorrectionLeft, Vector3 positionCorrectionRight ) {

        float distance = Random.Range( minDistance, maxDistance );

        float meshOffsetLeft = previousOffsets[ 0 ];
        float meshOffsetRight = previousOffsets[ 1 ];

        if( section.type == Type.Tunnel ) {

            GameObject banistersLeftParent = new GameObject( "Ringhiere (piattaforma sinistra)" );
            banistersLeftParent.transform.parent = section.sectionObj.transform;

            GameObject banistersRightParent = new GameObject( "Ringhiere (piattaforma destra)" );
            banistersRightParent.transform.parent = section.sectionObj.transform;
            int cLeft = 0, cRight = 0;

            for( int i = 1; i < section.platformSidesPoints.leftFloorRight.Count; i++ ) {
                Vector3 p0Left = section.platformSidesPoints.leftFloorRight[ i - 1 ];
                Vector3 p1Left =  section.platformSidesPoints.leftFloorRight[ i ];

                Vector3 p0Right = section.platformSidesPoints.rightFloorLeft[ i - 1 ];
                Vector3 p1Right =  section.platformSidesPoints.rightFloorLeft[ i ];

                float meshLenghtLeft = ( p1Left - p0Left ).magnitude;
                float meshLenghtRight = ( p1Right - p0Right ).magnitude;
                
                Vector3 meshDirLeft = ( p1Left - p0Left ).normalized;
                Vector3 meshDirRight = ( p1Right - p0Right ).normalized;

                Vector3 bpLeft = p0Left + ( meshDirLeft * meshOffsetLeft );
                Vector3 bpRight = p0Right + ( meshDirRight * meshOffsetRight );

                // Lato sinistro
                if( meshOffsetLeft < meshLenghtLeft ) {
                    if( distance < ( meshLenghtLeft - meshOffsetLeft ) ) { 
                        
                        // Caso 1: La distanza fra gli elementi è minore della distanza fra due punti (in lunghezza) della mesh della piattaforma
                        float remaingDistanceLeft = ( p1Left - bpLeft ).magnitude;

                        while( remaingDistanceLeft > distance ) {

                            GameObject banisterLeft = GameObject.Instantiate( banister, bpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirLeft, Vector3.forward ) ) );
                            banisterLeft.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            banisterLeft.transform.Translate( positionCorrectionLeft, Space.Self );
                            banisterLeft.isStatic = true;
                            banisterLeft.name = "Ringhiera " + cLeft; 
                            banisterLeft.transform.parent = banistersLeftParent.transform;

                            bpLeft += ( meshDirLeft * distance );
                            remaingDistanceLeft = ( p1Left - bpLeft ).magnitude;
                            cLeft++;
                        }

                        GameObject lastBanisterLeft = GameObject.Instantiate( banister, bpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirLeft, Vector3.forward ) ) );
                        lastBanisterLeft.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        lastBanisterLeft.transform.Translate( positionCorrectionLeft, Space.Self );
                        lastBanisterLeft.isStatic = true;
                        lastBanisterLeft.name = "Ringhiera" + cLeft;
                        lastBanisterLeft.transform.parent = banistersLeftParent.transform;

                        meshOffsetLeft = distance - remaingDistanceLeft;
                    }
                    else {

                        // Caso 2: La distanza fra gli elementi è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma. 
                        // L'offset precedente è inferiore alla distanza fra due punti (in lunghezza) della mesh della piattaforma. Quindi genero il GameObject all'offset rimanente sommo la 
                        // differenza fra la distanza fra gli elementi e la distanza fra due punti (in lunghezza) della mesh della piattaforma.
                        GameObject banisterLeft = GameObject.Instantiate( banister, bpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirLeft, Vector3.forward ) ) );
                        banisterLeft.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        banisterLeft.transform.Translate( positionCorrectionLeft, Space.Self );
                        banisterLeft.isStatic = true;
                        banisterLeft.name = "Ringhiera " + cLeft; 
                        banisterLeft.transform.parent = banistersLeftParent.transform;

                        meshOffsetLeft += distance - meshLenghtLeft;
                    }
                }
                else {

                    // Caso 3: La distanza fra gli elementi è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma. 
                    // L'offset precedente è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma.
                    // Non genero nulla e sottraggo all'offset la distanza fra due punti (in lunghezza) della mesh della piattaforma.
                    meshOffsetLeft -= meshLenghtLeft;
                }

                // Lato destro
                if( meshOffsetRight < meshLenghtRight ) {

                    if( distance < ( meshLenghtRight - meshOffsetRight ) ) {

                        // Caso 1: La distanza fra gli elementi è minore della distanza fra due punti (in lunghezza) della mesh della piattaforma
                        float remaingDistanceRight = ( p1Right - bpRight ).magnitude;

                        while( remaingDistanceRight > distance ) {

                            GameObject banisterRight = GameObject.Instantiate( banister, bpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirRight, Vector3.forward ) ) );
                            banisterRight.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            banisterRight.transform.Translate( positionCorrectionRight, Space.Self );
                            banisterRight.isStatic = true;
                            banisterRight.name = "Ringhiera " + cRight; 
                            banisterRight.transform.parent = banistersRightParent.transform;

                            bpRight += ( meshDirRight * distance );
                            remaingDistanceRight = ( p1Right - bpRight ).magnitude;
                            cRight++;
                        }

                        GameObject lastBanisterRight = GameObject.Instantiate( banister, bpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirRight, Vector3.forward ) ) );
                        lastBanisterRight.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        lastBanisterRight.transform.Translate( positionCorrectionRight, Space.Self );
                        lastBanisterRight.isStatic = true;
                        lastBanisterRight.name = "Ringhiera" + cRight;
                        lastBanisterRight.transform.parent = banistersRightParent.transform;

                        meshOffsetRight = distance - remaingDistanceRight;
                    }
                    else{

                        // Caso 2: La distanza fra gli elementi è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma. 
                        // L'offset precedente è inferiore alla distanza fra due punti (in lunghezza) della mesh della piattaforma. Quindi genero il GameObject all'offset rimanente sommo la 
                        // differenza fra la distanza fra gli elementi e la distanza fra due punti (in lunghezza) della mesh della piattaforma.
                        GameObject banisterRight = GameObject.Instantiate( banister, bpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirRight, Vector3.forward ) ) );
                        banisterRight.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        banisterRight.transform.Translate( positionCorrectionRight, Space.Self );
                        banisterRight.isStatic = true;
                        banisterRight.name = "Ringhiera " + cRight; 
                        banisterRight.transform.parent = banistersRightParent.transform;

                        meshOffsetRight += distance - meshLenghtRight;
                    }
                }
                else {

                    // Caso 3: La distanza fra gli elementi è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma. 
                    // L'offset precedente è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma.
                    // Non genero nulla e sottraggo all'offset la distanza fra due punti (in lunghezza) della mesh della piattaforma.
                    meshOffsetRight -= meshLenghtRight;
                }
            }
        }
        else {
            return Vector2.zero;
        }

        return new Vector2( meshOffsetLeft, meshOffsetRight );
    }

    private float AddCeilingFans( LineSection section, float previousOffset, float minDistance, float maxDistance, float distanceFromCenter, Vector3 rotationCorrection, Vector3 positionCorrection ) {

        float distance = Random.Range( minDistance, maxDistance );

        float offset = previousOffset;

        if( section.type == Type.Tunnel ) {

            GameObject fansParent = new GameObject( "Ventole" );
            fansParent.transform.parent = section.sectionObj.transform;

            int c = 0;

            for( int i = 1; i < section.bezierCurveLimitedAngle.Count; i++ ) {
                Vector3 m0 = section.bezierCurveLimitedAngle[ i - 1 ];
                Vector3 m1 =  section.bezierCurveLimitedAngle[ i ];

                float lenght = ( m1 - m0 ).magnitude;
                Vector3 dir = ( m1 - m0 ).normalized;

                Vector3 fp = m0 + ( dir * offset );
                fp = new Vector3( fp.x, fp.y, fp.z - tunnelWallHeight );

                int position = 2;

                if( offset < lenght ) {
                    if( distance < ( lenght - offset ) ) { 
                        
                        float remaingDistance = ( m1 - fp ).magnitude;

                        while( remaingDistance > distance ) {

                            if( section.bidirectional ) {
                                
                                position = Random.Range( 0, 3 ); // 0 = Only Left, 1 = Only Right, 2 = Both Sides
                                if( position == 0 || position == 2 ) {
                                    Vector3 fpLeft = fp + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir ) * distanceFromCenter;

                                    GameObject fan = GameObject.Instantiate( this.fan, fpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                    fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                    fan.transform.Translate( positionCorrection, Space.Self );
                                    fan.isStatic = true;
                                    fan.name = "Ventola (sinistra) " + c; 
                                    fan.transform.parent = fansParent.transform;

                                    //Debug.DrawLine( m0, fpLeft, Color.red, 999 );
                                }
                                if( position == 1 || position == 2 ) {
                                    Vector3 fpRight = fp + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * dir ) * distanceFromCenter;

                                    GameObject fan = GameObject.Instantiate( this.fan, fpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                    fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                    fan.transform.Translate( positionCorrection, Space.Self );
                                    fan.isStatic = true;
                                    fan.name = "Ventola (destra) " + c; 
                                    fan.transform.parent = fansParent.transform;

                                    Debug.DrawLine( m0, fpRight, Color.green, 999 );
                                }
                            }
                            else {
                                GameObject fan = GameObject.Instantiate( this.fan, fp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                fan.transform.Translate( positionCorrection, Space.Self );
                                fan.isStatic = true;
                                fan.name = "Ventola (centrale) " + c; 
                                fan.transform.parent = fansParent.transform;
                            }

                            fp += ( dir * distance );
                            remaingDistance = ( m1 - fp ).magnitude;
                            c++;
                        }

                        if( section.bidirectional ) {
                            position = Random.Range( 0, 3 ); // 0 = Only Left, 1 = Only Right, 2 = Both Sides
                            if( position == 0 || position == 2 ) {
                                Vector3 fpLeft = fp + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir ) * distanceFromCenter;

                                GameObject lastFan = GameObject.Instantiate( this.fan, fpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                lastFan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                lastFan.transform.Translate( positionCorrection, Space.Self );
                                lastFan.isStatic = true;
                                lastFan.name = "Ventola (sinistra) " + c; 
                                lastFan.transform.parent = fansParent.transform;
                            }
                            if( position == 1 || position == 2 ) {
                                Vector3 fpRight = fp + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * dir ) * distanceFromCenter;

                                GameObject lastFan = GameObject.Instantiate( this.fan, fpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                lastFan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                lastFan.transform.Translate( positionCorrection, Space.Self );
                                lastFan.isStatic = true;
                                lastFan.name = "Ventola (destra) " + c; 
                                lastFan.transform.parent = fansParent.transform;
                            }
                        }
                        else {
                            GameObject fan = GameObject.Instantiate( this.fan, fp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                            fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            fan.transform.Translate( positionCorrection, Space.Self );
                            fan.isStatic = true;
                            fan.name = "Ventola (centrale) " + c; 
                            fan.transform.parent = fansParent.transform;
                        }
                    }
                    else {
                        
                        if( section.bidirectional ) {
                            position = Random.Range( 0, 3 ); // 0 = Only Left, 1 = Only Right, 2 = Both Sides
                            if( position == 0 || position == 2 ) {
                                Vector3 fpLeft = fp + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir ) * distanceFromCenter;

                                GameObject lastFan = GameObject.Instantiate( this.fan, fpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                lastFan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                lastFan.transform.Translate( positionCorrection, Space.Self );
                                lastFan.isStatic = true;
                                lastFan.name = "Ventola (sinistra) " + c; 
                                lastFan.transform.parent = fansParent.transform;
                            }
                            if( position == 1 || position == 2 ) {
                                Vector3 fpRight = fp + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * dir ) * distanceFromCenter;

                                GameObject fan = GameObject.Instantiate( this.fan, fpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                fan.transform.Translate( positionCorrection, Space.Self );
                                fan.isStatic = true;
                                fan.name = "Ventola (destra) " + c; 
                                fan.transform.parent = fansParent.transform;
                            }
                        }
                        else {
                            GameObject fan = GameObject.Instantiate( this.fan, fp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                            fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            fan.transform.Translate( positionCorrection, Space.Self );
                            fan.isStatic = true;
                            fan.name = "Ventola (centrale) " + c; 
                            fan.transform.parent = fansParent.transform;
                        }

                        offset += distance - lenght;
                    }
                }
                else {

                    offset -= lenght;
                }
            }
        }
        else {
            return 0.0f;
        }

        return offset;
    }


    private void GenerateMeshes() {

        foreach( string lineName in this.lines.Keys ) {
            
            GameObject lineGameObj = new GameObject( lineName );

            //foreach( LineSection section in this.lines[ lineName ] ) {
            for( int i = 0; i < this.lines[ lineName ].Count; i++ ) { 
                
                LineSection section = this.lines[ lineName ][ i ];

                if( i == 0 ) {

                    if( section.fromSection != null && section.fromSection.switchType == SwitchType.MonoToNewMono ) {
                        section.bidirectional = false;
                    }
                    else if( section.fromSection != null && section.fromSection.switchType == SwitchType.BiToNewBi ) {
                        section.bidirectional = true;
                    }
                    else {
                        section.bidirectional = startingBidirectional;
                    }
                }
                else {
                    section.bidirectional = this.lines[ lineName ][ i - 1 ].bidirectional;
                }
                
                string sectionName = "Sezione " + i;
                section.sectionName = sectionName;

                GameObject sectionGameObj = new GameObject( sectionName );
                sectionGameObj.transform.parent = lineGameObj.transform;
                sectionGameObj.transform.position = section.bezierCurveLimitedAngle[ 0 ];

                section.sectionObj = sectionGameObj;

                switch( section.type ) {
                    case Type.Tunnel:   GenerateTunnelFloorMesh( section, sectionGameObj );
                                        GenerateTunnelSidePlatformMesh( section, sectionGameObj );
                                        GenerateTunnelWallMesh( section, sectionGameObj );

                                        break;

                    case Type.Station:  List<Vector3> stationsPoints = section.bezierCurveLimitedAngle;

                                        MeshGenerator.Floor stationRails = new MeshGenerator.Floor();

                                        if( section.bidirectional ) {

                                            if( section.stationType == StationType.BothSidesPlatform ) {
                                            
                                                stationRails = MeshGenerator.CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( section, centerWidth, tunnelWidth, railsWidth );

                                                Mesh leftFloorMesh = new Mesh();
                                                Mesh centerFloorMesh = new Mesh();
                                                Mesh rightFloorMesh = new Mesh();

                                                leftFloorMesh = MeshGenerator.GeneratePlanarMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.leftL, stationRails.leftR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                GameObject leftFloorGameObj = new GameObject( "Binari sinistra" );
                                                leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                leftFloorGameObj.transform.position = Vector3.zero;
                                                leftFloorGameObj.AddComponent<MeshFilter>();
                                                leftFloorGameObj.AddComponent<MeshRenderer>();
                                                leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                                leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

                                                centerFloorMesh = MeshGenerator.GeneratePlanarMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.centerL, stationRails.centerR ), false, centerTextureTilting.x, centerTextureTilting.y );
                                                GameObject centerFloorGameObj = new GameObject( "Divisore centrale" );
                                                centerFloorGameObj.transform.parent = sectionGameObj.transform;
                                                centerFloorGameObj.transform.position = Vector3.zero;
                                                centerFloorGameObj.AddComponent<MeshFilter>();
                                                centerFloorGameObj.AddComponent<MeshRenderer>();
                                                centerFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
                                                centerFloorGameObj.GetComponent<MeshRenderer>().material = centerTexture;

                                                /*for( int p = 0; p < stationRails.centerLine.Count; p += 2 ) {
                                                    GameObject pillarGameObj = Instantiate( pillar, stationRails.centerLine[ p ] + new Vector3( 0.0f, 0.0f, -pillar.transform.localScale.y / 2 ), Quaternion.Euler( 0.0f, -90.0f, 90.0f ) );
                                                    pillarGameObj.transform.parent = sectionGameObj.transform;
                                                    pillarGameObj.name = "Pilastro";
                                                }*/

                                                rightFloorMesh = MeshGenerator.GeneratePlanarMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.rightL, stationRails.rightR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                GameObject rightFloorGameObj = new GameObject( "Binari destra" );
                                                rightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                rightFloorGameObj.transform.position = Vector3.zero;
                                                rightFloorGameObj.AddComponent<MeshFilter>();
                                                rightFloorGameObj.AddComponent<MeshRenderer>();
                                                rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
                                                rightFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

                                                section.controlsPoints = stationsPoints;
                                                section.floorPoints = stationRails;
                                                section.curvePointsCount = stationRails.centerLine.Count;
                                            }
                                            else if( section.stationType == StationType.CentralPlatform ) {
                                                stationRails = MeshGenerator.CalculateBidirectionalWithCentralPlatformFloorMeshVertex( section, centerWidth, tunnelWidth, stationLenght, stationExtensionLenght, stationExtensionHeight, stationExtensionCurvePoints );
                                                Mesh leftFloorMesh = new Mesh();
                                                Mesh centerFloorMesh = new Mesh();
                                                Mesh rightFloorMesh = new Mesh();

                                                leftFloorMesh = MeshGenerator.GeneratePlanarMesh( stationRails.leftLine, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.leftL, stationRails.leftR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                GameObject leftFloorGameObj = new GameObject( "Binari sinistra" );
                                                leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                leftFloorGameObj.transform.position = Vector3.zero;
                                                leftFloorGameObj.AddComponent<MeshFilter>();
                                                leftFloorGameObj.AddComponent<MeshRenderer>();
                                                leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                                leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

                                                rightFloorMesh = MeshGenerator.GeneratePlanarMesh( stationRails.rightLine, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.rightL, stationRails.rightR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                GameObject rightFloorGameObj = new GameObject( "Binari destra" );
                                                rightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                rightFloorGameObj.transform.position = Vector3.zero;
                                                rightFloorGameObj.AddComponent<MeshFilter>();
                                                rightFloorGameObj.AddComponent<MeshRenderer>();
                                                rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
                                                rightFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

                                                section.controlsPoints = stationsPoints;
                                                section.floorPoints = stationRails;
                                                section.curvePointsCount = stationRails.leftLine.Count;
                                            }
                                        }
                                        else {
                                            stationRails = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( stationsPoints, null, tunnelWidth, tunnelParabolic, this.railsWidth );

                                            Mesh centerFloorMesh = new Mesh();

                                            centerFloorMesh = MeshGenerator.GeneratePlanarMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.centerL, stationRails.centerR ), false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                            GameObject leftFloorGameObj = new GameObject( "Binari centrali" );
                                            leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                            leftFloorGameObj.transform.position = Vector3.zero;
                                            leftFloorGameObj.AddComponent<MeshFilter>();
                                            leftFloorGameObj.AddComponent<MeshRenderer>();
                                            leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
                                            leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

                                            section.controlsPoints = stationsPoints;
                                            section.floorPoints = stationRails;
                                            section.curvePointsCount = stationRails.centerLine.Count;
                                        }
                    
                                        break;

                    case Type.Switch:   GenerateSwitchMeshes( lineName, i, section, sectionGameObj );
                                        break;
                }
            }
        }
    }

    private Vector3 GenerateLine( string lineName, Direction previousLineOrientation, Direction lineOrientation, int lineLength, Vector3 startingPoint, Vector3 startingDir, bool generateNewLines, LineSection fromSection ) {
        
        LineSection previousSection = null;

        lineTurnDistance = Random.Range( lineTurnDistanceMin, lineTurnDistanceMax );
        stationsDistance = Random.Range( stationsDistanceMin, stationsDistanceMax );
        switchDistance = stationsDistance;
        while( switchDistance == stationsDistance ) {
            switchDistance = Random.Range( switchDistanceMin, switchDistanceMax );
        }

        List<Direction> sectionAllAvailableOrientations = new List<Direction>();

        if( lineOrientation == Direction.Random ) {
            lineOrientation = ( Direction )Random.Range( 0, 4 );
        }
        switch( lineOrientation ) {
            case Direction.East:    sectionAllAvailableOrientations.Add( Direction.North );
                                    sectionAllAvailableOrientations.Add( Direction.East );
                                    sectionAllAvailableOrientations.Add( Direction.South );
                                    break;

            case Direction.West:    sectionAllAvailableOrientations.Add( Direction.North );
                                    sectionAllAvailableOrientations.Add( Direction.West );
                                    sectionAllAvailableOrientations.Add( Direction.South );
                                    break;

            case Direction.North:   sectionAllAvailableOrientations.Add( Direction.East );
                                    sectionAllAvailableOrientations.Add( Direction.North );
                                    sectionAllAvailableOrientations.Add( Direction.West );
                                    break;

            case Direction.South:   sectionAllAvailableOrientations.Add( Direction.East );
                                    sectionAllAvailableOrientations.Add( Direction.South );
                                    sectionAllAvailableOrientations.Add( Direction.West );   
                                    break;
        }
        Debug.Log( "MAIN DIR: " + lineOrientation );

        Direction sectionOrientation = lineOrientation;

        if( startingPoint == Vector3.zero ) {
            startingPoint = gameObject.transform.position;
        }

        // Genero la lista delle sezioni della linea
        List<LineSection> sections = new List<LineSection>();
        List<Vector3> bordersUp = new List<Vector3>();
        List<Vector3> bordersDown = new List<Vector3>();

        bool removeLine = false;
        int sectionsBeforeTurnCounter = 0;

        for( int i = 0; i < lineLength; i++ ) {
            
            if( i % lineTurnDistance == 0 && sections.Count != 0 ) {

                List<LineSection> actualSections = new List<LineSection>();
                //Debug.Log( ">>>>>>>>>>> Proibited Areas: " + lineName );
                //Debug.Log( ">>>>>>>>>>> Line Turn Distance: " + lineTurnDistance );
                for( int k = i - sectionsBeforeTurnCounter; k < i; k++ ) {
                    actualSections.Add( sections[ k ] );

                    //Debug.Log( ">>>>>>>>>>> indice sezioni: " + k );
                }
                //Debug.Log( "actualSections[0]: " + actualSections[ 0 ] );
                if( this.proibitedAreas.ContainsKey( lineName ) ){ 
                    this.proibitedAreas[ lineName ].Add( CalculateProibitedRectangularArea( actualSections, sectionOrientation ) );
                }
                else {
                    this.proibitedAreas.Add( lineName, new List<List<Vector3>>{ CalculateProibitedRectangularArea( actualSections, sectionOrientation ) } );
                }

                List<Direction> sectionActualAvailableOrientations = sectionAllAvailableOrientations;
                if( sectionOrientation != Direction.Random ) {
                    
                    switch( sectionOrientation ) {
                        case Direction.East:    sectionActualAvailableOrientations.Remove( Direction.West );
                                                break;

                        case Direction.West:    sectionActualAvailableOrientations.Remove( Direction.East );
                                                break;

                        case Direction.North:   sectionActualAvailableOrientations.Remove( Direction.South );
                                                break;

                        case Direction.South:   sectionActualAvailableOrientations.Remove( Direction.North );   
                                                break;
                    }
                }
                sectionOrientation = sectionActualAvailableOrientations[ Random.Range( 0, sectionActualAvailableOrientations.Count ) ];

                sectionsBeforeTurnCounter = 0;
                int nextTurnAfter = Random.Range( lineTurnDistanceMin, lineTurnDistanceMax );

                if( lineTurnDistance + nextTurnAfter < lineLength ) {
                    lineTurnDistance += nextTurnAfter;
                }
            }

            sectionsBeforeTurnCounter++;

            if( startingDir == Vector3.zero ) {
                switch( sectionOrientation ) {
                    case Direction.East:    startingDir = Vector3.right;
                                            break;

                    case Direction.West:    startingDir = -Vector3.right;
                                            break;

                    case Direction.North:   startingDir = Vector3.up;
                                            break;

                    case Direction.South:   startingDir = -Vector3.up;    
                                            break;
                }
            }

            string sectionName = "Sezione " + i;

            if( lines.ContainsKey( lineName ) ) {
                sections = lines[ lineName ];
            }
            else {
                lines.Add( lineName, sections );
            }

            if( sections.Count > 0 ) {
                startingDir =  sections[ ^1 ].nextStartingDirections[ 0 ];
                startingPoint = sections[ ^1 ].nextStartingPoints[ 0 ];
            }

            LineSection section = new LineSection();
            section.previousSection = previousSection;

            if( i > 0 && i < sectionsNumber - 1 && ( i % stationsDistance == 0 || i % switchDistance == 0 ) ) {

                section.bidirectional = sections[ i - 1 ].bidirectional;

                List<Vector3> nextStartingDirections = new List<Vector3>();
                nextStartingDirections.Add( startingDir );
                section.nextStartingDirections = nextStartingDirections;

                List<Vector3> nextStartingPoints = new List<Vector3>();
                if( i % stationsDistance == 0 ) {
                    section.type = Type.Station;
                    int variant = Random.Range( 0, 2 );
                    if( variant == 0 ) {
                        // Variante con banchine su entrambi i lati
                        nextStartingPoints.Add( startingPoint + ( startingDir.normalized * stationLenght ) );
                        section.stationType = StationType.BothSidesPlatform;
                    }
                    else if(variant == 1 ) {
                        // Variante con banchina centrale
                        nextStartingPoints.Add( startingPoint + ( startingDir.normalized * ( stationLenght + ( 2 * stationExtensionLenght ) ) ) );
                        section.stationType = StationType.CentralPlatform;
                    }

                    nextStartingPoints.Add( startingPoint + ( startingDir.normalized * stationLenght ) );
                    section.nextStartingPoints = nextStartingPoints;
                    stationsDistance += Random.Range( stationsDistanceMin, stationsDistanceMax );
                    if( switchDistance == stationsDistance ) {
                        switchDistance = stationsDistance + 1;
                    }
                }
                else if( i % switchDistance == 0 ) { 
                    section.type = Type.Switch;

                    nextStartingPoints.Add( startingPoint + ( startingDir.normalized * switchLenght ) );
                    section.nextStartingPoints = nextStartingPoints;
                    switchDistance += Random.Range( switchDistanceMin, switchDistanceMax );
                }

                List<Vector3> curvePoints = new List<Vector3>{ startingPoint, nextStartingPoints[ 0 ] };
                section.controlsPoints = curvePoints;
                section.bezierCurveLimitedAngle = curvePoints;
                section.curvePointsCount = 2;
                if( section.type == Type.Switch ) {
                    section.centerCoords = curvePoints[ 0 ] + ( startingDir.normalized * ( curvePoints[ 1 ] - curvePoints[ 0 ] ).magnitude / 2 );
                }
                
            }
            else {
                section.type = Type.Tunnel;

                List<Vector3> controlPoints = GenerateControlPoints( sectionOrientation, startingDir, startingPoint, distanceMultiplier, controlPointsNumber, tunnelStraightness );

                List<Vector3> baseCurve = BezierCurveCalculator.CalculateBezierCurve( controlPoints, baseBezierCurvePointsNumber );
                List<Vector3> fixedLenghtCurve = BezierCurveCalculator.RecalcultateCurveWithFixedLenght( baseCurve, baseCurve.Count );
                List<Vector3> limitedAngleCurve = BezierCurveCalculator.RecalcultateCurveWithLimitedAngle( fixedLenghtCurve, maxAngle, startingDir );

                // Update dettagli LineSection 
                section.controlsPoints = controlPoints;
                section.bezierCurveBase = baseCurve;
                section.bezierCurveFixedLenght = fixedLenghtCurve;
                section.bezierCurveLimitedAngle = limitedAngleCurve;
                section.curvePointsCount = limitedAngleCurve.Count;

                List<Vector3> nextStartingDirections = new List<Vector3>();
                nextStartingDirections.Add( limitedAngleCurve[ ^1 ] - limitedAngleCurve[ ^2 ] );
                section.nextStartingDirections = nextStartingDirections;

                List<Vector3> nextStartingPoints = new List<Vector3>();
                nextStartingPoints.Add( limitedAngleCurve[ ^1 ] );
                section.nextStartingPoints = nextStartingPoints;

                List<Vector3> bordersSectionUp = new List<Vector3>();
                List<Vector3> bordersSectionDown = new List<Vector3>();
            }

            section.orientation = sectionOrientation;
            section.sectionIndex = i;
            section.lineName = lineName;
            if( i == 0 ) {
                section.fromSection = fromSection;
            }

            sections.Add( section );
            previousSection = section;

            foreach( Vector3 curvePoint in section.bezierCurveLimitedAngle ) {
                if( isPointInsideProibitedArea( curvePoint, lineName ) ) {
                    removeLine = true;
                    break;
                }
            }

            if( i == lineLength - 1  && sections.Count != 0 ) {
                List<LineSection> lastSections = new List<LineSection>();
                for( int k = lineTurnDistance; k < lineLength; k++ ) {
                    lastSections.Add( sections[ k ] );
                }
                this.proibitedAreas[ lineName ].Add( CalculateProibitedRectangularArea( lastSections, sectionOrientation ) );
            }
        }

        if( removeLine ) {
            this.proibitedAreas.Remove( lineName );
            this.lines.Remove( lineName );
        }
        else {
            if( generateNewLines ) {
                foreach( LineSection finalSection in sections ) {
                    if( finalSection.type == Type.Switch ) {
                        Vector3 finalDir = ( finalSection.nextStartingPoints[ 0 ] - finalSection.bezierCurveLimitedAngle[ 0 ] );
                        Vector3 switchCenter = finalSection.bezierCurveLimitedAngle[ 0 ] + ( finalDir / 2 );
                        Vector3 switchLeft = switchCenter + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * finalDir.normalized * switchBracketsLenght;
                        Vector3 switchRight = switchCenter + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * finalDir.normalized * switchBracketsLenght;

                        bool leftAvailable = !isPointInsideProibitedArea( switchLeft, null );
                        bool rightAvailable = !isPointInsideProibitedArea( switchRight, null );

                        Dictionary<NewLineSide, LineStart> newLinesStarts = new Dictionary<NewLineSide, LineStart>();
                        switch( finalSection.orientation ) {
                            case Direction.East:    if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.North, NewLineSide.Left, switchLeft, switchLeft - switchCenter);
                                                        newLinesStarts.Add( NewLineSide.Left, newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.South, NewLineSide.Right, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Right, newLineStart );
                                                    }
                                                    break;

                            case Direction.West:    if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.South, NewLineSide.Left, switchLeft, switchLeft - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Left, newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.North, NewLineSide.Right, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Right, newLineStart );
                                                    }
                                                    break;

                            case Direction.North:   if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.West, NewLineSide.Left, switchLeft, switchLeft - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Left, newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.East, NewLineSide.Right, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Right, newLineStart );
                                                    }
                                                    break;

                            case Direction.South:   if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.East, NewLineSide.Left, switchLeft, switchLeft - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Left, newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.West, NewLineSide.Right, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Right, newLineStart );
                                                    }   
                                                    break;
                        }

                        if( leftAvailable && rightAvailable ) {
                            finalSection.newLineSide = NewLineSide.Both;
                        } 
                        else if( leftAvailable ) {
                            finalSection.newLineSide = NewLineSide.Left;
                        }
                        else if( leftAvailable ) {
                            finalSection.newLineSide = NewLineSide.Right;
                        } 
                        finalSection.newLinesStarts = newLinesStarts;
                    }
                }
            }
        }

        lineCounter++;

        return sections[ ^1 ].bezierCurveLimitedAngle[ ^1 ];
    }

    private bool isPointInsideProibitedArea( Vector3 point, string excludedLine ) {
        foreach( string lineName in this.proibitedAreas.Keys ) {
            
            if( excludedLine != lineName ) {
                List<List<Vector3>> proibitedAreas = this.proibitedAreas[ lineName ];
                foreach( List<Vector3> proibitedAreaVertex in proibitedAreas) {

                    float xMin = proibitedAreaVertex[ 0 ].x, xMax = proibitedAreaVertex[ 0 ].x, yMin = proibitedAreaVertex[ 0 ].y, yMax = proibitedAreaVertex[ 0 ].y;
                    for( int i = 1; i < proibitedAreaVertex.Count; i++ ) {
                        
                        if( xMin > proibitedAreaVertex[ i ].x ) {
                            xMin = proibitedAreaVertex[ i ].x;
                        }
                        if( xMax < proibitedAreaVertex[ i ].x ) {
                            xMax = proibitedAreaVertex[ i ].x;
                        }

                        if( yMin > proibitedAreaVertex[ i ].y ) {
                            yMin = proibitedAreaVertex[ i ].y;
                        }
                        if( yMax < proibitedAreaVertex[ i ].y ) {
                            yMax = proibitedAreaVertex[ i ].y;
                        }
                    }

                    if( point.x >= xMin && point.x <= xMax && point.y >= yMin && point.y <= yMax ) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private List<Vector3> CalculateProibitedRectangularArea( List<LineSection> sections, Direction lineOrientation ) {

        float xMin = 0.0f, xMax = 0.0f, yMin = 0.0f, yMax = 0.0f;

        for( int i = 0; i < sections.Count; i++ ) {
            for( int j = 0; j < sections[ i ].bezierCurveLimitedAngle.Count; j++ ) {
                if( i == 0 && j == 0 ) {
                    xMin = xMax = sections[ i ].bezierCurveLimitedAngle[ j ].x;
                    yMin = yMax = sections[ i ].bezierCurveLimitedAngle[ j ].y;
                }
                else {
                    if( xMin > sections[ i ].bezierCurveLimitedAngle[ j ].x ) {
                        xMin = sections[ i ].bezierCurveLimitedAngle[ j ].x;
                    }
                    if( xMax < sections[ i ].bezierCurveLimitedAngle[ j ].x ) {
                        xMax = sections[ i ].bezierCurveLimitedAngle[ j ].x;
                    }

                    if( yMin > sections[ i ].bezierCurveLimitedAngle[ j ].y ) {
                        yMin = sections[ i ].bezierCurveLimitedAngle[ j ].y;
                    }
                    if( yMax < sections[ i ].bezierCurveLimitedAngle[ j ].y ) {
                        yMax = sections[ i ].bezierCurveLimitedAngle[ j ].y;
                    }
                }
            }
        }

        switch( lineOrientation ) {
            case Direction.East: case Direction.West:   yMin -= ( tunnelWidth+ 2 );
                                                        yMax += ( tunnelWidth+ 2 );
                                                        break;

            case Direction.North: case Direction.South: xMin -= ( tunnelWidth+ 2 );
                                                        xMax += ( tunnelWidth+ 2 );
                                                        break;
        }

        return new List<Vector3> { new Vector3( xMin, yMin, 0.0f ), new Vector3( xMax, yMin, 0.0f ), new Vector3( xMax, yMax, 0.0f ), new Vector3( xMin, yMax, 0.0f ) };
    }

    private void InstantiateTrain() {
        Vector3 trainPos = lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        GameObject instantiatedTrain = Instantiate( train, trainPos, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, trainDir, Vector3.forward ) ) );
        instantiatedTrain.name = "Train";

        InstantiateMainCamera();
    }

    private void InstantiateMainCamera() {
        Vector3 trainPos = lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        mainCamera.tag = "MainCamera";
        Instantiate( mainCamera, trainPos, Quaternion.identity );
    }

    private List<Vector3> GenerateControlPoints( Direction orientation, Vector3 startingDir, Vector3 startingPoint, int pointsDistanceMultiplier, int pointsNumber, float straightness ) {
        List<Vector3> controlPoints = new List<Vector3>();
        controlPoints.Add( startingPoint );
        Vector3 furthermostPoint;
            
        controlPoints.Add( startingPoint + ( startingDir.normalized * pointsDistanceMultiplier ) );
        
        furthermostPoint = controlPoints[ 1 ];
        //if( furthermostPoint.x < controlPoints[ 0 ].x ) {
            //furthermostPoint = new Vector3( controlPoints[ 0 ].x, furthermostPoint.y, furthermostPoint.z );
        //}

        for( int i = 1; i < pointsNumber; i++ ) { 

            Vector2 range = new Vector2( -90.0f, 90.0f );
            range.x += 90.0f * straightness;
            range.y -= 90.0f * straightness;
            float angle = Random.Range( range.x, range.y );

            switch( orientation ) {
                case Direction.East:    angle += 0.0f;
                                        break;

                case Direction.West:    angle += 180.0f;
                                        break;

                case Direction.North:   angle += 90.0f;
                                        break;

                case Direction.South:   angle -= 90.0f;    
                                        break;
            }

            Vector3 newDir = Quaternion.Euler( 0.0f, 0.0f, angle ) * Vector3.right;
            furthermostPoint = furthermostPoint + ( newDir.normalized * pointsDistanceMultiplier );
            controlPoints.Add( furthermostPoint );
        }
        
        return controlPoints;
    }

    private void OnDrawGizmos()
    {
        if( lineGizmos ) {
            foreach( string lineName in this.lines.Keys ) {

                foreach( LineSection segment in this.lines[ lineName ] ) {

                    if( segment.type == Type.Tunnel ) {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere( segment.bezierCurveLimitedAngle[ 0 ], 5.0f );
                    }
                    else if( segment.type == Type.Switch ) {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawSphere( segment.bezierCurveLimitedAngle[ 0 ], 5.0f );
                    }
                    else if( segment.type == Type.Station ) {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere( segment.bezierCurveLimitedAngle[ 0 ], 5.0f );
                    }

                    for( int i = 0; i < segment.bezierCurveLimitedAngle.Count; i++ ) {

                        
                        if( i > 0 ) {

                            //if( segment.type == Type.Tunnel ) {
                                if( segment.orientation == Direction.North ) {
                                    Gizmos.color = Color.yellow;
                                }
                                else if( segment.orientation == Direction.South ) {
                                    Gizmos.color = Color.cyan;
                                }
                                else if( segment.orientation == Direction.East ) {
                                    Gizmos.color = Color.green;
                                }
                                else if( segment.orientation == Direction.West ) {
                                    Gizmos.color = Color.white;
                                }
                            //}
                            //else if( segment.type == Type.Switch ) {
                                //Gizmos.color = Color.magenta;
                            //}
                            //else if( segment.type == Type.Station ) {
                                //Gizmos.color = Color.green;
                            //}
                            Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ i - 1 ], segment.bezierCurveLimitedAngle[ i ] );
                            Gizmos.DrawWireSphere( segment.bezierCurveLimitedAngle[ i ], 1.0f );
                        }
                    }

                    if( segment.newLinesStarts != null && segment.newLinesStarts.Count > 0 ) {
                        foreach( NewLineSide side in segment.newLinesStarts.Keys ) {
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawWireCube( segment.newLinesStarts[ side ].pos, Vector3.one * 25 );
                            Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ 0 ], segment.newLinesStarts[ side ].pos );
                        }
                    }
                }
            }

            foreach( string lineName in this.proibitedAreas.Keys ) {
                Gizmos.color = Color.red;

                List<List<Vector3>> proibitedAreas = this.proibitedAreas[ lineName ];
                foreach( List<Vector3> proibitedAreaVertex in proibitedAreas) {
                    for( int i = 0; i < proibitedAreaVertex.Count; i++ ) {
                        
                        if( i < proibitedAreaVertex.Count - 1 ) {
                            Gizmos.DrawLine( proibitedAreaVertex[ i ], proibitedAreaVertex[ i + 1 ] );
                        }
                        else {
                            Gizmos.DrawLine( proibitedAreaVertex[ i ], proibitedAreaVertex[ 0 ] );
                        }
                    }
                }
            }   
        }

        foreach( string line in lines.Keys ) {

            foreach( LineSection segment in lines[ line ] ) {

                if(  segment.type == Type.Switch ) {
                    if( segment.switchType == SwitchType.BiToBi ) {
                        for( int i = 0; i < segment.floorPoints.leftLine.Count; i++ ) {
                            
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.LeftToLeft ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftLine[ i - 1 ], segment.floorPoints.leftLine[ i ] );

                                if( segment.activeSwitch == SwitchDirection.RightToRight ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightLine[ i - 1 ], segment.floorPoints.rightLine[ i ] );
                            }
                        }
                        for( int i = 0; i < segment.floorPoints.leftRightLine.Count; i++ ) {
                            
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.LeftToRight ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftRightLine[ i - 1 ], segment.floorPoints.leftRightLine[ i ] );
                                if( segment.activeSwitch == SwitchDirection.RightToLeft ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightLeftLine[ i - 1 ], segment.floorPoints.rightLeftLine[ i ] );
                            }
                        }
                    }
                    else if( segment.switchType == SwitchType.BiToMono || segment.switchType == SwitchType.MonoToBi ) {
                        for( int i = 0; i < segment.floorPoints.leftCenterLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.LeftToCenter ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftCenterLine[ i - 1 ], segment.floorPoints.leftCenterLine[ i ] );
                            }
                        }
                        for( int i = 0; i < segment.floorPoints.rightCenterLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.RightToCenter ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightCenterLine[ i - 1 ], segment.floorPoints.rightCenterLine[ i ] );
                            }
                        }
                    }

                    if( segment.switchType == SwitchType.MonoToNewMono ) {
                        for( int i = 0; i < segment.floorPoints.centerLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.CenterToCenter ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.centerLine[ i - 1 ], segment.floorPoints.centerLine[ i ] );
                            }
                        }
                        if( segment.floorPoints.centerEntranceLeft != null ) {
                            for( int i = 0; i < segment.floorPoints.centerEntranceLeft.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.CenterToEntranceLeft ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.blue;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.centerEntranceLeft[ i - 1 ], segment.floorPoints.centerEntranceLeft[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.centerEntranceRight != null ) {
                            for( int i = 0; i < segment.floorPoints.centerEntranceRight.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.CenterToEntranceRight ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.blue;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.centerEntranceRight[ i - 1 ], segment.floorPoints.centerEntranceRight[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.centerExitLeft != null ) {
                            for( int i = 0; i < segment.floorPoints.centerExitLeft.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.CenterToExitLeft ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.red;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.centerExitLeft[ i - 1 ], segment.floorPoints.centerExitLeft[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.centerExitRight != null ) {
                            for( int i = 0; i < segment.floorPoints.centerExitRight.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.CenterToExitRight ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.red;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.centerExitRight[ i - 1 ], segment.floorPoints.centerExitRight[ i ] );
                                }
                            }
                        }
                    }
                    else if( segment.switchType == SwitchType.BiToNewBi ) {
                        for( int i = 0; i < segment.floorPoints.leftLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.LeftToLeft ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftLine[ i - 1 ], segment.floorPoints.leftLine[ i ] );
                            }
                        }
                        for( int i = 0; i < segment.floorPoints.rightLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.RightToRight ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightLine[ i - 1 ], segment.floorPoints.rightLine[ i ] );
                            }
                        }
                        if( segment.floorPoints.leftEntranceLeft != null ) {
                            for( int i = 0; i < segment.floorPoints.leftEntranceLeft.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.LeftToEntranceLeft ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.blue;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.leftEntranceLeft[ i - 1 ], segment.floorPoints.leftEntranceLeft[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.rightEntranceRight != null ) {
                            for( int i = 0; i < segment.floorPoints.rightEntranceRight.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.RightToEntranceRight ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.blue;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.rightEntranceRight[ i - 1 ], segment.floorPoints.rightEntranceRight[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.leftExitLeft != null ) {
                            for( int i = 0; i < segment.floorPoints.leftExitLeft.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.LeftToExitLeft ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.red;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.leftExitLeft[ i - 1 ], segment.floorPoints.leftExitLeft[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.rightExitRight != null ) {
                            for( int i = 0; i < segment.floorPoints.rightExitRight.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.RightToExitRight ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.red;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.rightExitRight[ i - 1 ], segment.floorPoints.rightExitRight[ i ] );
                                }
                            }
                        }
                    }
                }
            }
        }
    }   
}
