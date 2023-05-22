using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullLineGenerator : MonoBehaviour
{


    public class NewLine{
        public Vector3 startingPoint { get; set; }
        public Vector3 startingDir { get; set; }
        public Direction mainDir { get; set; }
        public string previousLineName { get; set; }
        public int previousSectionIndex { get; set; }
        //public string previousPointIndex { get; set; }
    }

    private List<NewLine> newLines = new List<NewLine>();

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
    public float switchLenght = 100.0f;
    public Vector3 stationRotationCorrections = new Vector3( 90.0f, -90.0f, 90.0f );
    public GameObject station;
    public float stationLenght = 25.0f;
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
    public bool secondaryLines = false;
    public GameObject switchLight;
    public float switchLightDistance;
    public float switchLightHeight;
    public Vector3 switchLightRotation;
    public Dictionary<string, List<LineSection>> lineMap = new Dictionary<string, List<LineSection>>();
    private int lineCounter = 0;
    public Cell[ , ] map;

    public enum CellSide {
        Up,
        Right,
        Down,
        Left,
    }

    public enum EndedBy {
        OutOfBounds,
        OtherLine,
        Completed,
    }

    public int mapSize = 50;
    public float mainDirPercent = 0.7f; 
    public int linesNumber = 5;
    public int noGenNearBorders = 10;
    public int noGenNearLines = 2;
    

    public enum Direction{
        North,
        South,
        East,
        West,
    }

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    public void Generate(){
        /*InitializeMap();
        for( int i = 0; i < linesNumber; i++ ) {
            Direction startMainDir = CalculateNextLineDir();

            Debug.Log( "startMainDir: " + startMainDir );
            Vector3 startingWorldCoords = CalculateStartingCoords( startMainDir );
            if( startingWorldCoords != Vector3.zero) {
                GenerateMetro2( "Linea " + i, startMainDir, startingWorldCoords );
            }
        }*/
        GenerateMetro( "Linea 1", null );
        InstantiateTrain();
        InstantiateMainCamera();

        /*if( secondaryLines ) {
            for( int i = 0; i< newLines.Count; i++ ) {
                GenerateMetro( "Linea " + ( i + 2 ), newLines[ i ] );
                Debug.Log( "Linea " + ( i + 2 ) );
                Debug.Log( "startingPoint : " + newLines[ i ].startingPoint );
                Debug.Log( "startingDir : " + newLines[ i ].startingDir );
            }
        }*/
    }

    private Vector3 ConvertMatrixCoordsToWorldCoords( Vector2Int matrixCoords ) {
        return new Vector3( ( matrixCoords.x * cellSize ) + ( cellSize / 2 ), ( matrixCoords.y * cellSize ) + ( cellSize / 2 ), 0.0f );
    }

    private Vector2Int ConvertWorldCoordsToMatrixCoords( Vector3 worldCoords ) {
        return new Vector2Int( ( int )( worldCoords.x / cellSize ), ( int )( worldCoords.y / cellSize ) );
    }

    private Vector3 CalculateStartingCoords( Direction lineDir ) {
        Vector2Int actualCoords = Vector2Int.zero;
        Vector2Int startingCoords = Vector2Int.zero;

        int genStartingPointCounter = 0;

            GenStartingPoint:

        if( genStartingPointCounter > 10 ) {
            return Vector3.zero;
        }

        // In questo caso inizio la generazione dai bordi della mappa, in base alla lineDir
        HashSet<Cell> proibitedStarts = new HashSet<Cell>();
        switch( lineDir ) {
                case Direction.East:    actualCoords = new Vector2Int( 0, Random.Range( noGenNearBorders, mapSize - noGenNearBorders ) );
                                        for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.y + i < mapSize ) {        
                                                proibitedStarts.Add( this.map[ actualCoords.x, actualCoords.y + i ] );
                                            }
                                            if( actualCoords.y - i >= 0 ) {
                                                proibitedStarts.Add( this.map[ actualCoords.x, actualCoords.y - i ] );
                                            }
                                        }
                                        startingCoords = new Vector2Int( actualCoords.x + 1, actualCoords.y );
                                        break;

                case Direction.North:   actualCoords = new Vector2Int( Random.Range( noGenNearBorders, mapSize - noGenNearBorders ), 0 );
                                        for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.x + i < mapSize ) {        
                                                proibitedStarts.Add( this.map[ actualCoords.x + i, actualCoords.y ] );
                                            }
                                            if( actualCoords.x - i >= 0 ) {
                                                proibitedStarts.Add( this.map[ actualCoords.x - i, actualCoords.y ] );
                                            }
                                        }
                                        startingCoords = new Vector2Int( actualCoords.x, actualCoords.y + 1 );
                                        break;

                case Direction.South:   actualCoords = new Vector2Int( Random.Range( noGenNearBorders, mapSize - noGenNearBorders ), mapSize - 1 );
                                        for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.x + i < mapSize ) {        
                                                proibitedStarts.Add( this.map[ actualCoords.x + i, actualCoords.y ] );
                                            }
                                            if( actualCoords.x - i >= 0 ) {
                                                proibitedStarts.Add( this.map[ actualCoords.x - i, actualCoords.y ] );
                                            }
                                        }
                                        startingCoords = new Vector2Int( actualCoords.x, actualCoords.y - 1 );
                                        break;

                case Direction.West:    actualCoords = new Vector2Int( mapSize - 1, Random.Range( noGenNearBorders, mapSize - noGenNearBorders ) );
                                        for( int i = 0; i < noGenNearLines; i++ ) {
                                            if( actualCoords.y + i < mapSize ) {        
                                                proibitedStarts.Add( this.map[ actualCoords.x, actualCoords.y + i ] );
                                            }
                                            if( actualCoords.y - i >= 0 ) {
                                                proibitedStarts.Add( this.map[ actualCoords.x, actualCoords.y - i ] );
                                            }
                                        }
                                        startingCoords = new Vector2Int( actualCoords.x - 1, actualCoords.y );
                                        break;
        }
        if( this.map[ actualCoords.x, actualCoords.y ].content != Cell.Content.Empty || this.map[ actualCoords.x, actualCoords.y ].content == Cell.Content.Proibited ) {
            genStartingPointCounter++;
            goto GenStartingPoint;
        }
        foreach( Cell proibitedStart in proibitedStarts ) {
            if( proibitedStart.content == Cell.Content.Empty ) {
                proibitedStart.content = Cell.Content.Proibited;
            }
        }

        return ConvertMatrixCoordsToWorldCoords( startingCoords );
    }

    private Direction CalculateNextLineDir() {

        int proibitedCellUp = 0, proibitedCellDown = 0, proibitedCellLeft = 0, proibitedCellRight = 0;

        for( int i = 0; i < mapSize; i++ ) {
            if( map[ 0, i ].content == Cell.Content.Proibited ) {
                proibitedCellLeft++;
            }

            if( map[ i, 0 ].content == Cell.Content.Proibited ) {
                proibitedCellDown++;
            }

            if( map[ mapSize - 1, i ].content == Cell.Content.Proibited ) {
                proibitedCellRight++;
            }

            if( map[ i, mapSize - 1 ].content == Cell.Content.Proibited ) {
                proibitedCellUp++;
            }
        }

        Dictionary<int, Direction> availableDir = new Dictionary<int, Direction>();
        int min = Mathf.Min( new int[ 4 ]{ proibitedCellUp, proibitedCellDown, proibitedCellLeft, proibitedCellRight } );
        int k = 0;

        //LineDir dir = LineDir.Random;

        if( proibitedCellUp == min ) {
            availableDir.Add( k, Direction.South );
            k++;

            //dir = LineDir.South;
        }
        if( proibitedCellLeft == min ) {
            availableDir.Add( k, Direction.East );
            k++;

            //dir = LineDir.East;
        }
        if( proibitedCellDown == min ) {
            availableDir.Add( k, Direction.North );
            k++;

            //dir = LineDir.North;
        }
        if( proibitedCellRight == min ) {
            availableDir.Add( k, Direction.West );
            k++;

            //dir = LineDir.West;
        } 

        return availableDir[ Random.Range( 0, k ) ];
        //return dir;
    }

    private void InitializeMap() {
        Cell[ , ] matrix = new Cell[ mapSize, mapSize ];
            
        // Inizializzazione celle vuote
        for( int i = 0; i < mapSize; i++ ) {
            for( int j = 0; j < mapSize; j++ ) {
                Cell cell = new Cell();

                cell.content = Cell.Content.Empty;
                if( i == 0 || i == mapSize - 1 ) {
                    if( j < noGenNearBorders || j > mapSize - 1 - noGenNearBorders ) {
                        cell.content = Cell.Content.Proibited;
                    }
                }
                if( j == 0 || j == mapSize - 1) {
                    if( i < noGenNearBorders || i > mapSize - 1 - noGenNearBorders ) {
                        cell.content = Cell.Content.Proibited;
                    }
                }

                cell.coords = new Vector2Int( i, j );
                cell.spatialCoords = new Vector3( ( i * cellSize ) + ( cellSize / 2 ), ( j * cellSize ) + ( cellSize / 2 ), 0.0f );

                matrix[ i, j ] = cell;
            }    
        }

        this.map = matrix;
    }

    private void GenerateMetro2( string lineName, Direction mainDir, Vector3 startingPoint ) {
        
        GameObject lineGameObj = new GameObject( lineName );

        Vector3 startingDir = Vector3.zero;
        switch( mainDir ) {
            case Direction.East:    startingDir = Vector3.right;
                                    break;
            case Direction.North:   startingDir = Vector3.up;
                                    break;
            case Direction.South:   startingDir = -Vector3.up;
                                    break;
            case Direction.West:    startingDir = -Vector3.right;
                                    break;
        }

        Debug.Log( "mainDir: " + mainDir );

        for( int i = 0; i < sectionsNumber; i++ ) {

            string sectionName = "Sezione " + i;

            // Genero la lista delle sezioni della linea
            List<LineSection> sections = new List<LineSection>();
            if( lineMap.ContainsKey( lineName ) ) {
                sections = lineMap[ lineName ];
            }
            else {
                lineMap.Add( lineName, sections );
            }

            if( sections.Count > 0 ) {
                startingDir = sections[ sections.Count - 1 ].nextStartingDirections[ 0 ];
                startingPoint = sections[ sections.Count - 1 ].nextStartingPoints[ 0 ];
            }

            LineSection section = new LineSection();
            section.type = Type.Tunnel;

            if( i == 0 ) {
                section.bidirectional = startingBidirectional;
            }
            else {
                section.bidirectional = sections[ i - 1 ].bidirectional;
            }

            EndedBy endedBy = EndedBy.Completed;
            Vector2Int otherLineMatrixCoords = Vector2Int.zero;

            List<Vector3> controlPoints = GenerateControlPoints2( startingDir, startingPoint, distanceMultiplier, controlPointsNumber, tunnelStraightness );
            List<Vector3> controlPointsInsideMap = new List< Vector3>();
            foreach( Vector3 cp in controlPoints ) {
                Vector2Int matrixCoords = ConvertWorldCoordsToMatrixCoords( cp );
                Debug.Log( "matrixCoords: " + matrixCoords ); 
                if( matrixCoords.x >= 1 && matrixCoords.x < mapSize - 1 && matrixCoords.y >= 1 && matrixCoords.y < mapSize - 1 ) {
                    //if( ( map[ matrixCoords.x, matrixCoords.y ].content == Cell.Content.Line || map[ matrixCoords.x, matrixCoords.y ].content == Cell.Content.OutsideSwitch ) && map[ matrixCoords.x, matrixCoords.y ].lineName != lineName ) {
                        //endedBy = EndedBy.OtherLine;
                        //otherLineMatrixCoords = matrixCoords;
                        //break;
                    //}
                    //else {
                        controlPointsInsideMap.Add( cp );
                    //}
                }
                else {
                    endedBy = EndedBy.OutOfBounds;
                    break;
                }
            }
            if( controlPointsInsideMap.Count > 2 ) {
                List<Vector3> baseCurve = BezierCurveCalculator.CalculateBezierCurve( controlPointsInsideMap, baseBezierCurvePointsNumber );
                List<Vector3> fixedLenghtCurve = BezierCurveCalculator.RecalcultateCurveWithFixedLenght( baseCurve, baseCurve.Count );
                List<Vector3> limitedAngleCurve = BezierCurveCalculator.RecalcultateCurveWithLimitedAngle( fixedLenghtCurve, maxAngle, startingDir );
                foreach( Vector3 curvePoint in limitedAngleCurve ) {
                    Debug.Log( "curvePoint: " + curvePoint );
                    Vector2Int matrixCoords = ConvertWorldCoordsToMatrixCoords( curvePoint );
                    Debug.Log( "matrixCoords: " + matrixCoords ); 
                    if( matrixCoords.x >= 1 && matrixCoords.x < mapSize - 1 && matrixCoords.y >= 1 && matrixCoords.y < mapSize - 1 ) {
                        map[ matrixCoords.x, matrixCoords.y ].content = Cell.Content.Line;
                        map[ matrixCoords.x, matrixCoords.y ].lineName = lineName;
                    }
                }

                section.controlsPoints = controlPointsInsideMap;
                section.bezierCurveBase = baseCurve;
                section.bezierCurveFixedLenght = fixedLenghtCurve;
                section.bezierCurveLimitedAngle = limitedAngleCurve;
                section.curvePointsCount = limitedAngleCurve.Count;

                List<Vector3> nextStartingDirections = new List<Vector3>();
                nextStartingDirections.Add( limitedAngleCurve[ limitedAngleCurve.Count - 1 ] - limitedAngleCurve[ limitedAngleCurve.Count - 2 ] );
                section.nextStartingDirections = nextStartingDirections;

                Debug.Log( "section.nextStartingDirections " + i + " " + section.nextStartingDirections[ 0 ].x + ", " + section.nextStartingDirections[ 0 ].y + ", " + section.nextStartingDirections[ 0 ].z );

                List<Vector3> nextStartingPoints = new List<Vector3>();
                nextStartingPoints.Add( limitedAngleCurve[ limitedAngleCurve.Count - 1 ] );
                section.nextStartingPoints = nextStartingPoints;

                sections.Add( section );

                GameObject sectionGameObj = new GameObject( sectionName );
                sectionGameObj.transform.parent = lineGameObj.transform;
                sectionGameObj.transform.position = startingPoint;
            }

            if( endedBy == EndedBy.OutOfBounds ) {
                Debug.Log( "Stop Line Generation" );

                Vector3 finalDir = sections[ i - 1 ].controlsPoints[ sections[ i - 1 ].controlsPoints.Count - 1 ] - sections[ i - 1 ].controlsPoints[ sections[ i - 1 ].controlsPoints.Count - 2 ];

                float alpha = Vector3.SignedAngle( finalDir, Vector3.right, -Vector3.forward );

                if( alpha >= -45.0f && alpha < 45.0f ) {
                    mainDir = Direction.East;
                }
                else if( alpha >= 45.0f && alpha < 135.0f ) {
                    mainDir = Direction.North;
                }
                else if( alpha >= -135.0f && alpha < -45.0f ) {
                    mainDir = Direction.South;
                }
                else if( alpha >= 135.0f || alpha < -135.0f ) {
                    mainDir = Direction.West;
                }

                Vector2Int lastMatrixCoords = ConvertWorldCoordsToMatrixCoords( sections[ sections.Count - 1 ].bezierCurveLimitedAngle[ sections[ sections.Count - 1 ].bezierCurveLimitedAngle.Count - 1 ] );

                switch( mainDir ) {
                    case Direction.East:    for( int p = 0; p < noGenNearLines; p++ ) {
                                                if( lastMatrixCoords.y + p < mapSize ) {        
                                                    map[ mapSize - 1, lastMatrixCoords.y + p ].content = Cell.Content.Proibited;
                                                }
                                                if( lastMatrixCoords.y - p >= 0 ) {
                                                    map[ mapSize - 1, lastMatrixCoords.y - p ].content = Cell.Content.Proibited;
                                                }
                                            }
                                            break;

                    case Direction.North:   for( int p = 0; p < noGenNearLines; p++ ) {
                                                if( lastMatrixCoords.x + p < mapSize ) {        
                                                    map[ lastMatrixCoords.x + p, mapSize - 1 ].content = Cell.Content.Proibited;
                                                }
                                                if( lastMatrixCoords.x - p >= 0 ) {
                                                    map[ lastMatrixCoords.x - p, mapSize - 1 ].content = Cell.Content.Proibited;
                                                }
                                            }
                                            break;

                    case Direction.South:   for( int p = 0; p < noGenNearLines; p++ ) {
                                                if( lastMatrixCoords.x + p < mapSize ) {        
                                                    map[ lastMatrixCoords.x + p, 0 ].content = Cell.Content.Proibited;
                                                }
                                                if( lastMatrixCoords.x - p >= 0 ) {
                                                    map[ lastMatrixCoords.x - p, 0 ].content = Cell.Content.Proibited;
                                                }
                                            }
                                            break;

                    case Direction.West:    for( int p = 0; p < noGenNearLines; p++ ) {
                                                if( lastMatrixCoords.y + p < mapSize ) {        
                                                    map[ 0, lastMatrixCoords.y + p ].content = Cell.Content.Proibited;
                                                }
                                                if( lastMatrixCoords.y - p >= 0 ) {
                                                    map[ 0, lastMatrixCoords.y - p ].content = Cell.Content.Proibited;
                                                }
                                            }
                                            break;
                }

                break;
                
            }
            /*else if( endedBy == EndedBy.OtherLine ) {

                if( i > 0 ) {
                    map[ otherLineMatrixCoords.x, otherLineMatrixCoords.y ].content = Cell.Content.OutsideSwitch;

                    Vector2Int lastCellCoords = ConvertWorldCoordsToMatrixCoords( sections[ sections.Count - 1 ].bezierCurveLimitedAngle[ sections[ sections.Count - 1 ].bezierCurveLimitedAngle.Count - 1 ] );

                    if( map[ otherLineMatrixCoords.x, otherLineMatrixCoords.y ].switchtoCell != null ) {
                        map[ otherLineMatrixCoords.x, otherLineMatrixCoords.y ].switchtoCell.Add( lineName, map[ lastCellCoords.x, lastCellCoords.y ] );
                    }
                    else {
                        Dictionary<string, Cell> firstSwitchToCell = new Dictionary<string, Cell>();
                        firstSwitchToCell.Add( lineName, map[ lastCellCoords.x, lastCellCoords.y ] );
                        map[ otherLineMatrixCoords.x, otherLineMatrixCoords.y ].switchtoCell = firstSwitchToCell;
                    }
                }
                       

                break;
            }*/
        }

        lineCounter++;
    }

    private void GenerateMetro( string lineName, NewLine newLine ) {

        GameObject lineGameObj = new GameObject( lineName );

        Direction mainDir = ( Direction )Random.Range( 0, 4 );
        Vector3 startingPoint = gameObject.transform.position;
        Vector3 startingDir = Vector3.zero;
        if( newLine != null ) {
            startingPoint = newLine.startingPoint;
            startingDir = newLine.startingDir;
            mainDir = newLine.mainDir;
        }

        Debug.Log( "mainDir: " + mainDir );

        for( int i = 0; i < sectionsNumber; i++ ) {

            string sectionName = "Sezione " + i;

            // Genero la lista delle sezioni della linea
            List<LineSection> sections = new List<LineSection>();
            if( lineMap.ContainsKey( lineName ) ) {
                sections = lineMap[ lineName ];
            }
            else {
                lineMap.Add( lineName, sections );
            }

            if( sections.Count > 0 ) {
                startingDir =  sections[ sections.Count - 1 ].nextStartingDirections[ 0 ];
                startingPoint = sections[ sections.Count - 1 ].nextStartingPoints[ 0 ];
            }

            GameObject sectionGameObj = new GameObject( sectionName );
            sectionGameObj.transform.parent = lineGameObj.transform;
            sectionGameObj.transform.position = startingPoint;

            LineSection section = new LineSection();
            if( i % stationsDistance == 0 && i > 0 ) {
                section.type = Type.Station;
                section.bidirectional = sections[ i - 1 ].bidirectional;

                List<Vector3> nextStartingDirections = new List<Vector3>();
                nextStartingDirections.Add( startingDir );
                section.nextStartingDirections = nextStartingDirections;

                List<Vector3> nextStartingPoints = new List<Vector3>();
                nextStartingPoints.Add( startingPoint + ( startingDir.normalized * stationLenght ) );
                section.nextStartingPoints = nextStartingPoints;

                List<Vector3> stationsPoints = new List<Vector3>{ startingPoint, nextStartingPoints[ 0 ] };

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
            }
            else if( i % switchDistance == 0 && i > 0 ) {
                SwitchPath switchPath = SwitchPath.CreateInstance( switchLenght, centerWidth, tunnelWidth, switchLightDistance, switchLightHeight, baseBezierCurvePointsNumber, switchLightRotation, switchLight );

                bool previousBidirectional = sections[ i - 1 ].bidirectional;

                if( previousBidirectional ) {
                    if( Random.Range( 0, 2 ) == 0 ) {
                        section = switchPath.generateBiToBiSwitch( i, sections, startingDir, startingPoint, sectionGameObj );
                    }
                    else{
                        section = switchPath.generateBiToMonoSwitch( i, sections, startingDir, startingPoint, sectionGameObj );
                    }
                }
                else {
                    if( Random.Range( 0, 2 ) == 0 || lineCounter > 0 ) {
                        section = switchPath.generateMonoToBiSwitch( i, sections, startingDir, startingPoint, sectionGameObj );
                    }
                    else{

                        section = switchPath.generateMonoToNewMonoSwitch( i, sections, startingDir, startingPoint, sectionGameObj );

                        NewLine newSwitchLine = new NewLine();
                        newSwitchLine.startingDir = section.nextStartingDirections[ 1 ];
                        newSwitchLine.startingPoint = section.nextStartingPoints[ 1 ];
                        newSwitchLine.previousLineName = lineName;
                        newSwitchLine.previousSectionIndex = i;

                        float alpha = Vector3.SignedAngle( newSwitchLine.startingDir, Vector3.right, -Vector3.forward );
                        if( alpha >= -45.0f && alpha < 45.0f ) {
                            newSwitchLine.mainDir = Direction.East;
                        }
                        else if( alpha >= 45.0f && alpha < 135.0f ) {
                            newSwitchLine.mainDir = Direction.North;
                        }
                        else if( alpha >= -135.0f && alpha < -45.0f ) {
                            newSwitchLine.mainDir = Direction.South;
                        }
                        else if( alpha >= 135.0f || alpha < -135.0f ) {
                            newSwitchLine.mainDir = Direction.West;
                        }

                        newLines.Add( newSwitchLine );
                    }
                }
            }
            else {
                section.type = Type.Tunnel;

                if( i == 0 ) {
                    section.bidirectional = startingBidirectional;
                }
                else {
                    section.bidirectional = sections[ i - 1 ].bidirectional;
                }

                List<Vector3> controlPoints = GenerateControlPoints( mainDir, startingDir, startingPoint, distanceMultiplier, controlPointsNumber, tunnelStraightness );
                List<Vector3> baseCurve = BezierCurveCalculator.CalculateBezierCurve( controlPoints, baseBezierCurvePointsNumber );
                List<Vector3> fixedLenghtCurve = BezierCurveCalculator.RecalcultateCurveWithFixedLenght( baseCurve, baseCurve.Count );
                List<Vector3> limitedAngleCurve = BezierCurveCalculator.RecalcultateCurveWithLimitedAngle( fixedLenghtCurve, maxAngle, startingDir );
                //Debug.Log( "# punti curva: " + limitedAngleCurve.Count );
                //Debug.Log( "Lunghezza curva: " + BezierCurveCalculator.CalculateBezierCurveLenght( limitedAngleCurve ) );

                MeshGenerator.Floor floorVertexPoints = new MeshGenerator.Floor();
                if( section.bidirectional ) {
                    Mesh leftFloorMesh = new Mesh();
                    Mesh centerFloorMesh = new Mesh();
                    Mesh rightFloorMesh = new Mesh();

                    floorVertexPoints = MeshGenerator.CalculateBidirectionalFloorMeshVertex( limitedAngleCurve, controlPoints, centerWidth, tunnelWidth, tunnelParabolic );

                    leftFloorMesh = MeshGenerator.GenerateFloorMesh( limitedAngleCurve, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.leftL, floorVertexPoints.leftR ), railTextureTilting.x, railTextureTilting.y );
                    GameObject leftFloorGameObj = new GameObject( "Binari sinistra" );
                    leftFloorGameObj.transform.parent = sectionGameObj.transform;
                    leftFloorGameObj.transform.position = Vector3.zero;
                    leftFloorGameObj.AddComponent<MeshFilter>();
                    leftFloorGameObj.AddComponent<MeshRenderer>();
                    leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = leftFloorMesh;
                    leftFloorGameObj.GetComponent<MeshRenderer>().material = railTexture;

                    centerFloorMesh = MeshGenerator.GenerateFloorMesh( limitedAngleCurve, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.centerL, floorVertexPoints.centerR ), centerTextureTilting.x, centerTextureTilting.y );
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

                    rightFloorMesh = MeshGenerator.GenerateFloorMesh( limitedAngleCurve, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.rightL, floorVertexPoints.rightR ), railTextureTilting.x, railTextureTilting.y );
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
                    Debug.Log( "Mono " + i );
                    Mesh floorMesh = new Mesh();

                    floorVertexPoints = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( limitedAngleCurve, controlPoints, tunnelWidth, tunnelParabolic );

                    floorMesh = MeshGenerator.GenerateFloorMesh( limitedAngleCurve, MeshGenerator.ConvertListsToMatrix_2xM( floorVertexPoints.centerL, floorVertexPoints.centerR ), railTextureTilting.x, railTextureTilting.y );

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
                section.controlsPoints = controlPoints;
                section.bezierCurveBase = baseCurve;
                section.bezierCurveFixedLenght = fixedLenghtCurve;
                section.bezierCurveLimitedAngle = limitedAngleCurve;
                section.floorPoints = floorVertexPoints;
                Debug.Log( "section.floorPoints.centerLine[ 0 ]: " + section.floorPoints.centerLine[ 0 ] );
                section.curvePointsCount = limitedAngleCurve.Count;

                int k = 0;
                foreach( Vector3 controlPoint in controlPoints ) {
                    string controlPointName = "CP " + k;

                    GameObject controlPointGameObj = new GameObject( controlPointName );
                    controlPointGameObj.transform.parent = sectionGameObj.transform;
                    controlPointGameObj.transform.position = controlPoint;
                    k++;
                }

                List<Vector3> nextStartingDirections = new List<Vector3>();
                nextStartingDirections.Add( limitedAngleCurve[ limitedAngleCurve.Count - 1 ] - limitedAngleCurve[ limitedAngleCurve.Count - 2 ] );
                section.nextStartingDirections = nextStartingDirections;

                Debug.Log( "section.nextStartingDirections " + i + " " + section.nextStartingDirections[ 0 ].x + ", " + section.nextStartingDirections[ 0 ].y + ", " + section.nextStartingDirections[ 0 ].z );

                List<Vector3> nextStartingPoints = new List<Vector3>();
                nextStartingPoints.Add( limitedAngleCurve[ limitedAngleCurve.Count - 1 ] );
                section.nextStartingPoints = nextStartingPoints;

                Debug.Log( "section.nextStartingPoints " + i + " " + section.nextStartingPoints[ 0 ].x + ", " + section.nextStartingPoints[ 0 ].y + ", " + section.nextStartingPoints[ 0 ].z );
            }

            sections.Add( section );
        }

        lineCounter++;
    }

    private void InstantiateTrain() {
        Vector3 trainPos = lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        GameObject instantiatedTrain = Instantiate( train, trainPos, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, trainDir, Vector3.forward ) ) );
        instantiatedTrain.name = "Train";
    }

    private void InstantiateMainCamera() {
        Vector3 trainPos = lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        Instantiate( mainCamera, trainPos, Quaternion.identity );
    }

    private List<Vector3> GenerateControlPoints2( Vector3 startingDir, Vector3 startingPoint, int pointsDistanceMultiplier, int pointsNumber, float straightness ) {
        List<Vector3> controlPoints = new List<Vector3>();
        controlPoints.Add( startingPoint );

        Direction mainDir = new Direction();
        startingDir = startingDir.normalized;
        if( startingDir == Vector3.zero ) {
            mainDir = ( Direction )Random.Range( 0, 4 );

            switch( mainDir ) {
                case Direction.East:    controlPoints.Add( startingPoint + ( Vector3.right * pointsDistanceMultiplier ) );
                                        break;
                case Direction.North:   controlPoints.Add( startingPoint + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * Vector3.right * pointsDistanceMultiplier ) );
                                        break;
                case Direction.South:   controlPoints.Add( startingPoint + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * Vector3.right * pointsDistanceMultiplier ) );
                                        break;
                case Direction.West:    controlPoints.Add( startingPoint + ( Quaternion.Euler( 0.0f, 0.0f, 180.0f ) * Vector3.right * pointsDistanceMultiplier ) );
                                        break;
            }
        }
        else {
            
            controlPoints.Add( startingPoint + ( startingDir * pointsDistanceMultiplier ) );

            float alpha = Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward );

            if( alpha >= -45.0f && alpha < 45.0f ) {
                mainDir = Direction.East;
            }
            else if( alpha >= 45.0f && alpha < 135.0f ) {
                mainDir = Direction.North;
            }
            else if( alpha >= -135.0f && alpha < -45.0f ) {
                mainDir = Direction.South;
            }
            else if( alpha >= 135.0f || alpha < -135.0f ) {
                mainDir = Direction.West;
            }
        }

        for( int i = 2; i < pointsNumber; i++ ) {

            float baseAngle = Random.Range( -45.0f + ( 45.0f * straightness ), 45.0f - ( 45.0f * straightness ) );

            switch( mainDir ) {
                case Direction.East:    controlPoints.Add( controlPoints[ i - 1 ] + ( Quaternion.Euler( 0.0f, 0.0f, baseAngle ) * Vector3.right * pointsDistanceMultiplier ) );
                                        break;
                case Direction.North:   controlPoints.Add( controlPoints[ i - 1 ] + ( Quaternion.Euler( 0.0f, 0.0f, baseAngle + 90.0f ) * Vector3.right * pointsDistanceMultiplier ) );
                                        break;
                case Direction.South:   controlPoints.Add( controlPoints[ i - 1 ] + ( Quaternion.Euler( 0.0f, 0.0f, baseAngle - 90.0f ) * Vector3.right * pointsDistanceMultiplier ) );
                                        break;
                case Direction.West:    controlPoints.Add( controlPoints[ i - 1 ] + ( Quaternion.Euler( 0.0f, 0.0f, baseAngle + 180.0f ) * Vector3.right * pointsDistanceMultiplier ) );
                                        break;
            }
        }
        
        return controlPoints;
    }

    private List<Vector3> GenerateControlPoints( Direction mainDir, Vector3 startingDir, Vector3 startingPoint, int pointsDistanceMultiplier, int pointsNumber, float straightness ) {
        List<Vector3> controlPoints = new List<Vector3>();
        controlPoints.Add( startingPoint );
        Vector3 furthermostPoint;

        Vector2 range = new Vector2( -90.0f, 90.0f );
        range.x += 90.0f * straightness;
        range.y -= 90.0f * straightness;
        float angle = Random.Range( range.x, range.y );

        switch( mainDir ) {
            case Direction.East:    angle += 0.0f;
                                    break;

            case Direction.West:    angle += 180.0f;
                                    break;

            case Direction.North:   angle += 90.0f;
                                    break;

            case Direction.South:   angle -= 90.0f;    
                                    break;
        }

        Debug.Log( "mainDir:  " + mainDir + " - angle: " + angle );

        if( startingDir != Vector3.zero ) {
            controlPoints.Add( startingPoint + ( startingDir.normalized * pointsDistanceMultiplier ) );
        }
        else {
            Vector3 newDir = Quaternion.Euler( 0.0f, 0.0f, angle ) * Vector3.right;
            controlPoints.Add( startingPoint + ( newDir.normalized * pointsDistanceMultiplier ) );
        }
        
        furthermostPoint = controlPoints[ 1 ];
        if( furthermostPoint.x < controlPoints[ 0 ].x ) {
            furthermostPoint = new Vector3( controlPoints[ 0 ].x, furthermostPoint.y, furthermostPoint.z );
        }

        for( int i = 2; i < pointsNumber; i++ ) { 
            range = new Vector2( -90.0f, 90.0f );
            range.x += 90.0f * straightness;
            range.y -= 90.0f * straightness;
            angle = Random.Range( range.x, range.y );

            Vector3 newDir = Quaternion.Euler( 0.0f, 0.0f, angle ) * Vector3.right;

            furthermostPoint = furthermostPoint + ( newDir.normalized * pointsDistanceMultiplier );
            controlPoints.Add( furthermostPoint );
        }
        
        return controlPoints;
    }

    private void OnDrawGizmos()
    {
        foreach( string line in lineMap.Keys ) {
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere( lineMap[ line ][ 0 ].bezierCurveLimitedAngle[ 0 ], 100.0f );

            foreach( LineSection segment in lineMap[ line ] ) {

                if( segment.type == Type.Tunnel ) {
                    for( int i = 0; i < segment.controlsPoints.Count; i++ ) {
                        
                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube( segment.controlsPoints[ i ], Vector3.one );

                        if( i > 0 ) {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine( segment.controlsPoints[ i - 1 ], segment.controlsPoints[ i ] );
                        }
                    }

                    for( int i = 0; i < segment.bezierCurveLimitedAngle.Count; i++ ) {
                        
                        if( i > 0 ) {
                            //Gizmos.color = Color.cyan;
                            //Gizmos.DrawLine( segment.bezierCurveFixedLenght[ i - 1 ], segment.bezierCurveFixedLenght[ i ] );
                        
                            //Gizmos.color = Color.yellow; 
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ i - 1 ], segment.bezierCurveLimitedAngle[ i ] );

                            //if( i % ( int )( baseBezierCurvePointsNumber * 0.1f ) == 0 ) {
                                //Gizmos.color = Color.magenta;
                                //Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ i ], segment.bezierCurveFixedLenght[ i ] );
                            //}

                            //Gizmos.color = Color.green;
                            Gizmos.DrawWireSphere( segment.bezierCurveLimitedAngle[ i ], 0.5f );
                        }
                    }
                }
                else if(  segment.type == Type.Switch ) {
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
                    else if( segment.switchType == SwitchType.MonoToNewMono ) {
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
                        for( int i = 0; i < segment.floorPoints.rightCenterNewLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.CenterToNewLineBackward ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightCenterNewLine[ i - 1 ], segment.floorPoints.rightCenterNewLine[ i ] );
                            }
                        }
                        for( int i = 0; i < segment.floorPoints.leftCenterNewLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.CenterToNewLineForward ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftCenterNewLine[ i - 1 ], segment.floorPoints.leftCenterNewLine[ i ] );
                            }
                        }
                    }
                }

                if( map != null ) {

                    for( int i = 0; i < mapSize; i++ ) {
                        for( int j = 0; j < mapSize; j++ ) {
                            Cell cell = this.map[ i, j ];
                            Vector3 center = new Vector3( cell.spatialCoords.x, cell.spatialCoords.y, 0.0f );
                            Vector3 a, b, c, d;
                            a = center + ( Vector3.up * cellSize / 2 ) - ( Vector3.right * cellSize / 2 );
                            b = a + ( Vector3.right * cellSize );
                            c = b - ( Vector3.up * cellSize );
                            d = c - ( Vector3.right * cellSize );

                            Gizmos.color = Color.grey;
                            Gizmos.DrawLine( a, b );
                            Gizmos.DrawLine( b, c );
                            Gizmos.DrawLine( c, d );
                            Gizmos.DrawLine( d, a );

                            if( cell.previousCell != null ) {
                                Gizmos.color = Color.cyan;
                                Gizmos.DrawLine( center, new Vector3( cell.previousCell.spatialCoords.x, cell.previousCell.spatialCoords.y, 0.0f ) );
                            }

                            /*if( cell.content == Cell.Content.OutsideSwitch ) {
                                Gizmos.color = Color.blue;
                                foreach( Cell newLineCell in cell.newLineCells ) {
                                    Gizmos.DrawLine( center, new Vector3( newLineCell.spatialCoords.x, newLineCell.spatialCoords.y, 0.0f ) );
                                }
                            }*/

                            if( cell.content == Cell.Content.Proibited ) {
                                Gizmos.color = Color.red;
                                Gizmos.DrawLine( a, c );
                                Gizmos.DrawLine( b, d );
                            }

                            //if( cell.content == Cell.Content.Line ) {
                                //Gizmos.color = Color.yellow;
                                //Gizmos.DrawLine( a, c );
                                //Gizmos.DrawLine( b, d );
                            //}

                            if( cell.content == Cell.Content.OutsideSwitch ) {
                                Gizmos.color = Color.magenta;
                                Gizmos.DrawLine( a, c );
                                Gizmos.DrawLine( b, d );
                                if( cell.switchtoCell != null ) {
                                    foreach( string lineName in cell.switchtoCell.Keys ) {
                                        Gizmos.DrawLine( cell.spatialCoords, cell.switchtoCell[ lineName ].spatialCoords );
                                    }
                                }
                            }
                        }    
                    }
                }


            }
        }
    }   
}
