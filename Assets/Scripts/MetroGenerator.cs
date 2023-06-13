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
    //public float fixedLenght = 5.0f;
    public int stationsDistance = 3;
    public int switchDistance = 5;
    public float stationLenght = 200.0f;
    public float switchLenght = 25.0f;
    public GameObject pillar;
    public int baseBezierCurvePointsNumber = 50;
    public bool tunnelParabolic = false;
    public float tunnelWidth = 5.0f;
    public float centerWidth = 5.0f;
    public float tunnelStraightness = 0.5f;
    public Material railTexture;
    public Vector2 railTextureTilting;
    public Material centerTexture;
    public Vector2 centerTextureTilting;
    public bool startingBidirectional = true;
    public Dictionary<string, List<LineSection>> lines = new Dictionary<string, List<LineSection>>();
    public Dictionary<string, List<List<Vector3>>> proibitedAreas = new Dictionary<string, List<List<Vector3>>>();
    private int lineCounter = 0;
    public int lineNumber = 1;
    public int lineTurnDistance = 5;
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
        
        //while( lineCounter < lineNumber ) {
            //Debug.Log( GenerateLine( "Linea " + lineCounter, lineOrientation, startPos, startDir ) );
        //}
        //InstantiateTrain();
        //InstantiateMainCamera();
    }

    private void GenerateMeshes() {

        foreach( string lineName in this.lines.Keys ) {
            
            GameObject lineGameObj = new GameObject( lineName );

            //foreach( LineSection section in this.lines[ lineName ] ) {
            for( int i = 0; i < this.lines[ lineName ].Count; i++ ) { 
                
                LineSection section = this.lines[ lineName ][ i ];

                if( i == 0 ) {

                    Debug.Log( "Nome linea: " + lineName + " - fromSection.switchType: " + ( section.fromSection != null ? section.fromSection.switchType.ToString() : "null" ) );

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

                GameObject sectionGameObj = new GameObject( sectionName );
                sectionGameObj.transform.parent = lineGameObj.transform;
                sectionGameObj.transform.position = section.bezierCurveLimitedAngle[ 0 ];

                switch( section.type ) {
                    case Type.Tunnel:   MeshGenerator.Floor floorVertexPoints = new MeshGenerator.Floor();
                                        if( section.bidirectional ) {
                                            Mesh leftFloorMesh = new Mesh();
                                            Mesh centerFloorMesh = new Mesh();
                                            Mesh rightFloorMesh = new Mesh();

                                            floorVertexPoints = MeshGenerator.CalculateBidirectionalFloorMeshVertex( section.bezierCurveLimitedAngle, section.controlsPoints, centerWidth, tunnelWidth, tunnelParabolic );

                                            leftFloorMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.leftL, floorVertexPoints.leftR ), railTextureTilting.x, railTextureTilting.y );
                                            GameObject leftFloorGameObj = new GameObject( "Binari sinistra" );
                                            leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                            leftFloorGameObj.transform.position = Vector3.zero;
                                            leftFloorGameObj.AddComponent<MeshFilter>();
                                            leftFloorGameObj.AddComponent<MeshRenderer>();
                                            leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                            leftFloorGameObj.GetComponent<MeshRenderer>().material = railTexture;

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

                                            rightFloorMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.rightL, floorVertexPoints.rightR ), railTextureTilting.x, railTextureTilting.y );
                                            GameObject rightFloorGameObj = new GameObject( "Binari destra" );
                                            rightFloorGameObj.transform.parent = sectionGameObj.transform;
                                            rightFloorGameObj.transform.position = Vector3.zero;
                                            rightFloorGameObj.AddComponent<MeshFilter>();
                                            rightFloorGameObj.AddComponent<MeshRenderer>();
                                            rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
                                            rightFloorGameObj.GetComponent<MeshRenderer>().material = railTexture;

                                            section.bidirectional = true;
                                        }
                                        else {
                                            Mesh floorMesh = new Mesh();

                                            floorVertexPoints = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.bezierCurveLimitedAngle, section.controlsPoints, tunnelWidth, tunnelParabolic );

                                            floorMesh = MeshGenerator.GenerateFloorMesh( section.bezierCurveLimitedAngle, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.centerL, floorVertexPoints.centerR ), railTextureTilting.x, railTextureTilting.y );

                                            GameObject floorGameObj = new GameObject( "Binari centrali" );
                                            floorGameObj.transform.parent = sectionGameObj.transform;
                                            floorGameObj.transform.position = Vector3.zero;
                                            floorGameObj.AddComponent<MeshFilter>();
                                            floorGameObj.AddComponent<MeshRenderer>();
                                            floorGameObj.GetComponent<MeshFilter>().sharedMesh = floorMesh;
                                            floorGameObj.GetComponent<MeshRenderer>().material = railTexture;

                                            section.bidirectional = false;
                                        }

                                        // Update dettagli LineSection 
                                        section.floorPoints = floorVertexPoints;
                                        break;

                    case Type.Station:  List<Vector3> stationsPoints = section.bezierCurveLimitedAngle;

                                        MeshGenerator.Floor stationRails = new MeshGenerator.Floor();

                                        if( section.bidirectional ) {
                                            stationRails = MeshGenerator.CalculateBidirectionalFloorMeshVertex( stationsPoints, stationsPoints, centerWidth, tunnelWidth, tunnelParabolic );

                                            Mesh leftFloorMesh = new Mesh();
                                            Mesh centerFloorMesh = new Mesh();
                                            Mesh rightFloorMesh = new Mesh();

                                            leftFloorMesh = MeshGenerator.GenerateFloorMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.leftL, stationRails.leftR ), railTextureTilting.x, railTextureTilting.y );
                                            GameObject leftFloorGameObj = new GameObject( "Binari sinistra" );
                                            leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                            leftFloorGameObj.transform.position = Vector3.zero;
                                            leftFloorGameObj.AddComponent<MeshFilter>();
                                            leftFloorGameObj.AddComponent<MeshRenderer>();
                                            leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                                            leftFloorGameObj.GetComponent<MeshRenderer>().material = railTexture;

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

                                            rightFloorMesh = MeshGenerator.GenerateFloorMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.rightL, stationRails.rightR ), railTextureTilting.x, railTextureTilting.y );
                                            GameObject rightFloorGameObj = new GameObject( "Binari destra" );
                                            rightFloorGameObj.transform.parent = sectionGameObj.transform;
                                            rightFloorGameObj.transform.position = Vector3.zero;
                                            rightFloorGameObj.AddComponent<MeshFilter>();
                                            rightFloorGameObj.AddComponent<MeshRenderer>();
                                            rightFloorGameObj.GetComponent<MeshFilter>().sharedMesh = rightFloorMesh;
                                            rightFloorGameObj.GetComponent<MeshRenderer>().material = railTexture;
                                        }
                                        else {
                                            stationRails = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( stationsPoints, stationsPoints, tunnelWidth, tunnelParabolic );

                                            Mesh centerFloorMesh = new Mesh();

                                            centerFloorMesh = MeshGenerator.GenerateFloorMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.centerL, stationRails.centerR ), railTextureTilting.x, railTextureTilting.y );
                                            GameObject leftFloorGameObj = new GameObject( "Binari centrali" );
                                            leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                            leftFloorGameObj.transform.position = Vector3.zero;
                                            leftFloorGameObj.AddComponent<MeshFilter>();
                                            leftFloorGameObj.AddComponent<MeshRenderer>();
                                            leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
                                            leftFloorGameObj.GetComponent<MeshRenderer>().material = railTexture;
                                        }

                                        section.controlsPoints = stationsPoints;
                                        section.floorPoints = stationRails;
                                        section.curvePointsCount = stationRails.centerLine.Count;
                    
                                        break;

                    case Type.Switch:   SwitchPath switchPath = SwitchPath.CreateInstance( switchLenght, switchBracketsLenght, centerWidth, tunnelWidth, switchLightDistance, switchLightHeight, baseBezierCurvePointsNumber, switchLightRotation, switchLight );

                                        bool previousBidirectional = this.lines[ lineName ][ i - 1 ].bidirectional;

                                        LineSection switchSection = new LineSection();

                                        Debug.Log( "previousBidirectional: " + previousBidirectional );

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
                                        
                                        break;
                }
            }
        }
    }

    private Vector3 GenerateLine( string lineName, Direction previousLineOrientation, Direction lineOrientation, int lineLength, Vector3 startingPoint, Vector3 startingDir, bool generateNewLines, LineSection fromSection ) {

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

        for( int i = 0; i < lineLength; i++ ) {
            
            if( i % lineTurnDistance == 0 ) {
                
                if( sections.Count != 0 ) {
                    List<LineSection> actualSections = new List<LineSection>();
                    for( int k = i - lineTurnDistance; k < i; k++ ) {
                        actualSections.Add( sections[ k ] );
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
                }
            }

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

                    nextStartingPoints.Add( startingPoint + ( startingDir.normalized * stationLenght ) );
                    section.nextStartingPoints = nextStartingPoints;
                }
                else if( i % switchDistance == 0 ) { 
                    section.type = Type.Switch;

                    nextStartingPoints.Add( startingPoint + ( startingDir.normalized * switchLenght ) );
                    section.nextStartingPoints = nextStartingPoints;
                }

                List<Vector3> curvePoints = new List<Vector3>{ startingPoint, nextStartingPoints[ 0 ] };
                section.controlsPoints = curvePoints;
                section.bezierCurveLimitedAngle = curvePoints;
                section.curvePointsCount = 2;
                
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

            if( i == lineLength - 1 ) {
                List<LineSection> lastSections = new List<LineSection>();
                for( int k = i - ( i % lineTurnDistance ); k <= ( lineLength - 1 ); k++ ) {
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
