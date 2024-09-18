using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshGenerator
{
    public class Floor {

        public List<Vector3> railLeftR { get; set; }
        public List<Vector3> railLeftL { get; set; }
        public List<Vector3> railCenterR { get; set; }
        public List<Vector3> railCenterL { get; set; }
        public List<Vector3> railRightR { get; set; }
        public List<Vector3> railRightL { get; set; }
        public List<Vector3> railSwitchLeftRightR { get; set; }
        public List<Vector3> railSwitchLeftRightL { get; set; }
        public List<Vector3> railSwitchRightLeftR { get; set; }
        public List<Vector3> railSwitchRightLeftL { get; set; }

        public List<Vector3> rightR { get; set; }
        public List<Vector3> rightLine { get; set; }
        public List<Vector3> rightL { get; set; }

        public List<Vector3> centerR { get; set; }
        public List<Vector3> centerLine { get; set; }
        public List<Vector3> centerL { get; set; }

        public List<Vector3> leftR { get; set; }
        public List<Vector3> leftLine { get; set; }
        public List<Vector3> leftL { get; set; }

        public List<Vector3> leftRightLine { get; set; }
        public List<Vector3> rightLeftLine { get; set; }

        public List<Vector3> rightCenterLine { get; set; }
        
        public List<Vector3> leftCenterLine { get; set; }

        public List<Vector3> centerEntranceRight { get; set; }
        public List<Vector3> centerExitRight { get; set; }
        public List<Vector3> centerEntranceLeft { get; set; }
        public List<Vector3> centerExitLeft { get; set; }

        public List<Vector3> rightEntranceRight { get; set; }
        public List<Vector3> rightExitRight { get; set; }
        public List<Vector3> leftEntranceLeft { get; set; }
        public List<Vector3> leftExitLeft { get; set; }

        public List<Vector3> switchNewBaseLine { get; set; }
        public int switchNewStartIndex { get; set; }
        public int switchNewEndIndex { get; set; }
        public Vector3 switchNewStartIntersection { get; set; }
        public Vector3 switchNewEndIntersection { get; set; }
        public Vector3 switchNewUpIntersection { get; set; }
    }

    public class PlatformSide {

        public List<Vector3> rightDown { get; set; }
        public List<Vector3> rightUp { get; set; }

        public List<Vector3> leftDown { get; set; }
        public List<Vector3> leftUp { get; set; }

        public List<Vector3> rightFloorLeft { get; set; }
        public List<Vector3> rightFloorRight { get; set; }

        public List<Vector3> leftFloorLeft { get; set; }
        public List<Vector3> leftFloorRight { get; set; }

    }

    public class ProceduralMesh{ 
         
        public Mesh mesh { get; set; }
        public List<Vector3> lastProfileVertex { get; set; }
        public GameObject gameObj { get; set; }
        public Dictionary<Orientation, Dictionary<int, List<Vector3>>> verticesStructure { get; set; }
        public Vector2 uvMax;
    }

    public class SpecularBaseLine {
        public List<Vector3> left { get; set; }
        public List<Vector3> right { get; set; }

    }

    public class Edge {

        public int startingPointIndex;
        public int endingPointIndex;
        public Vector3 dir;
        public Vector3 startingPoint;
        public Vector3 endingPoint;

        public Edge( Vector3 startingPoint, Vector3 endingPoint, int? startingPointIndex, int? endingPointIndex ) {
            this.startingPoint = startingPoint;
            this.endingPoint = endingPoint;

            this.dir = endingPoint - startingPoint;

            if( startingPointIndex != null && endingPointIndex != null ) {
                this.startingPointIndex = ( int )startingPointIndex;
                this.endingPointIndex = ( int )endingPointIndex;
            }
            
        }
    }

    public class Node {
        public List<Edge> meetingEdges = new();
        public List<Edge> leavingEdges = new();
        public List<Node> connectedNodes = new();
        public int index;
        public HashSet<int> connectedIndexes = new();
    }

    public class Trig {

        private HashSet<int> _nodesIndexes;
        public HashSet<int> nodesIndexes
        {
            get { 
                return this._nodesIndexes; 
            }
            set
            {                
                this._nodesIndexes = value;

                this.indexsSum = 0;
                foreach( int nodesIndex in this._nodesIndexes ) {
                    this.indexsSum += nodesIndex;
                }
            }
        }
        public int indexsSum;
        public List<Node> nodes;
    }

    public static ProceduralMesh GenerateStair( List<Vector3> front, List<Vector3> up, float stepPercent, bool clockwise, float rotationCorrection, Utils.Texturing texturing ) {

        int steps = Mathf.RoundToInt( 1 / stepPercent );
        stepPercent = 1.0f / ( float )steps;

        List<Vector3> back = new() { new Vector3( up[ 0 ].x, up[ 0 ].y, front[ 0 ].z ),
                                     new Vector3( up[ ^1 ].x, up[ ^1 ].y, front[ ^1 ].z ) };
                                     
        Vector3 horDir = back[ 0 ] - front[ 0 ]; 
        Vector3 vertDir = up[ 0 ] - back[ 0 ];

        float width = horDir.magnitude, height = vertDir.magnitude;

        List<Vector3> stairProfile = new() { Vector3.zero }; 

        for( int i = 0; i < steps; i++ ) {
            stairProfile.Add( stairProfile[ ^1 ] + stepPercent * height * -Vector3.forward );
            stairProfile.Add( stairProfile[ ^1 ] + stepPercent * width * Vector3.up );
        }

        return GenerateExtrudedMesh( stairProfile, 1.0f, null, front, 0.0f, 0.0f, clockwise, false, texturing.tiling, Vector2.zero, rotationCorrection, 0.5f );
    }

    public static bool LineLineIntersect( out Vector3 intersection, Vector3 normal, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, ArrayType line1Type, ArrayType line2Type ) {

        Plane plane = new( normal, linePoint1 );

        Vector3 linePoint3 = plane.ClosestPointOnPlane( linePoint1 + lineVec1 );
        Vector3 linePoint4 = plane.ClosestPointOnPlane( linePoint2 + lineVec2 );

        linePoint1 = plane.ClosestPointOnPlane( linePoint1 );
        linePoint2 = plane.ClosestPointOnPlane( linePoint2 );

        lineVec1 = linePoint3 - linePoint1;
        lineVec2 = linePoint4 - linePoint2;

        Vector3 lineVec3 = linePoint2 - linePoint1;

        Vector3 crossVec1and2 = Vector3.Cross( lineVec1, lineVec2 );
        Vector3 crossVec3and2 = Vector3.Cross( lineVec3, lineVec2 );

        if( crossVec1and2.magnitude > 0 ) {

            float t = Vector3.Dot( crossVec3and2, crossVec1and2 ) / crossVec1and2.sqrMagnitude;

            if( float.IsNaN( t ) ) {
                t = 0;
            }

            intersection = linePoint1 + ( lineVec1 * t );

            bool isInsideLine1 = true, isInsideLine2 = true;

            switch( line1Type ) {
                case ArrayType.Line:    isInsideLine1 = true;
                                        break;
                
                case ArrayType.Ray:     if( linePoint1 == linePoint2 || linePoint1 == linePoint4 ) {
                                            isInsideLine1 = false;
                                        }
                                        else {
                                            isInsideLine1 = Vector3.Dot( lineVec1, intersection - linePoint1 ) > 0.0f;
                                        }
                                        break;

                case ArrayType.Segment: if( linePoint1 == linePoint2 || linePoint1 == linePoint4 || linePoint3 == linePoint2 || linePoint3 == linePoint4 ) {
                                            isInsideLine1 = false;
                                        }
                                        else {
                                            isInsideLine1 = lineVec1.magnitude >= ( intersection - linePoint1 ).magnitude && lineVec1.magnitude >= ( intersection - linePoint3 ).magnitude;
                                        }

                                        break;

            }

            switch( line2Type ) {
                case ArrayType.Line:    isInsideLine2 = true;
                                        break;
                
                case ArrayType.Ray:     if( linePoint1 == linePoint2 || linePoint3 == linePoint2 ) {
                                            isInsideLine2 = false;
                                        }
                                        else {
                                            isInsideLine2 = Vector3.Dot( lineVec2, intersection - linePoint2 ) > 0.0f;
                                        }
                                        break;

                case ArrayType.Segment: if( linePoint1 == linePoint2 || linePoint1 == linePoint4 || linePoint3 == linePoint2 || linePoint3 == linePoint4 ) {
                                            isInsideLine2 = false;
                                        }
                                        else {
                                            isInsideLine2 = lineVec2.magnitude >= ( intersection - linePoint2 ).magnitude && lineVec2.magnitude >= ( intersection - linePoint4 ).magnitude;
                                        }

                                        break;

            }

            if( !isInsideLine1 || !isInsideLine2 ) {
                intersection = Vector3.zero;
                return false;
            }
            else {
                return true;
            }   
        }
        else {
            // Linee parallele
            intersection = Vector3.zero;
            return false;
        }     
    }

    public static ProceduralMesh GeneratePoligonalMesh( List<Vector3> points, bool clockwiseRotation, Vector3 up, Vector3 right, Vector2 tiling ) {

        List<Edge> perimeterEdges = new();
        for( int i = 0; i < points.Count; i++ ) { 

            //Debug.Log( ">>> Point[ " + i + " ]: " + points[ i ] );

            if( i < points.Count - 1 ) {
                perimeterEdges.Add( new Edge( points[ i ], points[ i + 1 ], i, i + 1 ) );

                //Debug.DrawLine( points[ i ], points[ i + 1 ], Color.blue, Time.deltaTime );
            }
            else {
                perimeterEdges.Add( new Edge( points[ i ], points[ 0 ], i, 0 ) );
                //Debug.DrawLine( points[ i ], points[ 0 ], Color.green, Time.deltaTime );
            }
        }
        
        List<Edge> edges = new();
        edges.AddRange( perimeterEdges );

        List<Edge> internalEdges = new();

        for( int i = 0; i < points.Count; i++ ) {

            HashSet<int> ignorePoints = new() { i };
            if( i == 0 ) {
                ignorePoints.Add( points.Count - 1 );
                ignorePoints.Add( i + 1 );
            }
            if( i > 0 && i < points.Count - 1 ) {
                ignorePoints.Add( i - 1 );
                ignorePoints.Add( i + 1 );
            }
            else if( i == points.Count - 1 ) {
                ignorePoints.Add( i - 1 );
                ignorePoints.Add( 0 );
            }

            //Debug.Log( ">>> i: " + i );
            for( int j = 0; j < points.Count; j++ ) {

                if( !ignorePoints.Contains( j ) ) {

                    bool existsInverseEdge = false;

                    foreach( Edge internalEdge in internalEdges ) {
                        if( i == internalEdge.endingPointIndex && j == internalEdge.startingPointIndex ) {
                            existsInverseEdge = true;
                            break;
                        }
                    }

                    if( !existsInverseEdge ) {

                        bool intersect = false;
                        foreach( Edge edge in edges ) {
                            if( edge.startingPointIndex != i ) {
                                Vector3 internalIntersectionPoint;

                                intersect = LineLineIntersect( out internalIntersectionPoint, Quaternion.AngleAxis( -90.0f, right ) * up, points[ i ], points[ j ] - points[ i ], edge.startingPoint, edge.endingPoint - edge.startingPoint, ArrayType.Segment, ArrayType.Segment );

                                if( intersect ) {
                                    break;
                                }
                            }
                        }

                        if( !intersect ) { 

                            float epsilonValue = 0.25f;
                            Vector3 epsilonPoint = points[ i ] + ( points[ j ] - points[ i ] ).normalized * epsilonValue;

                            int counter = 0;
                            foreach( Edge perimeterEdge in perimeterEdges ) {

                                int counterOld = counter;
                                Vector3 perimeterIntersectionPoint;
                                counter += LineLineIntersect( out perimeterIntersectionPoint, Quaternion.AngleAxis( -90.0f, right ) * up, epsilonPoint, up, perimeterEdge.startingPoint, perimeterEdge.endingPoint - perimeterEdge.startingPoint, ArrayType.Ray, ArrayType.Segment ) ? 1 : 0;
                            }
                            bool outside = counter % 2 == 0;
                            
                            if( !outside ) {

                                edges.Add( new Edge( points[ i ], points[ j ], i, j ) );
                                internalEdges.Add( new Edge( points[ i ], points[ j ], i, j ) );

                                Vector3 halfWay = edges[ ^1 ].startingPoint + ( ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).normalized * ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).magnitude / 2 );
                                Vector3 arrow0 = halfWay + Quaternion.AngleAxis( 90.0f, Quaternion.AngleAxis( -90.0f, right ) * up ) * ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).normalized * 0.1f;
                                Vector3 arrow1 = halfWay + Quaternion.AngleAxis( 90.0f, Quaternion.AngleAxis( 90.0f, right ) * up ) * ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).normalized * 0.1f;
                                Vector3 arrow2 = halfWay + ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).normalized * 0.2f;

                                // Debug.DrawLine( edges[ ^1 ].startingPoint, edges[ ^1 ].endingPoint, Color.red, 9999 );
                                // Debug.DrawLine( arrow0, arrow1, Color.cyan, 9999 );
                                // Debug.DrawLine( arrow1, arrow2, Color.cyan, 9999);
                                // Debug.DrawLine( arrow2, arrow0, Color.cyan, 9999 );
                                // Debug.DrawLine( edges[ ^1 ].startingPoint, edges[ ^1 ].endingPoint, Color.cyan, 9999 );
                                

                            }
                        }
                    }
                }
            }
        }

        List<int> triangles = new();
        List<Vector2> uv = new();

        for( int i = 0; i < points.Count; i++ ) { 
            List<Edge> vertexEdges = new();
            foreach( Edge edge in edges ) {
                if( edge.startingPointIndex == i ) {
                    vertexEdges.Add( edge );
                }
            }
            if( i == 0 ) {
                vertexEdges.Add( new Edge( perimeterEdges[ ^1 ].endingPoint, perimeterEdges[ ^1 ].startingPoint, perimeterEdges[ ^1 ].endingPointIndex, perimeterEdges[ ^1 ].startingPointIndex ) );
            }

            for( int j = 0; j < vertexEdges.Count - 1; j++ ) {
                
                foreach( Edge edge in edges ) {

                    if( edge.startingPointIndex == vertexEdges[ j ].endingPointIndex && edge.endingPointIndex == vertexEdges[ j + 1 ].endingPointIndex ) {
                        
                        triangles.Add( vertexEdges[ j ].startingPointIndex );
                        if( clockwiseRotation ) {
                            triangles.Add( vertexEdges[ j ].endingPointIndex );
                            triangles.Add( vertexEdges[ j + 1 ].endingPointIndex );
                        }
                        else {
                            triangles.Add( vertexEdges[ j + 1 ].endingPointIndex );
                            triangles.Add( vertexEdges[ j ].endingPointIndex );
                        }
                        break;
                    }
                    else if( edge.endingPointIndex == vertexEdges[ j ].endingPointIndex && edge.startingPointIndex == vertexEdges[ j + 1 ].endingPointIndex ) {
                        triangles.Add( vertexEdges[ j ].startingPointIndex );
                        if( clockwiseRotation ) {
                            triangles.Add( vertexEdges[ j + 1 ].endingPointIndex );
                            triangles.Add( vertexEdges[ j ].endingPointIndex );
                        }
                        else {
                            triangles.Add( vertexEdges[ j ].endingPointIndex );
                            triangles.Add( vertexEdges[ j + 1 ].endingPointIndex );
                        }
                        break;
                    }
                }
            }

            Vector3 r = points[ i ] - points[ 0 ];
            float dotU = Vector3.Dot( r, right );
            float dotV = Vector3.Dot( r, up );
            uv.Add( new Vector3( dotU / tiling.x, dotV / tiling.y ) );
        }

        Mesh poligon = new(){ vertices = points.ToArray(),
                              triangles = triangles.ToArray(),
                              uv = uv.ToArray() };
        poligon.RecalculateNormals();

        return new() { mesh = poligon };
    }

    public static Vector3 CalculatePoligonCenterPoint( List<Vector3> vertices )
    {
        int  verticesNumber =  vertices.Count;

        float x = 0;
        float y = 0;
        float z = 0;

        foreach( Vector3 vertex in vertices )
        {
            x += vertex.x;
            y += vertex.y;
            z += vertex.z;
        }

        x /= verticesNumber;
        y /= verticesNumber;
        z /= verticesNumber;

        Vector3 centerPoint = new( x, y, z );

        return centerPoint;
    }

    public static Vector3[ , ] ConvertListsToMatrix_2xM( List<Vector3> up, List<Vector3> down )
    {
        if( up.Count == down.Count )
        {
            Vector3[ , ] vertexMatrix = new Vector3[ up.Count, down.Count ];

            for( int row = 0; row < 2; row++ )
            {
               for( int col = 0; col < up.Count; col++ )
               {
                    if( row == 0 )
                    {
                        vertexMatrix[ row, col ] = up[ col ];
                    }
                    else
                    {
                        vertexMatrix[ row, col ] = down[ col ];
                    }
               } 
            }

            return vertexMatrix;

        }
        else
        {
            return null;
        }
    }

    public static List<Vector3> JoinProfiles( List<ProceduralMesh> combinedMeshes, int index ) {
        List<Vector3> unifiedProfile = new();

        for( int i = 0; i < combinedMeshes.Count; i++ ) {
            
            for( int j = 0; j < combinedMeshes[ i ].verticesStructure[ Orientation.Vertical ][ index ].Count; j++ ) {

                unifiedProfile.Add( combinedMeshes[ i ].verticesStructure[ Orientation.Vertical ][ index ][ j ] );
                
                if( j == combinedMeshes[ i ].verticesStructure[ Orientation.Vertical ][ index ].Count - 1 && i < combinedMeshes.Count - 1 && combinedMeshes[ i + 1 ].verticesStructure[ Orientation.Vertical ][ index ][ 0 ] == combinedMeshes[ i ].verticesStructure[ Orientation.Vertical ][ index ][ j ] ) {
                    unifiedProfile.RemoveAt( unifiedProfile.Count - 1 );
                }
            }
        }

        return unifiedProfile;
    }

    public static void InstantiateSwitchNewLineGround( GameObject sectionGameObj, Side side, Floor entrance, Floor exit, Floor central, int bezPoints, Utils.Texturing groundTexturing ) {
        
        Mesh groundMesh = new();

        // Parte iniziale 

        int startingIndex = central.switchNewStartIndex;
        int endingIndex = central.switchNewEndIndex;

        List<Vector3> centralPoints = new();
        centralPoints.AddRange( central.switchNewBaseLine );

        List<Vector3> entrancePoints = entrance.centerL;
        List<Vector3> exitPoints = exit.centerL;
        if( side == Side.Right ) {
            entrancePoints = entrance.centerR;
            exitPoints = exit.centerR;
        }

        exitPoints.Reverse();

        List<Vector3> entrance0 = entrancePoints.GetRange( 0, startingIndex + 1 );
        List<Vector3> exit0 =  exitPoints.GetRange( 0, startingIndex + 1 );
        exit0.Reverse();

        List<Vector3> entrance1 = entrancePoints.GetRange( startingIndex, endingIndex - startingIndex );
        List<Vector3> exit1 =  exitPoints.GetRange( startingIndex, endingIndex - startingIndex );
        exit1.Reverse();

        List<Vector3> entrance2 = entrancePoints.GetRange( endingIndex - 1, entrancePoints.Count - endingIndex + 1 );
        List<Vector3> exit2 =  exitPoints.GetRange( endingIndex - 1, entrancePoints.Count - endingIndex + 1 );
        exit2.Reverse();

        List<Vector3> central0 = centralPoints.GetRange( 0, bezPoints );
        central0.Reverse();
        List<Vector3> central1 = centralPoints.GetRange( bezPoints - 1, endingIndex - startingIndex );
        central1.Reverse();
        List<Vector3> central2 = centralPoints.GetRange( bezPoints + endingIndex - startingIndex - 2, bezPoints );
        central2.Reverse();
        List<Vector3> central3 = centralPoints.GetRange( ( 2 * bezPoints ) + endingIndex - startingIndex - 3, endingIndex - startingIndex );
        central3.Reverse();
        List<Vector3> central4 = centralPoints.GetRange( ( 2 * ( bezPoints + endingIndex - startingIndex ) ) - 4, bezPoints );
        central4.Reverse();

        Vector3 switchDir = ( exit0[ ^1 ] - entrance0[ 0 ] ).normalized;

        List<Vector3> perimeter0 = new();
        perimeter0.AddRange( entrance0 );
        perimeter0.AddRange( central0 );
        Mesh partialMesh0 = GenerateConvexPoligonalMesh( perimeter0, central.switchNewStartIntersection, side == Side.Left, -Vector3.forward, switchDir, groundTexturing.tiling, Vector3.zero );

        List<Vector3> perimeter1 = new();
        perimeter1.AddRange( entrance1 );
        perimeter1.AddRange( central1 );
        Vector3 center1 = CalculatePoligonCenterPoint( perimeter1 );
        Mesh partialMesh1 = GenerateConvexPoligonalMesh( perimeter1, center1, side == Side.Left, -Vector3.forward, switchDir, groundTexturing.tiling, Vector3.zero );

        List<Vector3> perimeter2 = new();
        perimeter2.AddRange( entrance2 );
        perimeter2.AddRange( exit2 );
        perimeter2.AddRange( central2 );
        Mesh partialMesh2 = GenerateConvexPoligonalMesh( perimeter2, central.switchNewUpIntersection, side == Side.Left, -Vector3.forward, switchDir, groundTexturing.tiling, Vector3.zero );

        List<Vector3> perimeter3 = new();
        perimeter3.AddRange( exit1 );
        perimeter3.AddRange( central3 );
        Vector3 center3 = CalculatePoligonCenterPoint( perimeter3 );
        Mesh partialMesh3 = GenerateConvexPoligonalMesh( perimeter3, center3, side == Side.Left, -Vector3.forward, switchDir, groundTexturing.tiling, Vector3.zero );

        List<Vector3> perimeter4 = new();
        perimeter4.AddRange( exit0 );
        perimeter4.AddRange( central4 );
        Mesh partialMesh4 = GenerateConvexPoligonalMesh( perimeter4, central.switchNewEndIntersection, side == Side.Left, -Vector3.forward, switchDir, groundTexturing.tiling, Vector3.zero );

        CombineInstance[] combine = new CombineInstance[ 5 ];

        combine[ 0 ].mesh = partialMesh0;
        combine[ 0 ].transform = Matrix4x4.identity;
        combine[ 1 ].mesh = partialMesh1;
        combine[ 1 ].transform = Matrix4x4.identity;
        combine[ 2 ].mesh = partialMesh2;
        combine[ 2 ].transform = Matrix4x4.identity;
        combine[ 3 ].mesh = partialMesh3;
        combine[ 3 ].transform = Matrix4x4.identity;
        combine[ 4 ].mesh = partialMesh4;
        combine[ 4 ].transform = Matrix4x4.identity;

        groundMesh.CombineMeshes( combine );

        GameObject groundObj = new( "Scambio - Terreno" );
        groundObj.transform.parent = sectionGameObj.transform;
        groundObj.transform.position = Vector3.zero;
        groundObj.AddComponent<MeshFilter>();
        groundObj.AddComponent<MeshRenderer>();
        groundObj.GetComponent<MeshFilter>().sharedMesh = groundMesh;
        groundObj.GetComponent<MeshRenderer>().SetMaterials( groundTexturing.materials ); 
        groundObj.layer = LayerMask.NameToLayer( "Walkable" );
    }

    public static List<Vector3> GetFullProfile( List<ProceduralMesh> proceduralMeshes, int index, List<int> ignoreIndexes ) {
        List<Vector3> fullProfile = new();

        ignoreIndexes ??= new();

        List<ProceduralMesh> filteredProceduralMeshes = new();
        for( int i = 0; i < proceduralMeshes.Count; i++ ) {
            if( !ignoreIndexes.Contains( i ) ) {
                filteredProceduralMeshes.Add( proceduralMeshes[ i ] );
            }
        }

        for( int i = 0; i < filteredProceduralMeshes.Count; i++ ) {
            int finalIndex = index;
            if( index < 0 ) {
                finalIndex = filteredProceduralMeshes[ i ].verticesStructure[ Orientation.Vertical ].Count + index;
            }

            fullProfile.AddRange( filteredProceduralMeshes[ i ].verticesStructure[ Orientation.Vertical ][ finalIndex ] );

            if( i < filteredProceduralMeshes.Count - 1 ) {
                fullProfile.RemoveAt( fullProfile.Count - 1 );
            }
        }

        return fullProfile;
    }
    
    public static Floor CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( LineSection section, float centerWidth, float sideWidth, float railsWidth )
    {
        List<Vector3> curve = section.bezierCurveLimitedAngle;
        List<Vector3> controlPoints = section.controlsPoints;

        List<Vector3> railLeftR = new();
        List<Vector3> railLeftL = new();
        List<Vector3> railRightR = new();
        List<Vector3> railRightL = new();
        
        List<Vector3> leftLine = new();
        List<Vector3> leftL = new();

        List<Vector3> centerR = new();
        List<Vector3> centerL = new();

        List<Vector3> rightR = new();
        List<Vector3> rightLine = new();

        Vector3 dir;

        for( int i = 0; i < curve.Count; i++ )
        {
            if( i == 0 ) { 
               dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, 0.0f ) - new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) ).normalized;
            }
            else {
                dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i ].x, curve[ i ].y, 0.0f ) - new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f ) ).normalized;
            }

            leftL.Add( curve[ i ] + ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );
            leftLine.Add( curve[ i ] + ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
            
            centerL.Add( curve[ i ] + ( dir * ( centerWidth / 2 ) ) );
            centerR.Add( curve[ i ] - ( dir * ( centerWidth / 2 ) ) );

            rightLine.Add( curve[ i ] - ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
            rightR.Add( curve[ i ] - ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );

            railLeftL.Add( leftLine[ i ] + ( dir * ( railsWidth / 2 ) ) );
            railLeftR.Add( leftLine[ i ] - ( dir * ( railsWidth / 2 ) ) );
            railRightL.Add( rightLine[ i ] + ( dir * ( railsWidth / 2 ) ) );
            railRightR.Add( rightLine[ i ] - ( dir * ( railsWidth / 2 ) ) );
        }

        Floor singleFloor = new() { leftL = leftL,
                                    leftLine = leftLine,
                                    leftR = centerL,

                                    centerL = centerL,
                                    centerLine = curve,
                                    centerR = centerR,

                                    rightL = centerR,
                                    rightLine = rightLine,
                                    rightR = rightR,

                                    railLeftL = railLeftL,
                                    railLeftR = railLeftR,
                                    railRightL = railRightL,
                                    railRightR = railRightR };

        return singleFloor;
    }

    public static Floor CalculateBidirectionalWithCentralPlatformFloorMeshVertex( LineSection section, float centerWidth, float sideWidth, float railsWidth, float stationLenght, float stationExtensionLenght, float stationExtensionHeight, int stationExtensionCurvePoints )
    {
        List<Vector3> curve = section.bezierCurveLimitedAngle;
        
        List<Vector3> leftR = new();
        List<Vector3> leftLine = new();
        List<Vector3> leftL = new();

        List<Vector3> leftRailR = new();
        List<Vector3> leftRailL = new();
        List<Vector3> rightRailL = new();
        List<Vector3> rightRailR = new();

        List<Vector3> rightR = new();
        List<Vector3> rightLine = new();
        List<Vector3> rightL = new();


        Vector3 dir = ( new Vector3( curve[ 1 ].x, curve[ 1 ].y, 0.0f ) - new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) ).normalized;
        Vector3 orthogonalDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir;

        // Calcolo punti controllo
        Vector3 lb0, lb1, lb2, lb3, rb0, rb1, rb2, rb3;

        rb0 = curve[ 0 ] - ( orthogonalDir  * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) );
        rb1 = rb0 + ( dir * ( stationExtensionLenght / 2 ) );
        rb2 = rb1 - ( orthogonalDir * ( stationExtensionHeight / 2 ) );
        rb3 = rb2 + ( dir * ( stationExtensionLenght / 2 ) );

        lb0 = curve[ 0 ] + ( orthogonalDir  * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) );
        lb1 = lb0 + ( dir * ( stationExtensionLenght / 2 ) );
        lb2 = lb1 + ( orthogonalDir * ( stationExtensionHeight / 2 ) );
        lb3 = lb2 + ( dir * ( stationExtensionLenght / 2 ) );

        leftL.Add( lb0 + ( orthogonalDir * sideWidth / 2 ) );
        leftRailL.Add( lb0 + ( orthogonalDir * railsWidth / 2 ) );
        leftLine.AddRange( BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ lb0, lb1, lb2, lb3 }, stationExtensionCurvePoints ) );
        leftRailR.Add( lb0 - ( orthogonalDir * railsWidth / 2 ) );
        leftR.Add( lb0 - ( orthogonalDir * sideWidth / 2 ) );
        
        rightL.Add( rb0 + ( orthogonalDir * sideWidth / 2 ) );
        rightRailL.Add( rb0 + ( orthogonalDir * railsWidth / 2 ) );
        rightLine.AddRange( BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ rb0, rb1, rb2, rb3 }, stationExtensionCurvePoints ) );
        rightRailR.Add( rb0 - ( orthogonalDir * railsWidth / 2 ) );
        rightR.Add( rb0 - ( orthogonalDir * sideWidth / 2 ) );

        for( int i = 1; i < stationExtensionCurvePoints - 1; i++ ) {

            Vector3 orthogonalDirLeft = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( leftLine[ i - 1 ].x, leftLine[ i - 1 ].y, 0.0f ) - new Vector3( leftLine[ i + 1 ].x, leftLine[ i + 1 ].y, 0.0f ) ).normalized;
            leftL.Add( leftLine[ i ] - ( orthogonalDirLeft * sideWidth / 2 ) );
            leftRailL.Add( leftLine[ i ] - ( orthogonalDirLeft * railsWidth / 2 ) );
            leftR.Add( leftLine[ i ] + ( orthogonalDirLeft * sideWidth / 2 ) );
            leftRailR.Add( leftLine[ i ] + ( orthogonalDirLeft * railsWidth / 2 ) );

            Vector3 orthogonalDirRight = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( rightLine[ i - 1 ].x, rightLine[ i - 1 ].y, 0.0f ) - new Vector3( rightLine[ i + 1 ].x, rightLine[ i + 1 ].y, 0.0f ) ).normalized;
            rightL.Add( rightLine[ i ] - ( orthogonalDirRight * sideWidth / 2 ) );
            rightRailL.Add( rightLine[ i ] - ( orthogonalDirRight * railsWidth / 2 ) );
            rightR.Add( rightLine[ i ] + ( orthogonalDirRight * sideWidth / 2 ) );
            rightRailR.Add( rightLine[ i ] + ( orthogonalDirRight * railsWidth / 2 ) );
        }

        leftL.Add( leftLine[ ^1 ] + ( orthogonalDir * sideWidth / 2 ) );
        leftRailL.Add( leftLine[ ^1 ] + ( orthogonalDir * railsWidth / 2 ) );
        leftR.Add( leftLine[ ^1 ] - ( orthogonalDir * sideWidth / 2 ) );
        leftRailR.Add( leftLine[ ^1 ] - ( orthogonalDir * railsWidth / 2 ) );
        
        rightL.Add( rightLine[ ^1 ] + ( orthogonalDir * sideWidth / 2 ) );
        rightRailL.Add( rightLine[ ^1 ] + ( orthogonalDir * railsWidth / 2 ) );
        rightR.Add( rightLine[ ^1 ] - ( orthogonalDir * sideWidth / 2 ) );
        rightRailR.Add( rightLine[ ^1 ] - ( orthogonalDir * railsWidth / 2 ) );

        // Calcolo punti controllo finali
        Vector3 lb0Inv, lb1Inv, lb2Inv, lb3Inv, rb0Inv, rb1Inv, rb2Inv, rb3Inv;

        lb0Inv = leftLine[ ^1 ] + ( dir * stationLenght );
        rb0Inv = rightLine[ ^1 ] + ( dir * stationLenght );

        lb1Inv = lb0Inv  + ( dir * ( stationExtensionLenght / 2 ) );
        lb2Inv = lb1Inv - ( orthogonalDir * ( stationExtensionHeight / 2 ) );
        lb3Inv = lb2Inv + ( dir * ( stationExtensionLenght / 2 ) );

        rb1Inv = rb0Inv + ( dir * ( stationExtensionLenght / 2 ) );
        rb2Inv = rb1Inv + ( orthogonalDir * ( stationExtensionHeight / 2 ) );
        rb3Inv = rb2Inv + ( dir * ( stationExtensionLenght / 2 ) );

        leftLine.AddRange( BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ lb0Inv, lb1Inv, lb2Inv, lb3Inv }, stationExtensionCurvePoints ) );
        rightLine.AddRange( BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ rb0Inv, rb1Inv, rb2Inv, rb3Inv }, stationExtensionCurvePoints ) );

        for( int i = stationExtensionCurvePoints; i < ( 2 * stationExtensionCurvePoints ) - 1; i++ ) {
            Vector3 orthogonalDirLeft = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( leftLine[ i - 1 ].x, leftLine[ i - 1 ].y, 0.0f ) - new Vector3( leftLine[ i + 1 ].x, leftLine[ i + 1 ].y, 0.0f ) ).normalized;
            leftL.Add( leftLine[ i ] - ( orthogonalDirLeft * sideWidth / 2 ) );
            leftRailL.Add( leftLine[ i ] - ( orthogonalDirLeft * railsWidth/ 2 ) );
            leftR.Add( leftLine[ i ] + ( orthogonalDirLeft * sideWidth / 2 ) );
            leftRailR.Add( leftLine[ i ] + ( orthogonalDirLeft * railsWidth/ 2 ) );

            Vector3 orthogonalDirRight = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( rightLine[ i - 1 ].x, rightLine[ i - 1 ].y, 0.0f ) - new Vector3( rightLine[ i + 1 ].x, rightLine[ i + 1 ].y, 0.0f ) ).normalized;
            rightL.Add( rightLine[ i ] - ( orthogonalDirRight * sideWidth / 2 ) );
            rightRailL.Add( rightLine[ i ] - ( orthogonalDirRight * railsWidth / 2 ) );
            rightR.Add( rightLine[ i ] + ( orthogonalDirRight * sideWidth / 2 ) );
            rightRailR.Add( rightLine[ i ] + ( orthogonalDirRight * railsWidth / 2 ) );
        }

        leftL.Add( leftLine[ ^1 ] + ( orthogonalDir * sideWidth / 2 ) );
        leftRailL.Add( leftLine[ ^1 ] + ( orthogonalDir * railsWidth / 2 ) );
        leftR.Add( leftLine[ ^1 ] - ( orthogonalDir * sideWidth / 2 ) );
        leftRailR.Add( leftLine[ ^1 ] - ( orthogonalDir * railsWidth / 2 ) );
        
        rightL.Add( rightLine[ ^1 ] + ( orthogonalDir * sideWidth / 2 ) );
        rightRailL.Add( rightLine[ ^1 ] + ( orthogonalDir * railsWidth / 2 ) );
        rightR.Add( rightLine[ ^1 ] - ( orthogonalDir * sideWidth / 2 ) );
        rightRailR.Add( rightLine[ ^1 ] - ( orthogonalDir * railsWidth / 2 ) );

        Floor singleFloor = new() { leftL = leftL,
                                    leftLine = leftLine,
                                    leftR = leftR,

                                    centerL = leftR,
                                    centerLine = curve,
                                    centerR = rightL,

                                    rightL = rightL,
                                    rightLine = rightLine,
                                    rightR = rightR,
                                    
                                    railLeftL = leftRailL,
                                    railLeftR = leftRailR,
                                    railRightL = rightRailL,
                                    railRightR = rightRailR };

        return singleFloor;
    }

    public static Floor CalculateMonodirectionalFloorMeshVertex( List<Vector3> curve, Vector3? startingDir, float floorWidth, float railsWidth )
    {

        List<Vector3> rightPoints = new();
        List<Vector3> leftPoints = new();

        List<Vector3> railCenterR = new();
        List<Vector3> railCenterL = new();

        Vector3 dir;

        for( int i = 0; i < curve.Count; i++ )
        {
            if( i == 0 ) {
                if( startingDir == null ) {
                    dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 1 ].x, curve[ 1 ].y, 0.0f ) - new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) ).normalized;
                }
                else {
                    dir = ( Vector3 )startingDir;
                    dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir.normalized;
                }
            }
            else {
                dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i ].x, curve[ i ].y, 0.0f ) - new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f ) ).normalized;
            }

            leftPoints.Add( curve[ i ] + ( dir * ( floorWidth / 2 ) )  );
            rightPoints.Add(curve[ i ] - ( dir * ( floorWidth / 2 ) )  );

            railCenterL.Add( curve[ i ] + ( dir * ( railsWidth / 2 ) ) );
            railCenterR.Add( curve[ i ] - ( dir * ( railsWidth / 2 ) ) );
        }

        Floor singleFloor = new() { centerL = leftPoints,
                                    centerLine = curve,
                                    centerR = rightPoints,
                                    railCenterL = railCenterL,
                                    railCenterR = railCenterR };

        return singleFloor;
    }

    public static SpecularBaseLine CalculateBaseLinesFromCurve( List<Vector3> curve, Vector3? startingDir, float distance, float angle ) {

        SpecularBaseLine lines = new() { left = new List<Vector3>(),
                                         right = new List<Vector3>() };

        Vector3 curveDir = curve[ 1 ] - curve[ 0 ];
        if( startingDir != null ) {
            curveDir = ( Vector3 )startingDir;
        }
        curveDir = curveDir.normalized;

        Vector3 leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * curveDir * distance;
        Vector3 rightDir = -leftDir;

        leftDir = Quaternion.AngleAxis( -angle, curveDir ) * leftDir;
        rightDir = Quaternion.AngleAxis( angle, curveDir ) * rightDir;

        lines.left.Add( curve[ 0 ] + leftDir );
        lines.right.Add( curve[ 0 ] + rightDir );

        for( int i = 1; i < curve.Count; i++ )
        {

            curveDir = ( curve[ i ] - curve[ i - 1 ] ).normalized;
            leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * curveDir * distance;
            rightDir = -leftDir;

            leftDir = Quaternion.AngleAxis( -angle, curveDir ) * leftDir;
            rightDir = Quaternion.AngleAxis( angle, curveDir ) * rightDir;

            lines.left.Add( curve[ i ] + leftDir );
            lines.right.Add( curve[ i ] + rightDir );
        }

        return lines;
    }

    public static PlatformSide CalculatePlatformSidesMeshesVertex( List<Vector3> curve, Vector3? startingDir, float floorWidth, bool floorParabolic, float sideHeight, float sideWidth )
    {
        PlatformSide platformSide = new()
        {
            leftDown = new List<Vector3>(),
            rightDown = new List<Vector3>(),
            leftUp = new List<Vector3>(),
            rightUp = new List<Vector3>(),
            leftFloorLeft = new List<Vector3>(),
            leftFloorRight = new List<Vector3>(),
            rightFloorLeft = new List<Vector3>(),
            rightFloorRight = new List<Vector3>()
        };

        Vector3 dir;

        for( int i = 0; i < curve.Count; i++ )
        {

            if( i == 0 ) {
                if( startingDir == null ) {
                    dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 1 ].x, curve[ 1 ].y, 0.0f ) - new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) ).normalized;
                }
                else {
                    dir = ( Vector3 )startingDir;
                    dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir.normalized;
                }
            }
            else {
                dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i ].x, curve[ i ].y, 0.0f ) - new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f ) ).normalized;
            }

            platformSide.leftDown.Add( curve[ i ] + ( dir * ( floorWidth / 2 ) ) );
            platformSide.rightDown.Add(curve[ i ] - ( dir * ( floorWidth / 2 ) ) );

            platformSide.leftUp.Add( new Vector3( platformSide.leftDown[ i ].x, platformSide.leftDown[ i ].y, platformSide.leftDown[ i ].z - sideHeight ) ); 
            platformSide.leftFloorRight.Add( platformSide.leftUp[ i ] );
            platformSide.leftFloorLeft.Add( platformSide.leftUp[ i ] + ( dir * sideWidth ) );

            platformSide.rightUp.Add( new Vector3( platformSide.rightDown[ i ].x, platformSide.rightDown[ i ].y, platformSide.rightDown[ i ].z - sideHeight ) );
            platformSide.rightFloorLeft.Add( platformSide.rightUp[ i ] );
            platformSide.rightFloorRight.Add( platformSide.rightUp[ i ] - ( dir * sideWidth ) );
        }

        return platformSide;
    }

    public static List<Vector3> CalculateCircularShape( float radius, int points, Vector2 centerCoords, float eccentricity ) {
        List<Vector3> shape = new();

        float fixedAngle = 360.0f / points;

        float k = Mathf.Sqrt( 1 - Mathf.Pow( eccentricity, 2 ) );
        float a = radius / k;
        float b = a * k;

        float alpha = 0.0f;
        for( int i = 0; i < points; i++ ) {
            //Debug.Log( ">>> alpha: " + alpha );
            float x = centerCoords.x + ( a * Mathf.Cos( alpha * Mathf.Deg2Rad ) );
            float y = centerCoords.y + ( b * Mathf.Sin( alpha * Mathf.Deg2Rad ) );

            shape.Add( new Vector3( 0.0f, x, y ) );

            alpha += fixedAngle;
        }
        shape.Add( shape[ 0 ] );

        return shape;
    }

    public static ProceduralMesh GenerateExtrudedMesh( /*Orientation extrusionOrientation,*/ List<Vector3> profileVertices, float profileScale, List<Vector3> previousProfileVertices, List<Vector3> baseVertices, float horPosCorrection, float vertPosCorrection, bool clockwiseRotation, bool closeMesh, Vector2 tiling, Vector2 offset, float verticalRotationCorrection, float smoothFactor ) {
        
        // Vector3 worldUp = extrusionOrientation == Orientation.Horizontal ? -Vector3.forward : Vector3.right;
        // Vector3 worldRight = extrusionOrientation == Orientation.Horizontal ? Vector3.right : -Vector3.forward;
        
        ProceduralMesh extrudedMesh = new();

        if( closeMesh && baseVertices[ ^1 ] != baseVertices[ 0 ] ) {
            baseVertices.Add( baseVertices[ 0 ] );
        }

        // Mappa livello - vertice (ogni livello è un punto del profilo della mesh)
        Dictionary<Orientation, Dictionary<int, List<Vector3>>> verticesStructure = new();
        verticesStructure[ Orientation.Horizontal ] = new Dictionary<int, List<Vector3>>();
        verticesStructure[ Orientation.Vertical ] = new Dictionary<int, List<Vector3>>();

        verticesStructure[ Orientation.Horizontal ][ 0 ] = baseVertices;
        for( int i = 1; i < profileVertices.Count; i++ ) {
            verticesStructure[ Orientation.Horizontal ].Add( i, new List<Vector3>() ); 
        }
        for( int i = 0; i < baseVertices.Count; i++ ) {
            verticesStructure[ Orientation.Vertical ].Add( i, new List<Vector3>{ baseVertices[ i ] } );
        }

        List<Vector3> lastProfileVertices = new();
        List<Vector3> profileVerticesScaled = new();
        foreach( Vector3 profileVertex in profileVertices ) {
            profileVerticesScaled.Add( profileVertex * profileScale );
        }

        Mesh mesh = new() { name = "Procedural Mesh" };

        int h = profileVerticesScaled.Count;
        int trianglesCounter = ( h - 1 ) * ( baseVertices.Count - 1 ) * 6;
        int verticesCounter = profileVerticesScaled.Count * baseVertices.Count;

        int[] triangles = new int[ trianglesCounter ];
        Vector3[] vertices = new Vector3[ verticesCounter ];
        Vector2[] uv = new Vector2[ verticesCounter ];
        Vector3[] normals = new Vector3[ verticesCounter ];

        Vector2 uvMax = new( 0.0f, 0.0f );

        // Array di supporto con le distanze dei punti del profilo e della base calcolate rispetto allo zero (serve per gestire l'UV mapping ripetuto)
        List<float> distancesHor = new(){ 0.0f };
        List<float> distancesVert = new(){ 0.0f };

        List<Vector3> profileDirs = new();
        for( int i = 1; i < h; i++ ) {
            Vector3 profileDir = profileVerticesScaled[ i ] - profileVerticesScaled[ i - 1 ];
            profileDirs.Add( profileDir );

            distancesVert.Add( distancesVert[ i - 1 ] + profileDir.magnitude );
        }

        List<Vector3> baseDirs = new();
        for( int i = 1; i < baseVertices.Count; i++ ) { 
            Vector3 baseDir = baseVertices[ i ] - baseVertices[ i - 1 ];
            baseDirs.Add( baseDir );

            distancesHor.Add( distancesHor[ i - 1 ] + baseDir.magnitude );
        }

        // Genero i tutti i profili e aggiungo i vertici alla lista dei vertici
        for( int i = 0; i < baseVertices.Count; i++ ) {
            
            // Alpha è l'angolo che il profilo deve essere ruotato per risultare sempre alla stessa angolazione con il baseDir attuale
            float alpha = 0.0f;
            if( ( i == 0 || i == baseVertices.Count - 1 ) && closeMesh ) {
                // Nel caso di mesh chiusa, l'alpha del primo punto e ultimo punto combaciano
                alpha = -Vector3.SignedAngle( Vector3.right, baseDirs[ 0 ], -Vector3.forward ) + ( Vector3.SignedAngle( baseDirs[ 0 ], baseDirs[ baseDirs.Count - 1 ], Vector3.forward ) * smoothFactor );
            }
            else {
                if( i < baseVertices.Count - 1 ) {
                    alpha = -Vector3.SignedAngle( Vector3.right, baseDirs[ i ], -Vector3.forward );

                    // Correzione alpha per punti successivi al primo e precedenti all'ultimo
                    if( i > 0 ) {
                        alpha -= Vector3.SignedAngle( baseDirs[ i ], baseDirs[ i - 1 ], -Vector3.forward ) * smoothFactor;
                    }
                }
                else {
                    // L'angolo alpha dell'ultimo punto è calcolato sulla baseDir del punto precedente
                    alpha = -Vector3.SignedAngle( Vector3.right, baseDirs[ baseDirs.Count - 1 ], -Vector3.forward );
                }
            }

            if( i == 0 && previousProfileVertices != null && previousProfileVertices.Count == profileVerticesScaled.Count ) {
                vertices[ 0 ] = previousProfileVertices[ 0 ];
            }
            else {

                if( i >= baseDirs.Count ) {
                    Vector3 baseNormal = Vector3.Cross( baseDirs[ ( closeMesh && i == baseVertices.Count - 1 ) ? 0 : baseDirs.Count - 1 ].normalized, -Vector3.forward ).normalized;
                    vertices[ i * h ] = baseVertices[ i ] + ( ( clockwiseRotation ? 1 : -1 ) * horPosCorrection * baseNormal ) + ( -Vector3.forward * vertPosCorrection );
                }
                else {
                    Vector3 baseNormal = Vector3.Cross( baseDirs[ i ].normalized, -Vector3.forward ).normalized;
                    vertices[ i * h ] = baseVertices[ i ] + ( ( clockwiseRotation ? 1 : -1 ) * horPosCorrection * baseNormal ) + ( -Vector3.forward * vertPosCorrection );
                }

                if( i == baseVertices.Count - 1 ) {
                    lastProfileVertices.Add( vertices[ i * h ] );
                }
            }

            float u = ( distancesHor[ i ] / tiling.x ) + offset.x;
            if( u > uvMax.x ) {
                uvMax.x = u;
            }

            uv[ i * h ] = new Vector2( u, 0 );

            // Il numero di dir orizontali e verticali è minore di 1 rispetto al numero di vertici che stiamo ciclando, quindi per l'ultimo punto calcolo la normale usando la direzione precedente
            int normalHorIndex = i < baseDirs.Count ? i : ( baseDirs.Count - 1 );
            Vector3 normal = Vector3.Cross( profileDirs[ 0 ], baseDirs[ normalHorIndex ] ).normalized;
            if( !clockwiseRotation ) {
                normal = -normal;
            }
            normals[ i * h ] = normal;

            for( int j = 0; j < profileDirs.Count; j++ ) {

                if( i == 0 && previousProfileVertices != null && previousProfileVertices.Count == profileVertices.Count ) {
                    vertices[ j + 1 ] = previousProfileVertices[ j + 1 ];
                    verticesStructure[ Orientation.Horizontal ][ j + 1 ].Add( vertices[ j + 1 ] );
                    verticesStructure[ Orientation.Vertical ][ i ].Add( vertices[ j + 1 ] );
                }
                else {
                    vertices[ ( i * h ) + j + 1 ] = vertices[ ( i * h ) + j ] + Quaternion.Euler( 0.0f, 0.0f, alpha + verticalRotationCorrection ) * profileDirs[ j ];
                    verticesStructure[ Orientation.Horizontal ][ j + 1 ].Add( vertices[ ( i * h ) + j + 1 ] );
                    verticesStructure[ Orientation.Vertical ][ i ].Add( vertices[ ( i * h ) + j + 1 ] );

                    if( i == baseVertices.Count - 1 ) {
                        lastProfileVertices.Add( vertices[ ( i * h ) + j + 1 ] );
                    }
                }

                float v = ( distancesVert[ j + 1 ] / tiling.y ) + offset.y;
                if( v > uvMax.y ) {
                    uvMax.y = v;
                }
                uv[ ( i * h ) + j + 1 ] = new Vector2( u, v );

                int normalVertIndex = ( j + 1 ) < profileDirs.Count ? ( j + 1 ) : ( profileDirs.Count - 1 );
                normal = Vector3.Cross( Quaternion.Euler( 0.0f, 0.0f, alpha + verticalRotationCorrection ) * profileDirs[ normalVertIndex ], baseDirs[ normalHorIndex ] ).normalized;
                if( !clockwiseRotation ) {
                    normal = -normal;
                }
                normals[ ( i * h ) + j + 1 ] = normal;

                if( i > 0 ) {
                    int arrayIndex = 6 * ( ( ( i - 1 ) * ( h - 1 ) ) + j );
                    int vertIndex = ( int )( arrayIndex / 6 ) + ( i - 1 );

                    triangles[ arrayIndex + 0 ] = triangles[ arrayIndex + 3 ] = vertIndex;

                    if( clockwiseRotation ) {

                        triangles[ arrayIndex + 1 ] = triangles[ arrayIndex + 5 ] = vertIndex + ( h + 1 ); 
                        triangles[ arrayIndex + 2 ] = vertIndex + h;
                        triangles[ arrayIndex + 4 ] = vertIndex + 1;
                    }
                    else {
                        triangles[ arrayIndex + 1 ] = vertIndex + h;
                        triangles[ arrayIndex + 2 ] = triangles[ arrayIndex + 4 ] = vertIndex + ( h + 1 ); 
                        triangles[ arrayIndex + 5 ] = vertIndex + 1;
                    }
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;

        extrudedMesh.mesh = mesh;
        extrudedMesh.lastProfileVertex = lastProfileVertices;
        extrudedMesh.verticesStructure = verticesStructure;
        extrudedMesh.uvMax = uvMax;

        return extrudedMesh;
    }

    public static Mesh GenerateConvexPoligonalMesh( List<Vector3> closedLine, Vector3 center, bool clockwiseRotation, Vector3 planeNormal, Vector3 textureDir, Vector2 tiling, Vector2 offset ) {
        Mesh planarMesh = new();
        
        List<Vector3> perimeter = new( closedLine );
        if( perimeter[ 0 ] != perimeter[ closedLine.Count - 1 ] ) {
            perimeter.Add( perimeter[ 0 ] );
        }

        List<Vector3> vertices = new( perimeter );
        vertices.Insert( 0, center );

        int[] triangles = new int[ ( vertices.Count - 1 ) * 3 ];
        Vector2[] uv = new Vector2[ vertices.Count ];

        Vector3 dirU = textureDir.normalized;
        //Debug.DrawRay( center, dirU * 100, UnityEngine.Color.red, Mathf.Infinity );
        //Vector3 dirV = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dirU;
        Vector3 dirV = Quaternion.AngleAxis( 90.0f, planeNormal ) * dirU;
        //Debug.DrawRay( center, dirV * 100, UnityEngine.Color.blue, Mathf.Infinity );
        uv[ 0 ] = new Vector2( 0.0f, 0.0f );

        for( int i = 0; i < perimeter.Count; i++ ) {

            // Per mia convenzione closedLine è clockwise
            if( clockwiseRotation ) {
                triangles[ i * 3 ] = i + 1;
                triangles[ ( i * 3 ) + 1 ] = ( i + 2 ) >= perimeter.Count ? 1 : ( i + 2 );
                triangles[ ( i * 3 ) + 2 ] = 0;
            }
            else {
                triangles[ i * 3 ] = i + 1;
                triangles[ ( i * 3 ) + 1 ] = 0;
                triangles[ ( i * 3 ) + 2 ] = ( i + 2 ) >= perimeter.Count ? 1 : ( i + 2 );
            }

            Vector3 r = perimeter[ i ] - center;
            float dotU = Vector3.Dot( r, dirU );
            float dotV = Vector3.Dot( r, dirV );
            uv[ i + 1 ] = new Vector3( ( dotU / tiling.x ) + offset.x, ( dotV / tiling.y ) + offset.y );
        }

        planarMesh.vertices = vertices.ToArray();
        planarMesh.triangles = triangles;
        planarMesh.uv = uv;
        planarMesh.RecalculateNormals();

        return planarMesh;
    }
    
    public static ProceduralMesh GeneratePlanarMesh( List<Vector3> line, Vector3[ , ] vertMatrix, bool clockwiseRotation,  bool closeMesh, bool centerTexture, Vector2 tiling )
    {
        // In questo modo anche se aggiungo un punto alla lista, è stata copiata per valore e non per riferimento, quindi la
        // modifica non ha effetti fuori dal metodo.
        List<Vector3> curve = new( line );

        List<Vector3> up = new();
        List<Vector3> down = new();
        for( int i = 0; i < curve.Count; i++ ) {
            up.Add( vertMatrix[ 0, i ] );
            down.Add( vertMatrix[ 1, i ] );
        }

        if( closeMesh ) {
            up.Add( vertMatrix[ 0, 0 ] );
            down.Add( vertMatrix[ 1, 0 ] );

            vertMatrix = ConvertListsToMatrix_2xM( up, down ); 

            curve.Add( curve[ 0 ] );
        }

        Dictionary<int, List<Vector3>> horizontalStructure = new(){ { 0, down },
                                                                    { 1, up } };
        Dictionary<Orientation, Dictionary<int, List<Vector3>>> floorStructure = new(){ { Orientation.Horizontal, horizontalStructure },
                                                                                        { Orientation.Vertical, new Dictionary<int, List<Vector3>>() } };
 
        Mesh floorMesh = new();

        Vector3[] vertices = new Vector3[ curve.Count * 2 ];
        int[] edges = new int[( curve.Count - 1 ) * 6 ];
        Vector2[] uv = new Vector2[ vertices.Length ];
        float u = 0.0f, vMin = 0.0f, vMax = 0.0f;
        // List<Vector2[]> uvs = new ();
        // List<float> us = new(), vMins = new(), vMaxs = new();
        for( int i = 0; i < curve.Count; i++ )
        {
            floorStructure[ Orientation.Vertical ].Add( i, new List<Vector3>{ vertMatrix[ 1, i ], vertMatrix[ 0, i ] } );

            if( i < curve.Count - 1 )
            {
                if( clockwiseRotation ) {
                    edges[ ( i * 6 ) + 0 ] = i * 2;
                    edges[ ( i * 6 ) + 1 ] = edges[ ( i * 6 ) + 4 ] = ( i * 2 ) + 1;
                    edges[ ( i * 6 ) + 2 ] = edges[ ( i * 6 ) + 3 ] = ( i * 2 ) + 2;
                    edges[ ( i * 6 ) + 5 ] = ( i * 2 ) + 3;
                }
                else {
                    edges[ ( i * 6 ) + 0 ] = i * 2;
                    edges[ ( i * 6 ) + 1 ] = edges[ ( i * 6 ) + 4 ] = ( i * 2 ) + 2;
                    edges[ ( i * 6 ) + 2 ] = edges[ ( i * 6 ) + 3 ] = ( i * 2 ) + 1;
                    edges[ ( i * 6 ) + 5 ] = ( i * 2 ) + 3;
                }
            }

            // for( int j = 0; j < tilings.Count; j++ ) {

            //     if( uvs.Count - 1 < j ) {
            //         uvs.Add( new Vector2[ vertices.Length ] );
            //     }
            //     if( us.Count - 1 < j ) {
            //         us.Add( 0.0f );
            //     }
            //     if( vMins.Count - 1 < j ) {
            //         vMins.Add( 0.0f );
            //     }
            //     if( vMaxs.Count - 1 < j ) {
            //         vMaxs.Add( 0.0f );
            //     }

            //     vertices[ i * 2 ] = vertMatrix[ 0, i ];
            //     vertices[ ( i * 2 ) + 1 ] = vertMatrix[ 1, i ];

            //     // deltaLText rappresenta la distanza della curva che una singola ripetizione di texture deve coprire,
            //     // considerando la lunghezza dell'intera curva e dividendola per il numero di punti ho la 
            //     // distanza (costante) fra ogni punto deltaLCurve. In questo modo, dividendo deltaLCurve per deltaLText
            //     // ottengo quante volte la texture deve ripetersi per ogni segmento e moltiplico per i in modo da mappare la texture
            //     // sulla curva indipendentemente dalla lunghezza della stessa.
                
            //     if( i > 0 ) {
            //         us[ j ] += ( float )( ( curve[ i ] - curve[ i - 1 ] ).magnitude / tilings[ j ].x );
            //     }
            //     vMaxs[ j ] = ( float )( ( vertMatrix[ 1, i ] - vertMatrix[ 0, i ] ).magnitude / tilings[ j ].y );

            //     if( centerTexture ){
            //         vMins[ j ] = -( vMaxs[ j ] / 2 );
            //         vMaxs[ j ] = -vMins[ j ];
            //     }

            //     uvs[ j ][ ( i * 2 ) ] = new Vector2( us[ j ], vMins[ j ] );
            //     uvs[ j ][ ( i * 2 ) + 1 ] = new Vector2( us[ j ], vMaxs[ j ] );
            // }

            vertices[ i * 2 ] = vertMatrix[ 0, i ];
            vertices[ ( i * 2 ) + 1 ] = vertMatrix[ 1, i ];

            // deltaLText rappresenta la distanza della curva che una singola ripetizione di texture deve coprire,
            // considerando la lunghezza dell'intera curva e dividendola per il numero di punti ho la 
            // distanza (costante) fra ogni punto deltaLCurve. In questo modo, dividendo deltaLCurve per deltaLText
            // ottengo quante volte la texture deve ripetersi per ogni segmento e moltiplico per i in modo da mappare la texture
            // sulla curva indipendentemente dalla lunghezza della stessa.
            
            if( i > 0 ) {
                u += ( float )( ( curve[ i ] - curve[ i - 1 ] ).magnitude / tiling.x );
            }
            vMax = ( float )( ( vertMatrix[ 1, i ] - vertMatrix[ 0, i ] ).magnitude / tiling.y );

            if( centerTexture ){
                vMin = -( vMax / 2 );
                vMax = -vMin;
            }

            uv[ ( i * 2 ) ] = new Vector2( u, vMin );
            uv[ ( i * 2 ) + 1 ] = new Vector2( u, vMax );
        }

        floorMesh.Clear();
        floorMesh.vertices = vertices;
        floorMesh.triangles = edges;
        // for( int i = 0; i < uvs.Count; i++ ) {

        //     Debug.Log( "uv: " + i + " - " + String.Join( " ", uvs[ i ] ) );
        //     floorMesh.SetUVs( i, uvs[ i ] );
        // }
        floorMesh.uv = uv;

        floorMesh.RecalculateNormals();

        ProceduralMesh toReturn = new() { mesh = floorMesh,
                                          lastProfileVertex = floorStructure[ Orientation.Vertical ][ floorStructure[ Orientation.Vertical ].Count - 1 ],
                                          verticesStructure = floorStructure };

        return toReturn;
    }
}
