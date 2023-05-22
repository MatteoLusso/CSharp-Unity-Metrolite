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
    
    public enum Direction{
        North,
        South,
        East,
        West,
        Random,
        None,
    }

    public class LineStart{
        public Direction previousOrientation { get; set; }
        public Direction orientation { get; set; }
        public Vector3 dir { get; set; }
        public Vector3 pos { get; set; }
        public bool generated { get; set; }

        public LineStart( Direction inputOrientation, Vector3 inputPos, Vector3 inputDir ) {
            this.generated = false;
            this.orientation = inputOrientation;
            this.pos = inputPos;
            this.dir = inputDir;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    public void Generate() {

        GenerateLine( "Linea " + lineCounter, Direction.None, Direction.Random, sectionsNumber, Vector3.zero, Vector3.zero, true );

        foreach( LineSection section in lines[ "Linea " + ( lineCounter - 1 ) ] ) {
            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                foreach( LineStart lineStart in section.newLinesStarts ) {
                    if( !lineStart.generated ){
                        GenerateLine( "Linea " + lineCounter, lineStart.previousOrientation, lineStart.orientation, sectionsNumber/ 2, lineStart.pos, lineStart.dir, false );
                        lineStart.generated = true;
                    }
                }

                foreach( LineStart lineStart in section.newLinesStarts ) {
                    if( !lineStart.generated ){
                        GenerateLine( "Linea " + lineCounter, lineStart.previousOrientation, lineStart.orientation, sectionsNumber/ 4, lineStart.pos, lineStart.dir, false );
                        lineStart.generated = true;
                    }
                }
            }
        }
        
        //while( lineCounter < lineNumber ) {
            //Debug.Log( GenerateLine( "Linea " + lineCounter, lineOrientation, startPos, startDir ) );
        //}
        //InstantiateTrain();
        //InstantiateMainCamera();
    }

    private Vector3 GenerateLine( string lineName, Direction previousLineOrientation, Direction lineOrientation, int lineLength, Vector3 startingPoint, Vector3 startingDir, bool mainLine ) {

        GameObject lineGameObj = new GameObject( lineName );
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

            GameObject sectionGameObj = new GameObject( sectionName );
            sectionGameObj.transform.parent = lineGameObj.transform;
            sectionGameObj.transform.position = startingPoint;

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

                if( i == 0 ) {
                    section.bidirectional = startingBidirectional;
                }
                else {
                    section.bidirectional = sections[ i - 1 ].bidirectional;
                }

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

                int k = 0;
                foreach( Vector3 controlPoint in controlPoints ) {
                    string controlPointName = "CP " + k;

                    GameObject controlPointGameObj = new GameObject( controlPointName );
                    controlPointGameObj.transform.parent = sectionGameObj.transform;
                    controlPointGameObj.transform.position = controlPoint;
                    k++;
                }

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
            if( newLineFromSwitch ) {
                foreach( LineSection finalSection in sections ) {
                    if( finalSection.type == Type.Switch ) {
                        Vector3 finalDir = ( finalSection.nextStartingPoints[ 0 ] - finalSection.bezierCurveLimitedAngle[ 0 ] );
                        Vector3 switchCenter = finalSection.bezierCurveLimitedAngle[ 0 ] + ( finalDir / 2 );
                        Vector3 switchLeft = switchCenter + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * finalDir.normalized * switchBracketsLenght;
                        Vector3 switchRight = switchCenter + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * finalDir.normalized * switchBracketsLenght;

                        bool leftAvailable = !isPointInsideProibitedArea( switchLeft, null );
                        bool rightAvailable = !isPointInsideProibitedArea( switchRight, null );

                        List<LineStart> newLinesStarts = new List<LineStart>();
                        switch( finalSection.orientation ) {
                            case Direction.East:    if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.North, switchLeft, switchLeft - switchCenter );
                                                        newLinesStarts.Add( newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.South, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( newLineStart );
                                                    }
                                                    break;

                            case Direction.West:    if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.South, switchLeft, switchLeft - switchCenter );
                                                        newLinesStarts.Add( newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.North, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( newLineStart );
                                                    }
                                                    break;

                            case Direction.North:   if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.West, switchLeft, switchLeft - switchCenter );
                                                        newLinesStarts.Add( newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.East, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( newLineStart );
                                                    }
                                                    break;

                            case Direction.South:   if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.East, switchLeft, switchLeft - switchCenter );
                                                        newLinesStarts.Add( newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( Direction.West, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( newLineStart );
                                                    }   
                                                    break;
                        }
                        finalSection.newLinesStarts = newLinesStarts;

                        if( newLinesStarts.Count > 0 ) {
                            Debug.Log( "Section Orientation: " + finalSection.orientation );
                            foreach( LineStart start in finalSection.newLinesStarts ) {
                                Debug.Log( "New Line Starting Orientation: " + start.orientation );
                            }
                        }
                    }
                }
            }
        }

        lineCounter++;

        return sections[ ^1 ].bezierCurveLimitedAngle[ ^1 ];
    }

    private bool isPointInsideProibitedArea( Vector3 point, string excludedLine ) {
        foreach( string lineName in this.proibitedAreas.Keys ) {
            
            if( excludedLine != null && excludedLine != lineName ) {
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
        Vector3 trainPos = lines[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lines[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lines[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        GameObject instantiatedTrain = Instantiate( train, trainPos, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, trainDir, Vector3.forward ) ) );
        instantiatedTrain.name = "Train";
    }

    private void InstantiateMainCamera() {
        Vector3 trainPos = lines[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lines[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lines[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
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
                    foreach( LineStart newLineStart in segment.newLinesStarts ) {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawWireCube( newLineStart.pos, Vector3.one * 25 );
                        Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ 0 ], newLineStart.pos );
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
}
