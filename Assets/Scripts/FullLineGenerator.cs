using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullLineGenerator : MonoBehaviour
{
    public GameObject train;
    public float trainHeightFromGround = 1.5f;
    public int controlPointsNumber = 3;
    public int distanceMultiplier = 25;
    public int sectionsNumber = 1;
    public float maxAngle = 2.5f;
    //public float fixedLenght = 5.0f;
    public int stationsDistance = 3;
    public Vector3 stationRotationCorrections = new Vector3( 90.0f, -90.0f, 90.0f );
    public GameObject station;
    public int baseBezierCurvePointsNumber = 50;
    public bool tunnelParabolic = false;
    public float tunnelWidth = 5.0f;
    public float tunnelStraightness = 0.5f;
    public Material texture;

    public Dictionary<string, List<LineSection>> lineMap = new Dictionary<string, List<LineSection>>();

    private enum Direction{
        North,
        South,
        East,
        West,
    }

    // Start is called before the first frame update
    void Start()
    {
        string lineName = "Linea " + "1";
        GameObject lineGameObj = new GameObject( lineName );

        Direction lineMainDir = ( Direction )Random.Range( 0, 3 );

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

            Vector3 startingPoint = gameObject.transform.position;
            Vector3 startingDir = Vector3.zero; // dipenderà dalla main direction della linea
            if( sections.Count > 0 ) {
                startingPoint = sections[ sections.Count - 1 ].nextStartingPoints[ 0 ]; //Per il momento gli starting point successivi possono essere solo 1
                startingDir =  sections[ sections.Count - 1 ].nextStartingDirections[ 0 ]; //Per il momento le starting direction successive possono essere solo 1
            }

            GameObject sectionGameObj = new GameObject( sectionName );
            sectionGameObj.transform.parent = lineGameObj.transform;
            sectionGameObj.transform.position = startingPoint;


            LineSection section = new LineSection();
            if( i % stationsDistance == 0 && i > 0) {
                section.type = Type.Station;

                GameObject newStation = Instantiate( station, startingPoint, station.transform.rotation );

                newStation.transform.localRotation *= Quaternion.Euler( Vector3.SignedAngle( Vector3.right, startingDir, newStation.transform.forward ) + stationRotationCorrections.x, stationRotationCorrections.y, stationRotationCorrections.z );

                List<Vector3> nextStartingDirections = new List<Vector3>();
                nextStartingDirections.Add( startingDir );
                section.nextStartingDirections = nextStartingDirections;

                List<Vector3> nextStartingPoints = new List<Vector3>();
                nextStartingPoints.Add( newStation.transform.Find( "End" ).transform.position );
                section.nextStartingPoints = nextStartingPoints;

                newStation.name = "Stazione";
                newStation.transform.parent = sectionGameObj.transform;

                section.bezierCurveLimitedAngle = new List<Vector3>{ startingPoint, newStation.transform.Find( "End" ).transform.position };
            }
            else {
                section.type = Type.Tunnel;

                List<Vector3> controlPoints = GenerateControlPoints( lineMainDir, startingDir, startingPoint, distanceMultiplier, controlPointsNumber, tunnelStraightness );
                List<Vector3> baseCurve = CalculateBaseBezierCurve( controlPoints );
                List<Vector3> fixedLenghtCurve = BezierCurveCalculator.RecalcultateCurveWithFixedLenght( baseCurve, baseCurve.Count );
                List<Vector3> limitedAngleCurve = BezierCurveCalculator.RecalcultateCurveWithLimitedAngle( fixedLenghtCurve, maxAngle, startingDir );

                //Debug.Log( "# punti curva: " + limitedAngleCurve.Count );
                //Debug.Log( "Lunghezza curva: " + BezierCurveCalculator.CalculateBezierCurveLenght( limitedAngleCurve ) );

                List<List<Vector3>> rightAndLeftVertexPoints = CalculateFloorMeshVertex( limitedAngleCurve, controlPoints );
                Mesh floorMesh = GenerateFloorMesh( limitedAngleCurve, ConvertListsToMatrix_2xM( rightAndLeftVertexPoints[ 0 ], rightAndLeftVertexPoints[ 1 ] ) );

                // Update dettagli LineSection 
                section.controlsPoints = controlPoints;
                section.bezierCurveBase = baseCurve;
                section.bezierCurveFixedLenght = fixedLenghtCurve;
                section.bezierCurveLimitedAngle = limitedAngleCurve;
                section.floorRightPoints = rightAndLeftVertexPoints[ 0 ];
                section.floorLeftPoints = rightAndLeftVertexPoints[ 1 ];
                section.floorMesh = floorMesh;

                GameObject floorGameObj = new GameObject( "Binari" );
                floorGameObj.transform.parent = sectionGameObj.transform;
                floorGameObj.transform.position = Vector3.zero;
                floorGameObj.AddComponent<MeshFilter>();
                floorGameObj.AddComponent<MeshRenderer>();
                floorGameObj.GetComponent<MeshFilter>().sharedMesh = floorMesh;
                floorGameObj.GetComponent<MeshRenderer>().material = texture;

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

                List<Vector3> nextStartingPoints = new List<Vector3>();
                nextStartingPoints.Add( limitedAngleCurve[ limitedAngleCurve.Count - 1 ] );
                section.nextStartingPoints = nextStartingPoints;
            }

            sections.Add( section );
        }

        InstantiateTrain();
    }

    private void InstantiateTrain() {
        Vector3 trainPos = lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lineMap[ "Linea 1" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        Instantiate( train, trainPos, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, trainDir, Vector3.forward ) ) );
    }

    private Vector3[,] ConvertListsToMatrix_2xM( List<Vector3> up, List<Vector3> down )
    {
        if( up.Count == down.Count )
        {
            Vector3[ , ] VertexMatrix = new Vector3[ up.Count, down.Count ];

            for( int row = 0; row < 2; row++ )
            {
               for( int col = 0; col < up.Count; col++ )
               {
                    if( row == 0 )
                    {
                        VertexMatrix[ row, col ] = up[ col ];
                    }
                    else
                    {
                        VertexMatrix[ row, col ] = down[ col ];
                    }
               } 
            }

            return VertexMatrix;

        }
        else
        {
            return null;
        }
    }

    private List<Vector3> CalculateBaseBezierCurve( List<Vector3> controlsPoints )
    {
        List<Vector3> baseBezierCurve = new List<Vector3>();

        for(int k = 0; k < baseBezierCurvePointsNumber; k++)
        {
            float t = k / ( float )( baseBezierCurvePointsNumber - 1 );
            Vector3 newBezierCurvePoint = BezierCurveCalculator.CalculateSingleBezierPoint( t, controlsPoints );
            baseBezierCurve.Add( newBezierCurvePoint );
        }

        return baseBezierCurve;
    }

    private List<Vector3> GenerateControlPoints( Direction mainDir, Vector3 startingDir, Vector3 startingPoint, int pointsDistanceMultiplier, int pointsNumber, float straightness ) {
        List<Vector3> controlPoints = new List<Vector3>();
        controlPoints.Add( startingPoint );
        Vector3 furthermostPoint;

        Vector2 range = new Vector2( 0.0f, 180.0f );
        range.x += 90.0f * straightness;
        range.y -= 90.0f * straightness;
        float angle = Random.Range( range.x, range.y );

        switch( mainDir ) {
            case Direction.East:    angle -= 90.0f;

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

                                        range = new Vector2( 0.0f, 180.0f );
                                        range.x += 90.0f * straightness;
                                        range.y -= 90.0f * straightness;
                                        angle = Random.Range( range.x, range.y ) - 90.0f;

                                        Vector3 newDir = Quaternion.Euler( 0.0f, 0.0f, angle ) * Vector3.right;

                                        furthermostPoint = furthermostPoint + ( newDir.normalized * pointsDistanceMultiplier );
                                        controlPoints.Add( furthermostPoint );
                                    } 
                                
                                    break;

            case Direction.West:    angle += 90.0f;
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
                                        range = new Vector2( 0.0f, 180.0f );
                                        range.x += 90.0f * straightness;
                                        range.y -= 90.0f * straightness;
                                        angle = Random.Range( range.x, range.y ) + 90.0f;

                                        Vector3 newDir = Quaternion.Euler( 0.0f, 0.0f, angle ) * Vector3.right;

                                        furthermostPoint = furthermostPoint + ( newDir.normalized * pointsDistanceMultiplier );
                                        controlPoints.Add( furthermostPoint );
                                    } 
                                
                                    break;

            case Direction.North:   angle += 0.0f;
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
                                        range = new Vector2( 0.0f, 180.0f );
                                        range.x += 90.0f * straightness;
                                        range.y -= 90.0f * straightness;
                                        angle = Random.Range( range.x, range.y ) + 0.0f;

                                        Vector3 newDir = Quaternion.Euler( 0.0f, 0.0f, angle ) * Vector3.right;

                                        furthermostPoint = furthermostPoint + ( newDir.normalized * pointsDistanceMultiplier );
                                        controlPoints.Add( furthermostPoint );
                                    } 
                                
                                    break;

            case Direction.South:   angle += 180.0f;
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
                                        range = new Vector2( 0.0f, 180.0f );
                                        range.x += 90.0f * straightness;
                                        range.y -= 90.0f * straightness;
                                        angle = Random.Range( range.x, range.y ) + 180.0f;

                                        Vector3 newDir = Quaternion.Euler( 0.0f, 0.0f, angle ) * Vector3.right;

                                        furthermostPoint = furthermostPoint + ( newDir.normalized * pointsDistanceMultiplier );
                                        controlPoints.Add( furthermostPoint );
                                    } 
                            
                                    break;
        }
        return controlPoints;
    }

    private List<List<Vector3>> CalculateFloorMeshVertex( List<Vector3> curve, List<Vector3> controlPoints )
    {

        List<Vector3> rightPoints = new List<Vector3>();
        List<Vector3> leftPoints = new List<Vector3>();

        float zHeightPrev = 0.0f;
        float zHeightNext = 0.0f;

        if( tunnelParabolic )
        {
            zHeightPrev = curve[ 0 ].z;
            zHeightNext = curve[ 1 ].z;
        }

        //Vector3 dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3(curve[ 0 ].x, curve[ 0 ].y, zHeightPrev) - new Vector3(curve[ 1 ].x, curve[ 1 ].y, zHeightNext ) ).normalized * tunnelWidth;
        Vector3 dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, zHeightNext ) ).normalized * tunnelWidth;

        leftPoints.Add( curve[ 0 ] + dir );
        rightPoints.Add(curve[ 0 ] - dir );

        for( int i = 1; i < curve.Count - 1; i++ )
        {
            if( tunnelParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }

            dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * tunnelWidth;

            leftPoints.Add( curve[ i ] + dir );
            rightPoints.Add(curve[ i ] - dir );
        }

        if( tunnelParabolic )
        {
            zHeightPrev = curve[ curve.Count - 2 ].z;
            zHeightNext = curve[ curve.Count - 1 ].z;
        }

        dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, zHeightPrev ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, zHeightNext ) ).normalized * tunnelWidth;
        leftPoints.Add( curve[ curve.Count - 1 ] + dir );
        rightPoints.Add( curve[ curve.Count - 1 ] - dir );

        List<List<Vector3>> rightAndLeftPoints = new List<List<Vector3>>();
        rightAndLeftPoints.Add( rightPoints ); // Indice 0 >>> destra
        rightAndLeftPoints.Add( leftPoints ); // Indice 1 >>> sinistra

        return rightAndLeftPoints;
    }

    private Mesh GenerateFloorMesh( List<Vector3> curve, Vector3[ , ] vertMatrix )
    {
        Mesh floorMesh = new Mesh();

        Vector3[] vertices = new Vector3[ curve.Count * 2 ];
        int[] edges = new int[( curve.Count - 1) * 6 ];
        Vector2[] uvs = new Vector2[ vertices.Length ];

        float curveLenght = BezierCurveCalculator.CalculateCurveLenght( curve );

        for( int i = 0; i < curve.Count; i++ )
        {
            vertices[ i * 2 ] = vertMatrix[ 0, i ];
            vertices[ ( i * 2 ) + 1 ] = vertMatrix[ 1, i ];

            float meshPercent = curveLenght * i;
            //float meshPercent = ( i / ( float )( vertices.Length - 1 ) );
            uvs[ ( i * 2 ) ] = new Vector2( meshPercent, 0 );
            uvs[ ( i * 2 ) + 1 ] = new Vector2( meshPercent, 1 );

            if( i < curve.Count - 1 )
            {
                edges[ ( i * 6 ) + 0 ] = ( i * 2 );
                edges[ ( i * 6 ) + 1 ] = edges[ ( i * 6 ) + 4 ] = ( i * 2 ) + 2;
                edges[ ( i * 6 ) + 2 ] = edges[ ( i * 6 ) + 3 ] = ( i * 2 ) + 1;
                edges[ ( i * 6 ) + 5 ] = ( i * 2 ) + 3;
            }
        }

        floorMesh.Clear();
        floorMesh.vertices = vertices;
        floorMesh.triangles = edges;
        floorMesh.uv = uvs;

        floorMesh.RecalculateNormals();

        return floorMesh;
    }

    private void OnDrawGizmos()
    {
        foreach( string line in lineMap.Keys ) {
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
                            Gizmos.color = Color.cyan;
                            Gizmos.DrawLine( segment.bezierCurveFixedLenght[ i - 1 ], segment.bezierCurveFixedLenght[ i ] );
                        
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ i - 1 ], segment.bezierCurveLimitedAngle[ i ] );

                            if( i % ( int )( baseBezierCurvePointsNumber * 0.1f ) == 0 ) {
                                Gizmos.color = Color.magenta;
                                Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ i ], segment.bezierCurveFixedLenght[ i ] );
                            }
                        }
                    }
                }
                Vector3 firstDir = segment.bezierCurveLimitedAngle[ 1 ] - segment.bezierCurveLimitedAngle[ 0 ];
                Vector3 lastDir = segment.bezierCurveLimitedAngle[ segment.bezierCurveLimitedAngle.Count - 1 ] - segment.bezierCurveLimitedAngle[ segment.bezierCurveLimitedAngle.Count - 2 ];
            
                Gizmos.color = Color.red;
                Gizmos.DrawRay( segment.bezierCurveLimitedAngle[ segment.bezierCurveLimitedAngle.Count - 1 ] - Vector3.forward, lastDir );
                Gizmos.DrawRay( segment.bezierCurveLimitedAngle[ 0 ] - Vector3.forward, -firstDir );
                Gizmos.DrawRay( segment.bezierCurveLimitedAngle[ segment.bezierCurveLimitedAngle.Count - 1 ] - Vector3.forward, Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * lastDir );
                Gizmos.DrawRay( segment.bezierCurveLimitedAngle[ segment.bezierCurveLimitedAngle.Count - 1 ] - Vector3.forward, Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * lastDir );


            }
        }
    }   
}
