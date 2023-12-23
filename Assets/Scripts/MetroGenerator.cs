using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetroGenerator : MonoBehaviour
{
    public float cellSize = 150.0f;
    public GameObject train;
    public GameObject mainCamera;
    public float trainHeightFromGround = 1.5f;
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
    public GameObject pillar;
    public int baseBezierCurvePointsNumber = 50;
    public bool tunnelParabolic = false;
    public float tunnelWidth = 5.0f;
    public float centerWidth = 5.0f;
    public float platformHeight = 0.5f;
    public float platformWidth = 3.5f;
    public float tunnelStraightness = 0.5f;
    public Material tunnelRailTexture;
    public Vector2 tunnelRailTextureTilting;
    public Material platformSideTexture;
    public Vector2 platformSideTextureTilting;
    public Material platformFloorTexture;
    public Vector2 platformFloorTextureTilting;
    public Material switchRailTexture;
    public Vector2 switchRailTextureTilting;
    public Material switchGroundTexture;
    public Vector2 switchGroundTextureTilting;
    public Material centerTexture;
    public Vector2 centerTextureTilting;
    public bool startingBidirectional = true;
    public Dictionary<string, List<LineSection>> lines = new Dictionary<string, List<LineSection>>();
    public Dictionary<string, List<List<Vector3>>> proibitedAreas = new Dictionary<string, List<List<Vector3>>>();
    private int lineCounter = 0;
    public int lineNumber = 1;
    public int lineTurnDistanceMin = 5;
    public int lineTurnDistanceMax = 20;
    private int lineTurnDistance;
    public float switchBracketsLenght = 50.0f;
    public bool newLineFromSwitch = false;
    public GameObject switchLight;
    public float switchLightDistance;
    public float switchLightHeight;
    public Vector3 switchLightRotation;
    public bool lineGizmos = true;
    
    public enum Direction{
        North,
        South,
        East,
        West,
        Random,
        None,
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateMetro();
    }

    public void GenerateMetro() {

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
        InstantiateTrain();
    }

    private void GenerateFloorMesh( LineSection section, GameObject sectionGameObj ) {
        MeshGenerator.Floor floorVertexPoints = new MeshGenerator.Floor();
        if( section.bidirectional ) {
            Mesh leftFloorMesh = new Mesh();
            Mesh centerFloorMesh = new Mesh();
            Mesh rightFloorMesh = new Mesh();

            floorVertexPoints = MeshGenerator.CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( section, centerWidth, tunnelWidth );

            leftFloorMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.leftL, floorVertexPoints.leftR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
            GameObject leftFloorGameObj = new GameObject( "Binari sinistra" );
            leftFloorGameObj.transform.parent = sectionGameObj.transform;
            leftFloorGameObj.transform.position = Vector3.zero;
            leftFloorGameObj.AddComponent<MeshFilter>();
            leftFloorGameObj.AddComponent<MeshRenderer>();
            leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
            leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

            centerFloorMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.centerL, floorVertexPoints.centerR ), centerTextureTilting.x, centerTextureTilting.y );
            GameObject centerFloorGameObj = new GameObject( "Divisore centrale" );
            centerFloorGameObj.transform.parent = sectionGameObj.transform;
            centerFloorGameObj.transform.position = Vector3.zero;
            centerFloorGameObj.AddComponent<MeshFilter>();
            centerFloorGameObj.AddComponent<MeshRenderer>();
            centerFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
            centerFloorGameObj.GetComponent<MeshRenderer>().material = centerTexture;

            for( int p = 0; p < floorVertexPoints.centerLine.Count; p += 2 ) {
                GameObject pillarGameObj = Instantiate( pillar, floorVertexPoints.centerLine[ p ] + new Vector3( 0.0f, 0.0f, -pillar.transform.localScale.y / 2 ), Quaternion.Euler( 0.0f, -90.0f, 90.0f ) );
                pillarGameObj.transform.parent = sectionGameObj.transform;
                pillarGameObj.name = "Pilastro";
            }

            rightFloorMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.rightL, floorVertexPoints.rightR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
            GameObject rightFloorGameObj = new GameObject( "Binari destra" );
            rightFloorGameObj.transform.parent = sectionGameObj.transform;
            rightFloorGameObj.transform.position = Vector3.zero;
            rightFloorGameObj.AddComponent<MeshFilter>();
            rightFloorGameObj.AddComponent<MeshRenderer>();
            rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
            rightFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

            section.bidirectional = true;
        }
        else {
            Mesh floorMesh = new Mesh();

            floorVertexPoints = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.bezierCurveLimitedAngle, section.controlsPoints, tunnelWidth, tunnelParabolic );

            floorMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.centerL, floorVertexPoints.centerR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );

            GameObject floorGameObj = new GameObject( "Binari centrali" );
            floorGameObj.transform.parent = sectionGameObj.transform;
            floorGameObj.transform.position = Vector3.zero;
            floorGameObj.AddComponent<MeshFilter>();
            floorGameObj.AddComponent<MeshRenderer>();
            floorGameObj.GetComponent<MeshFilter>().sharedMesh = floorMesh;
            floorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

            section.bidirectional = false;
        }

        // Update dettagli LineSection 
        section.floorPoints = floorVertexPoints;
    }

    private void GenerateSidePlatformMesh( LineSection section, GameObject sectionGameObj ) {
        MeshGenerator.PlatformSide platformSidesVertexPoints = new MeshGenerator.PlatformSide();

            Mesh platformSideLeftMesh = new Mesh();
            Mesh platformSideRightMesh = new Mesh();
            Mesh platformFloorLeftMesh = new Mesh();
            Mesh platformFloorRightMesh = new Mesh();

            float sectionWidth = section.bidirectional ? ( ( tunnelWidth * 2 ) + centerWidth ) : tunnelWidth;

            platformSidesVertexPoints = MeshGenerator.CalculateMonodirectionalPlatformSidesMeshesVertex( section.bezierCurveLimitedAngle, section.controlsPoints, sectionWidth, tunnelParabolic, platformHeight, platformWidth );

            platformSideLeftMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( platformSidesVertexPoints.leftUp, platformSidesVertexPoints.leftDown ), platformSideTextureTilting.x, platformSideTextureTilting.y );
            platformSideRightMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( platformSidesVertexPoints.rightDown, platformSidesVertexPoints.rightUp ), platformSideTextureTilting.x, platformSideTextureTilting.y );
            platformFloorLeftMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( platformSidesVertexPoints.leftFloorLeft, platformSidesVertexPoints.leftFloorRight ), platformFloorTextureTilting.x, platformFloorTextureTilting.y );
            platformFloorRightMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( platformSidesVertexPoints.rightFloorLeft, platformSidesVertexPoints.rightFloorRight ), platformFloorTextureTilting.x, platformFloorTextureTilting.y );

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
            platformFloorLeftGameObj.GetComponent<MeshFilter>().sharedMesh = platformFloorLeftMesh;
            platformFloorLeftGameObj.GetComponent<MeshRenderer>().material = platformFloorTexture;

            GameObject platformFloorRightGameObj = new GameObject( "Piattaforma destra (pavimento)" );
            platformFloorRightGameObj.transform.parent = sectionGameObj.transform;
            platformFloorRightGameObj.transform.position = Vector3.zero;
            platformFloorRightGameObj.AddComponent<MeshFilter>();
            platformFloorRightGameObj.AddComponent<MeshRenderer>();
            platformFloorRightGameObj.GetComponent<MeshFilter>().sharedMesh = platformFloorRightMesh;
            platformFloorRightGameObj.GetComponent<MeshRenderer>().material = platformFloorTexture;

        // Update dettagli LineSection 
        section.platformSidesPoints = platformSidesVertexPoints;
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

                switch( section.type ) {
                    case Type.Tunnel:   GenerateFloorMesh( section, sectionGameObj );
                                        GenerateSidePlatformMesh( section, sectionGameObj );

                                        break;

                    case Type.Station:  List<Vector3> stationsPoints = section.bezierCurveLimitedAngle;

                                        MeshGenerator.Floor stationRails = new MeshGenerator.Floor();

                                        if( section.bidirectional ) {

                                            if( section.stationType == StationType.BothSidesPlatform ) {
                                            
                                                stationRails = MeshGenerator.CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( section, centerWidth, tunnelWidth );

                                                Mesh leftFloorMesh = new Mesh();
                                                Mesh centerFloorMesh = new Mesh();
                                                Mesh rightFloorMesh = new Mesh();

                                                leftFloorMesh = MeshGenerator.GenerateFloorMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.leftL, stationRails.leftR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                GameObject leftFloorGameObj = new GameObject( "Binari sinistra" );
                                                leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                leftFloorGameObj.transform.position = Vector3.zero;
                                                leftFloorGameObj.AddComponent<MeshFilter>();
                                                leftFloorGameObj.AddComponent<MeshRenderer>();
                                                leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                                leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

                                                centerFloorMesh = MeshGenerator.GenerateFloorMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.centerL, stationRails.centerR ), centerTextureTilting.x, centerTextureTilting.y );
                                                GameObject centerFloorGameObj = new GameObject( "Divisore centrale" );
                                                centerFloorGameObj.transform.parent = sectionGameObj.transform;
                                                centerFloorGameObj.transform.position = Vector3.zero;
                                                centerFloorGameObj.AddComponent<MeshFilter>();
                                                centerFloorGameObj.AddComponent<MeshRenderer>();
                                                centerFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
                                                centerFloorGameObj.GetComponent<MeshRenderer>().material = centerTexture;

                                                for( int p = 0; p < stationRails.centerLine.Count; p += 2 ) {
                                                    GameObject pillarGameObj = Instantiate( pillar, stationRails.centerLine[ p ] + new Vector3( 0.0f, 0.0f, -pillar.transform.localScale.y / 2 ), Quaternion.Euler( 0.0f, -90.0f, 90.0f ) );
                                                    pillarGameObj.transform.parent = sectionGameObj.transform;
                                                    pillarGameObj.name = "Pilastro";
                                                }

                                                rightFloorMesh = MeshGenerator.GenerateFloorMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.rightL, stationRails.rightR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
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

                                                leftFloorMesh = MeshGenerator.GenerateFloorMesh( stationRails.leftLine, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.leftL, stationRails.leftR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                GameObject leftFloorGameObj = new GameObject( "Binari sinistra" );
                                                leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                leftFloorGameObj.transform.position = Vector3.zero;
                                                leftFloorGameObj.AddComponent<MeshFilter>();
                                                leftFloorGameObj.AddComponent<MeshRenderer>();
                                                leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                                leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

                                                rightFloorMesh = MeshGenerator.GenerateFloorMesh( stationRails.rightLine, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.rightL, stationRails.rightR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
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
                                            stationRails = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( stationsPoints, stationsPoints, tunnelWidth, tunnelParabolic );

                                            Mesh centerFloorMesh = new Mesh();

                                            centerFloorMesh = MeshGenerator.GenerateFloorMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.centerL, stationRails.centerR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
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

                    case Type.Switch:   SwitchPath switchPath = SwitchPath.CreateInstance( switchLenght, switchBracketsLenght, centerWidth, tunnelWidth, switchLightDistance, switchLightHeight, baseBezierCurvePointsNumber, switchLightRotation, switchLight );

                                        bool previousBidirectional = this.lines[ lineName ][ i - 1 ].bidirectional;

                                        LineSection switchSection = new LineSection();

                                        //Debug.Log( "previousBidirectional: " + previousBidirectional );

                                        if( previousBidirectional ) {
                                            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                                                switchSection = switchPath.generateBiToNewBiSwitch( section, this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                                            }
                                            else {

                                                if( Random.Range( 0, 2 ) == 0 ) {
                                                    switchSection = switchPath.generateBiToBiSwitch( i, this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                                                }
                                                else{
                                                    switchSection = switchPath.generateBiToMonoSwitch( i, this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                                                }
                                            }
                                        }
                                        else {
                                            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                                                switchSection = switchPath.generateMonoToNewMonoSwitch( section, this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                                            }
                                            else {

                                                if( Random.Range( 0, 2 ) == 0 || lineCounter > 0 ) {
                                                    switchSection = switchPath.generateMonoToBiSwitch( i, this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
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
                                            case SwitchType.BiToMono:       MeshGenerator.Floor centerToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftCenterLine, section.floorPoints.leftCenterLine, tunnelWidth, false );
                                                                            MeshGenerator.Floor centerToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightCenterLine, section.floorPoints.rightCenterLine, tunnelWidth, false );
                                                                            

                                                                            Mesh centerToLeftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.leftCenterLine, MeshGenerator.ConvertListsToMatrix_2xM( centerToLeftFloor.centerL, centerToLeftFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                            GameObject centerToLeftFloorGameObj = new GameObject( "Scambio Binario Centrale - Sinistro" );
                                                                            centerToLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                            centerToLeftFloorGameObj.transform.position = Vector3.zero;
                                                                            centerToLeftFloorGameObj.AddComponent<MeshFilter>();
                                                                            centerToLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                                            centerToLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToLeftFloorMesh;
                                                                            centerToLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                            Mesh centerToRightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.rightCenterLine, MeshGenerator.ConvertListsToMatrix_2xM( centerToRightFloor.centerL, centerToRightFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                            GameObject centerToRightFloorGameObj = new GameObject( "Scambio Binario Centrale - Sinistro" );
                                                                            centerToRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                            centerToRightFloorGameObj.transform.position = Vector3.zero;
                                                                            centerToRightFloorGameObj.AddComponent<MeshFilter>();
                                                                            centerToRightFloorGameObj.AddComponent<MeshRenderer>();
                                                                            centerToRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToRightFloorMesh;
                                                                            centerToRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                                                            
                                                                            break;

                                            case SwitchType.BiToBi:         MeshGenerator.Floor leftToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, section.floorPoints.leftLine, tunnelWidth, false );
                                                                            MeshGenerator.Floor rightToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, section.floorPoints.rightLine, tunnelWidth, false );
                                                                            MeshGenerator.Floor leftToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftRightLine, section.floorPoints.leftRightLine, tunnelWidth, false );
                                                                            MeshGenerator.Floor rightToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLeftLine, section.floorPoints.rightLeftLine, tunnelWidth, false );
                                                                            

                                                                            Mesh leftToLeftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.leftLine, MeshGenerator.ConvertListsToMatrix_2xM( leftToLeftFloor.centerL, leftToLeftFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                            GameObject leftToLeftFloorGameObj = new GameObject( "Scambio Binario Sinistro - Sinistro" );
                                                                            leftToLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                            leftToLeftFloorGameObj.transform.position = Vector3.zero;
                                                                            leftToLeftFloorGameObj.AddComponent<MeshFilter>();
                                                                            leftToLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                                            leftToLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftToLeftFloorMesh;
                                                                            leftToLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                            Mesh rightToRightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.rightLine, MeshGenerator.ConvertListsToMatrix_2xM( rightToRightFloor.centerL, rightToRightFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                            GameObject rightToRightFloorGameObj = new GameObject( "Scambio Binario Destro - Destro" );
                                                                            rightToRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                            rightToRightFloorGameObj.transform.position = Vector3.zero;
                                                                            rightToRightFloorGameObj.AddComponent<MeshFilter>();
                                                                            rightToRightFloorGameObj.AddComponent<MeshRenderer>();
                                                                            rightToRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightToRightFloorMesh;
                                                                            rightToRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                            Mesh leftToRightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.leftRightLine, MeshGenerator.ConvertListsToMatrix_2xM( leftToRightFloor.centerL, leftToRightFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                            GameObject leftToRightFloorGameObj = new GameObject( "Scambio Binario Sinistro - Destro" );
                                                                            leftToRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                            leftToRightFloorGameObj.transform.position = Vector3.zero;
                                                                            leftToRightFloorGameObj.AddComponent<MeshFilter>();
                                                                            leftToRightFloorGameObj.AddComponent<MeshRenderer>();
                                                                            leftToRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftToRightFloorMesh;
                                                                            leftToRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                            Mesh rightToLeftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.rightLeftLine, MeshGenerator.ConvertListsToMatrix_2xM( rightToLeftFloor.centerL, rightToLeftFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                            GameObject rightToLeftFloorGameObj = new GameObject( "Scambio Binario Destro - Sinistro" );
                                                                            rightToLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                            rightToLeftFloorGameObj.transform.position = Vector3.zero;
                                                                            rightToLeftFloorGameObj.AddComponent<MeshFilter>();
                                                                            rightToLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                                            rightToLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightToLeftFloorMesh;
                                                                            rightToLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                                                            
                                                                            break;

                                            case SwitchType.MonoToNewMono:  MeshGenerator.Floor centerToCenterFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerLine, section.floorPoints.centerLine, tunnelWidth, false );
                                                                            
                                                                            Mesh centerToCenterFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.centerLine, MeshGenerator.ConvertListsToMatrix_2xM( centerToCenterFloor.centerL, centerToCenterFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                            GameObject centerToCenterFloorGameObj = new GameObject( "Scambio Binario Centrale - Centrale" );
                                                                            centerToCenterFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                            centerToCenterFloorGameObj.transform.position = Vector3.zero;
                                                                            centerToCenterFloorGameObj.AddComponent<MeshFilter>();
                                                                            centerToCenterFloorGameObj.AddComponent<MeshRenderer>();
                                                                            centerToCenterFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToCenterFloorMesh;
                                                                            centerToCenterFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Left ) ) {
                                                                                MeshGenerator.Floor centerToEntranceLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerEntranceLeft, section.floorPoints.centerEntranceLeft, tunnelWidth, false );
                                                                                Mesh centerToEntranceLeftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.centerEntranceLeft, MeshGenerator.ConvertListsToMatrix_2xM( centerToEntranceLeftFloor.centerL, centerToEntranceLeftFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject centerToEntranceLeftFloorGameObj = new GameObject( "Scambio Binario Centrale - Ingresso Sinistro" );
                                                                                centerToEntranceLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                centerToEntranceLeftFloorGameObj.transform.position = Vector3.zero;
                                                                                centerToEntranceLeftFloorGameObj.AddComponent<MeshFilter>();
                                                                                centerToEntranceLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                                                centerToEntranceLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToEntranceLeftFloorMesh;
                                                                                centerToEntranceLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                                MeshGenerator.Floor centerToExitLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerExitLeft, section.floorPoints.centerExitLeft, tunnelWidth, false );
                                                                                Mesh centerToExitLeftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.centerExitLeft, MeshGenerator.ConvertListsToMatrix_2xM( centerToExitLeftFloor.centerL, centerToExitLeftFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject centerToExitLeftFloorGameObj = new GameObject( "Scambio Binario Centrale - Uscita Sinistra" );
                                                                                centerToExitLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                centerToExitLeftFloorGameObj.transform.position = Vector3.zero;
                                                                                centerToExitLeftFloorGameObj.AddComponent<MeshFilter>();
                                                                                centerToExitLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                                                centerToExitLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToExitLeftFloorMesh;
                                                                                centerToExitLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                                                            }

                                                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Right ) ) {
                                                                                MeshGenerator.Floor centerToEntranceRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerEntranceRight, section.floorPoints.centerEntranceRight, tunnelWidth, false );
                                                                                Mesh centerToEntranceRightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.centerEntranceRight, MeshGenerator.ConvertListsToMatrix_2xM( centerToEntranceRightFloor.centerL, centerToEntranceRightFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject centerToEntranceRightFloorGameObj = new GameObject( "Scambio Binario Centrale - Ingresso Destro" );
                                                                                centerToEntranceRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                centerToEntranceRightFloorGameObj.transform.position = Vector3.zero;
                                                                                centerToEntranceRightFloorGameObj.AddComponent<MeshFilter>();
                                                                                centerToEntranceRightFloorGameObj.AddComponent<MeshRenderer>();
                                                                                centerToEntranceRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToEntranceRightFloorMesh;
                                                                                centerToEntranceRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                                MeshGenerator.Floor centerToExitRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerExitRight, section.floorPoints.centerExitRight, tunnelWidth, false );
                                                                                Mesh centerToExitRightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.centerExitRight, MeshGenerator.ConvertListsToMatrix_2xM( centerToExitRightFloor.centerL, centerToExitRightFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject centerToExitRightFloorGameObj = new GameObject( "Scambio Binario Centrale - Uscita Destra" );
                                                                                centerToExitRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                centerToExitRightFloorGameObj.transform.position = Vector3.zero;
                                                                                centerToExitRightFloorGameObj.AddComponent<MeshFilter>();
                                                                                centerToExitRightFloorGameObj.AddComponent<MeshRenderer>();
                                                                                centerToExitRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerToExitRightFloorMesh;
                                                                                centerToExitRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                                                            }
                                                                            
                                                                            break;

                                            case SwitchType.BiToNewBi:      MeshGenerator.Floor centerFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerLine, section.floorPoints.centerLine, centerWidth, false );
                                                                            Mesh centerFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.centerLine, MeshGenerator.ConvertListsToMatrix_2xM( centerFloor.centerL, centerFloor.centerR ), centerTextureTilting.x, centerTextureTilting.y );
                                                                            GameObject centerFloorGameObj = new GameObject( "Divisore centrale" );
                                                                            centerFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                            centerFloorGameObj.transform.position = Vector3.zero;
                                                                            centerFloorGameObj.AddComponent<MeshFilter>();
                                                                            centerFloorGameObj.AddComponent<MeshRenderer>();
                                                                            centerFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
                                                                            centerFloorGameObj.GetComponent<MeshRenderer>().material = centerTexture;

                                                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Left ) ) {
                                                                                MeshGenerator.Floor leftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, section.floorPoints.leftLine, tunnelWidth, false );
                                                                                Mesh leftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.leftLine, MeshGenerator.ConvertListsToMatrix_2xM( leftFloor.centerL, leftFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject leftFloorGameObj = new GameObject( "Scambio Binario Sinistro - Sinistro" );
                                                                                leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                leftFloorGameObj.transform.position = Vector3.zero;
                                                                                leftFloorGameObj.AddComponent<MeshFilter>();
                                                                                leftFloorGameObj.AddComponent<MeshRenderer>();
                                                                                leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                                                                leftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                                MeshGenerator.Floor leftToEntranceLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftEntranceLeft, section.floorPoints.leftEntranceLeft, tunnelWidth, false );
                                                                                Mesh leftToEntranceLeftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.leftEntranceLeft, MeshGenerator.ConvertListsToMatrix_2xM( leftToEntranceLeftFloor.centerL, leftToEntranceLeftFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject leftToEntranceLeftFloorGameObj = new GameObject( "Scambio Binario Sinistro - Ingresso Sinistro" );
                                                                                leftToEntranceLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                leftToEntranceLeftFloorGameObj.transform.position = Vector3.zero;
                                                                                leftToEntranceLeftFloorGameObj.AddComponent<MeshFilter>();
                                                                                leftToEntranceLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                                                leftToEntranceLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftToEntranceLeftFloorMesh;
                                                                                leftToEntranceLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                                MeshGenerator.Floor leftToExitLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftExitLeft, section.floorPoints.leftExitLeft, tunnelWidth, false );
                                                                                Mesh leftToExitLeftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.leftExitLeft, MeshGenerator.ConvertListsToMatrix_2xM( leftToExitLeftFloor.centerL, leftToExitLeftFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject leftToExitLeftFloorGameObj = new GameObject( "Scambio Binario Sinistro - Uscita Sinistra" );
                                                                                leftToExitLeftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                leftToExitLeftFloorGameObj.transform.position = Vector3.zero;
                                                                                leftToExitLeftFloorGameObj.AddComponent<MeshFilter>();
                                                                                leftToExitLeftFloorGameObj.AddComponent<MeshRenderer>();
                                                                                leftToExitLeftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftToExitLeftFloorMesh;
                                                                                leftToExitLeftFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                                                            }
                                                                            else {
                                                                                MeshGenerator.Floor leftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, section.floorPoints.leftLine, tunnelWidth, false );
                                                                                Mesh leftFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.leftLine, MeshGenerator.ConvertListsToMatrix_2xM( leftFloor.centerL, leftFloor.centerR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                                                                GameObject leftFloorGameObj = new GameObject( "Binario Sinistro" );
                                                                                leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                leftFloorGameObj.transform.position = Vector3.zero;
                                                                                leftFloorGameObj.AddComponent<MeshFilter>();
                                                                                leftFloorGameObj.AddComponent<MeshRenderer>();
                                                                                leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                                                                leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;
                                                                            }

                                                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Right ) ) {
                                                                                MeshGenerator.Floor rightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, section.floorPoints.rightLine, tunnelWidth, false );
                                                                                Mesh rightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.rightLine, MeshGenerator.ConvertListsToMatrix_2xM( rightFloor.centerL, rightFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject rightFloorGameObj = new GameObject( "Scambio Binario Destro - Destro" );
                                                                                rightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                rightFloorGameObj.transform.position = Vector3.zero;
                                                                                rightFloorGameObj.AddComponent<MeshFilter>();
                                                                                rightFloorGameObj.AddComponent<MeshRenderer>();
                                                                                rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
                                                                                rightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                                MeshGenerator.Floor rightToEntranceRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightEntranceRight, section.floorPoints.rightEntranceRight, tunnelWidth, false );
                                                                                Mesh rightToEntranceRightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.rightEntranceRight, MeshGenerator.ConvertListsToMatrix_2xM( rightToEntranceRightFloor.centerL, rightToEntranceRightFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject rightToEntranceRightFloorGameObj = new GameObject( "Scambio Binario Destro - Ingresso Destro" );
                                                                                rightToEntranceRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                rightToEntranceRightFloorGameObj.transform.position = Vector3.zero;
                                                                                rightToEntranceRightFloorGameObj.AddComponent<MeshFilter>();
                                                                                rightToEntranceRightFloorGameObj.AddComponent<MeshRenderer>();
                                                                                rightToEntranceRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightToEntranceRightFloorMesh;
                                                                                rightToEntranceRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;

                                                                                MeshGenerator.Floor rightToExitRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightExitRight, section.floorPoints.rightExitRight, tunnelWidth, false );
                                                                                Mesh rightToExitRightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.rightExitRight, MeshGenerator.ConvertListsToMatrix_2xM( rightToExitRightFloor.centerL, rightToExitRightFloor.centerR ), switchRailTextureTilting.x, switchRailTextureTilting.y );
                                                                                GameObject rightToExitRightFloorGameObj = new GameObject( "Scambio Binario Destro - Uscita Destra" );
                                                                                rightToExitRightFloorGameObj.transform.parent = sectionGameObj.transform;
                                                                                rightToExitRightFloorGameObj.transform.position = Vector3.zero;
                                                                                rightToExitRightFloorGameObj.AddComponent<MeshFilter>();
                                                                                rightToExitRightFloorGameObj.AddComponent<MeshRenderer>();
                                                                                rightToExitRightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightToExitRightFloorMesh;
                                                                                rightToExitRightFloorGameObj.GetComponent<MeshRenderer>().material = switchRailTexture;
                                                                            }
                                                                            else {
                                                                                MeshGenerator.Floor rightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, section.floorPoints.rightLine, tunnelWidth, false );
                                                                                Mesh rightFloorMesh = MeshGenerator.GenerateFloorMesh( section.floorPoints.rightLine, MeshGenerator.ConvertListsToMatrix_2xM( rightFloor.centerL, rightFloor.centerR ), tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
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
                                        break;
                }
            }
        }
    }

    private Vector3 GenerateLine( string lineName, Direction previousLineOrientation, Direction lineOrientation, int lineLength, Vector3 startingPoint, Vector3 startingDir, bool generateNewLines, LineSection fromSection ) {

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
                Debug.Log( ">>>>>>>>>>> Proibited Areas: " + lineName );
                Debug.Log( ">>>>>>>>>>> Line Turn Distance: " + lineTurnDistance );
                for( int k = i - sectionsBeforeTurnCounter; k < i; k++ ) {
                    actualSections.Add( sections[ k ] );

                    Debug.Log( ">>>>>>>>>>> indice sezioni: " + k );
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
