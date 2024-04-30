using System.Collections.Generic;
using Unity.Mathematics;
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
        public List<Vector3> railSwitchLeftCenterR { get; set; }
        public List<Vector3> railSwitchLeftCenterL { get; set; }
        public List<Vector3> railSwitchRightCenterR { get; set; }
        public List<Vector3> railSwitchRightCenterL { get; set; }
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

        public List<Vector3> rightCenterL { get; set; }
        public List<Vector3> rightCenterLine { get; set; }
        public List<Vector3> rightCenterR { get; set; }

        public List<Vector3> leftCenterL { get; set; }
        public List<Vector3> leftCenterLine { get; set; }
        public List<Vector3> leftCenterR { get; set; }

        public List<Vector3> leftCenterNewLine { get; set; }
        public List<Vector3> rightCenterNewLine { get; set; }

        public List<Vector3> centerEntranceRight { get; set; }
        public List<Vector3> centerExitRight { get; set; }
        public List<Vector3> centerEntranceLeft { get; set; }
        public List<Vector3> centerExitLeft { get; set; }

        public List<Vector3> rightEntranceRight { get; set; }
        public List<Vector3> rightExitRight { get; set; }
        public List<Vector3> leftEntranceLeft { get; set; }
        public List<Vector3> leftExitLeft { get; set; }

        public List<Vector3> switchBiNewGroundBaseLine { get; set; }
        public List<Vector3> switchBiNewGroundBez0Line { get; set; }
        public List<Vector3> switchBiNewGroundBez1Line { get; set; }
        public List<Vector3> switchBiNewGroundBez2Line { get; set; }
        public List<Vector3> switchBiNewGroundLimitPoints0 { get; set; }
    }

    public class BaseLine {
        public List<Vector3> tunnelWallLeft;
        public List<Vector3> tunnelWallRight;
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
    }

    public class Wall {

    public List<Vector3> rightDown { get; set; }
    public List<Vector3> rightUp { get; set; }

    public List<Vector3> leftDown { get; set; }
    public List<Vector3> leftUp { get; set; }

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

    public static List<Vector2> Planarize3DPoints( Vector3 normal, Vector3 right, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2 ) {

        float angleToPlane = Vector3.SignedAngle( normal, -Vector3.forward, Vector3.right );

        Quaternion rot = Quaternion.AngleAxis( angleToPlane, right );

        Debug.Log( "rot * lineVec1: " + rot * lineVec1 );

        Vector2 lineVec1_2D = rot * lineVec1;
        Vector2 lineVec2_2D = rot * lineVec2;
        Vector2 lineVec3_2D = rot * ( linePoint2 - linePoint1 );

        Vector2 linePoint1_2D = new Vector3( linePoint1.x, linePoint1.y, 0.0f );
        Vector2 linePoint2_2D = linePoint1_2D + lineVec3_2D;

        return new List<Vector2>(){ linePoint1_2D, lineVec1_2D, linePoint2_2D, lineVec2_2D };
    }

    // public static bool LineLineIntersect( out Vector3 intersection, Vector3 normal, float maxPlanarFactor, float edgeReduction, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, ArrayType line1Type, ArrayType line2Type ) {

    //     Plane plane = new( normal, linePoint1 );

    //     Vector3 linePoint3 = plane.ClosestPointOnPlane( linePoint1 + lineVec1 );
    //     Vector3 linePoint4 = plane.ClosestPointOnPlane( linePoint2 + lineVec2 );

    //     // Debug.DrawRay( linePoint3, normal, Color.cyan, Time.deltaTime );
    //     // Debug.DrawRay( linePoint4, normal, Color.cyan, Time.deltaTime );

    //     linePoint1 = plane.ClosestPointOnPlane( linePoint1 );
    //     linePoint2 = plane.ClosestPointOnPlane( linePoint2 );

    //     // Debug.DrawRay( linePoint1, normal, Color.red, Time.deltaTime );
    //     // Debug.DrawRay( linePoint2, normal, Color.red, Time.deltaTime );

    //     lineVec1 = linePoint3 - linePoint1;
    //     lineVec2 = linePoint4 - linePoint2;

    //     // Debug.DrawRay( linePoint1, lineVec1, Color.blue, Time.deltaTime );
    //     // Debug.DrawRay( linePoint2, lineVec2, Color.blue, Time.deltaTime );

    //     Vector3 lineVec3 = linePoint2 - linePoint1;

    //     Vector3 crossVec1and2 = Vector3.Cross( lineVec1, lineVec2 );
    //     Vector3 crossVec3and2 = Vector3.Cross( lineVec3, lineVec2 );

    //     float signedAngleVec1and2 = Vector3.SignedAngle( lineVec1, lineVec2, normal );

    //     float planarFactor = Vector3.Dot( lineVec3, crossVec1and2 );

    //     Debug.Log( "planarFactor: " + Mathf.Abs( planarFactor ) + " < " + maxPlanarFactor + "? " + ( Mathf.Abs( planarFactor ) < maxPlanarFactor ) );
    //     Debug.Log( "crossVec1and2.sqrMagnitude: " + crossVec1and2.sqrMagnitude + " > " + 0.001f + "? " + ( crossVec1and2.sqrMagnitude > 0.001f ) );

    //     Debug.Log( "signedAngleVec1and2: " + signedAngleVec1and2 );

    //     //is coplanar, and not parallel
    //     if( Mathf.Abs( planarFactor ) < maxPlanarFactor && crossVec1and2.sqrMagnitude > 0.001f ) {
    //         // Debug.Log( "Linee non parallele" );
    //         float t = Vector3.Dot( crossVec3and2, crossVec1and2 ) / crossVec1and2.sqrMagnitude;
    //         intersection = linePoint1 + ( lineVec1 * t );

    //         // Debug.Log( "t: " + t );

    //         bool isInsideLine1 = true, isInsideLine2 = true;

    //         switch( line1Type ) {
    //             case ArrayType.Line:    isInsideLine1 = true;
    //                                     break;
                
    //             case ArrayType.Ray:     Debug.Log( "Vector3.Dot( lineVec1, intersection - linePoint1 ): " + Vector3.Dot( lineVec1, intersection - linePoint1 ) );
    //                                     isInsideLine1 = Vector3.Dot( lineVec1, intersection - linePoint1 ) > 0.001f;
    //                                     break;

    //             case ArrayType.Segment: isInsideLine1 = lineVec1.magnitude - edgeReduction >= ( intersection - linePoint1 ).magnitude && lineVec1.magnitude - edgeReduction >= ( intersection - linePoint3 ).magnitude;

    //                                     Debug.Log( "lineVec1.magnitude: " + lineVec1.magnitude );
    //                                     Debug.Log( "( intersection - linePoint1 ).magnitude: " + ( intersection - linePoint1 ).magnitude );
    //                                     Debug.Log( "( intersection - ( linePoint1 + lineVec1 ) ).magnitude: " + ( intersection - ( linePoint1 + lineVec1 ) ).magnitude );

    
    //                                     break;

    //         }

    //         Debug.Log( ">>>isInsideLine1: " + isInsideLine1 );

    //         switch( line2Type ) {
    //             case ArrayType.Line:    isInsideLine2 = true;
    //                                     break;
                
    //             case ArrayType.Ray:     isInsideLine2 = Vector3.Dot( lineVec2, intersection - linePoint2 ) > 0.001f;
    //                                     break;

    //             case ArrayType.Segment: isInsideLine2 = lineVec2.magnitude - edgeReduction >= ( intersection - linePoint2 ).magnitude && lineVec2.magnitude - edgeReduction >= ( intersection - linePoint4 ).magnitude;

    //                                     Debug.Log( "lineVec2.magnitude: " + lineVec2.magnitude );
    //                                     Debug.Log( "( intersection - linePoint2 ).magnitude: " + ( intersection - linePoint1 ).magnitude );
    //                                     Debug.Log( "( intersection - ( linePoint2 + lineVec2 ) ).magnitude: " + ( intersection - ( linePoint2 + lineVec2 ) ).magnitude );

                                        
    //                                     break;

    //         }

    //          Debug.Log( ">>>isInsideLine2: " + isInsideLine2 );

    //         if( !isInsideLine1 || !isInsideLine2 ) {
    //             intersection = Vector3.zero;
    //             return false;
    //         }
    //         else {
    //             return true;
    //         }          
    //     }
    //     else
    //     {
    //         // Debug.Log( "linee parallele" );

    //         intersection = Vector3.zero;

    //         if( line1Type == ArrayType.Line || line2Type ==  ArrayType.Line ) {

    //             return true;
    //         }
    //         else if( line1Type == ArrayType.Ray && line2Type ==  ArrayType.Ray ) {
    //             if( Vector3.Dot( lineVec1, lineVec2 ) > 0.001f ) {

    //                 return true;
    //             }
    //             else {

    //                 return Vector3.Dot( lineVec1, linePoint2 - linePoint1 ) > 0.001f;
    //             }
    //         }
    //         else if( line1Type == ArrayType.Segment && line2Type == ArrayType.Segment ) {

    //             return lineVec1.magnitude - 0.001f  > ( linePoint2 - linePoint1 ).magnitude + ( linePoint2 - ( linePoint1 + lineVec1 ) ).magnitude || 
    //                    lineVec1.magnitude - 0.001f  > ( linePoint2 + lineVec2 - linePoint1 ).magnitude + ( linePoint2 + lineVec2 - ( linePoint1 + lineVec1 ) ).magnitude ||
    //                    ( linePoint1 == linePoint2 && linePoint1 + lineVec1 == linePoint2 + lineVec2 ) || ( linePoint1 == linePoint2 + lineVec2 && linePoint1 + lineVec1 == linePoint2 );
    //         }
    //         else if( line1Type == ArrayType.Ray && line2Type ==  ArrayType.Segment ) {
                
    //             Debug.Log( "signed angle: " + Vector3.SignedAngle( lineVec1, linePoint2 - linePoint1, normal ) );
                
    //             return Vector3.SignedAngle( lineVec1, linePoint2 - linePoint1, normal ) < 0.1f && Vector3.SignedAngle( lineVec1, linePoint2 - linePoint1, normal ) > -0.1f;
    //         }
            
    //         return false;
    //     }
    // }

    public static bool LineLineIntersect( out Vector3 intersection, Vector3 normal, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, ArrayType line1Type, ArrayType line2Type ) {

        Plane plane = new( normal, linePoint1 );

        Vector3 linePoint3 = plane.ClosestPointOnPlane( linePoint1 + lineVec1 );
        Vector3 linePoint4 = plane.ClosestPointOnPlane( linePoint2 + lineVec2 );

        // Debug.DrawRay( linePoint3, normal, Color.cyan, Time.deltaTime );
        // Debug.DrawRay( linePoint4, normal, Color.cyan, Time.deltaTime );

        linePoint1 = plane.ClosestPointOnPlane( linePoint1 );
        linePoint2 = plane.ClosestPointOnPlane( linePoint2 );

        // Debug.DrawRay( linePoint1, normal, Color.red, Time.deltaTime );
        // Debug.DrawRay( linePoint2, normal, Color.red, Time.deltaTime );

        lineVec1 = linePoint3 - linePoint1;
        lineVec2 = linePoint4 - linePoint2;

        // Debug.DrawRay( linePoint1, lineVec1, Color.blue, Time.deltaTime );
        // Debug.DrawRay( linePoint2, lineVec2, Color.blue, Time.deltaTime );

        Vector3 lineVec3 = linePoint2 - linePoint1;

        Vector3 crossVec1and2 = Vector3.Cross( lineVec1, lineVec2 );
        Vector3 crossVec3and2 = Vector3.Cross( lineVec3, lineVec2 );

        // float signedAngleVec1and2 = Vector3.SignedAngle( lineVec1, lineVec2, normal );

        // float planarFactor = Vector3.Dot( lineVec3, crossVec1and2 );

        // Debug.Log( "planarFactor: " + Mathf.Abs( planarFactor ) + " < " + maxPlanarFactor + "? " + ( Mathf.Abs( planarFactor ) < maxPlanarFactor ) );
        // Debug.Log( "crossVec1and2.sqrMagnitude: " + crossVec1and2.sqrMagnitude + " > " + 0.001f + "? " + ( crossVec1and2.sqrMagnitude > 0.001f ) );

        // Debug.Log( "signedAngleVec1and2: " + signedAngleVec1and2 );

        //is coplanar, and not parallel
        //if( Mathf.Abs( planarFactor ) < maxPlanarFactor && crossVec1and2.sqrMagnitude > 0.001f ) {
            // Debug.Log( "Linee non parallele" );
            float t = Vector3.Dot( crossVec3and2, crossVec1and2 ) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + ( lineVec1 * t );

            // Debug.Log( "t: " + t );

            bool isInsideLine1 = true, isInsideLine2 = true;

            switch( line1Type ) {
                case ArrayType.Line:    isInsideLine1 = true;
                                        break;
                
                case ArrayType.Ray:     // Debug.Log( "Vector3.Dot( lineVec1, intersection - linePoint1 ): " + Vector3.Dot( lineVec1, intersection - linePoint1 ) );
                                        if( linePoint1 == linePoint2 || linePoint1 == linePoint4 ) {
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

                                        // Debug.Log( "lineVec1.magnitude: " + lineVec1.magnitude );
                                        // Debug.Log( "( intersection - linePoint1 ).magnitude: " + ( intersection - linePoint1 ).magnitude );
                                        // Debug.Log( "( intersection - ( linePoint1 + lineVec1 ) ).magnitude: " + ( intersection - ( linePoint1 + lineVec1 ) ).magnitude );

    
                                        break;

            }

            // Debug.Log( ">>>isInsideLine1: " + isInsideLine1 );

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
                                        // Debug.Log( "lineVec2.magnitude: " + lineVec2.magnitude );
                                        // Debug.Log( "( intersection - linePoint2 ).magnitude: " + ( intersection - linePoint1 ).magnitude );
                                        // Debug.Log( "( intersection - ( linePoint2 + lineVec2 ) ).magnitude: " + ( intersection - ( linePoint2 + lineVec2 ) ).magnitude );

                                        
                                        break;

            }

            // Debug.Log( ">>>isInsideLine2: " + isInsideLine2 );

            if( !isInsideLine1 || !isInsideLine2 ) {
                intersection = Vector3.zero;
                return false;
            }
            else {
                return true;
            }          
    }

    public static ProceduralMesh GeneratePoligonalMesh( List<Vector3> points, bool clockwiseRotation, Vector3 up, Vector3 right, float textureHorLenght, float textureVertLenght ) {

        List<Edge> perimeterEdges = new();
        for( int i = 0; i < points.Count; i++ ) { 

            //Debug.Log( ">>> Point[ " + i + " ]: " + points[ i ] );

            if( i < points.Count - 1 ) {
                perimeterEdges.Add( new Edge( points[ i ], points[ i + 1 ], i, i + 1 ) );

                Debug.DrawLine( points[ i ], points[ i + 1 ], Color.blue, Time.deltaTime );
            }
            else {
                perimeterEdges.Add( new Edge( points[ i ], points[ 0 ], i, 0 ) );
                Debug.DrawLine( points[ i ], points[ 0 ], Color.green, Time.deltaTime );
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

                                // Debug.Log( "Verifico le intersezioni fra i " + i + " e j " + j + " con il segmento da " + edge.startingPointIndex + " a " + edge.endingPointIndex );

                                intersect = LineLineIntersect( out internalIntersectionPoint, Quaternion.AngleAxis( -90.0f, right ) * up, points[ i ], points[ j ] - points[ i ], edge.startingPoint, edge.endingPoint - edge.startingPoint, ArrayType.Segment, ArrayType.Segment );

                                if( intersect ) {
                                    
                                    //Debug.Log( "Il segmento dal punto i " + i + " al punto j " + j + " interseca la retta fra i punti " + edge.startingPointIndex + " e " + edge.endingPointIndex );

                                    // Debug.Log( ">>> internalIntersectionPoint: " + internalIntersectionPoint );

                                    // Debug.DrawLine( edge.startingPoint, edge.endingPoint, Color.yellow, Time.deltaTime );
                                    break;
                                }
                            }
                        }

                        //Debug.Log( ">>> intersect: " + intersect );
                        if( !intersect ) { 

                            float epsilonValue = 0.25f;
                            Vector3 epsilonPoint = points[ i ] + ( points[ j ] - points[ i ] ).normalized * epsilonValue;

                            int counter = 0;
                            foreach( Edge perimeterEdge in perimeterEdges ) {

                                int counterOld = counter;
                                Vector3 perimeterIntersectionPoint;
                                //Debug.Log( "Verifico raggio ( " + i + " - " + j + " ) con epsilon" + epsilonValue * 100 + "% con segmento perimetro ( " + perimeterEdge.startingPointIndex + " - " + perimeterEdge.endingPointIndex + " ) " );
                                counter += LineLineIntersect( out perimeterIntersectionPoint, Quaternion.AngleAxis( -90.0f, right ) * up, epsilonPoint, up, perimeterEdge.startingPoint, perimeterEdge.endingPoint - perimeterEdge.startingPoint, ArrayType.Ray, ArrayType.Segment ) ? 1 : 0;
                                
                                // if( counterOld != counter ) {
                                    //Debug.Log( "counter: " + counter );
                                    //Debug.Log( "Raggio epsilon ( " + i + " - " + j + " ) interseca segmento perimetro ( " + perimeterEdge.startingPointIndex + " - " + perimeterEdge.endingPointIndex + " ) " );
                                    //Debug.DrawRay( epsilonPoint, up * 10, Color.green, Time.deltaTime );
                                // }
                                // else {
                                //     //Debug.DrawRay( epsilonPoint, up * 10, Color.yellow, Time.deltaTime );
                                // }
                                //Debug.DrawRay( perimeterIntersectionPoint, Vector3.right, UnityEngine.Color.yellow, Time.deltaTime );
                            }
                            //Debug.Log( "final counter: " + counter );
                            bool outside = counter % 2 == 0;
                            
                            //Debug.Log( ">>> outside: " + outside );
                            if( !outside ) {

                                edges.Add( new Edge( points[ i ], points[ j ], i, j ) );
                                internalEdges.Add( new Edge( points[ i ], points[ j ], i, j ) );

                                //Debug.Log( "Aggiunto edge" );

                                Debug.DrawLine( edges[ ^1 ].startingPoint, edges[ ^1 ].endingPoint, Color.red, Time.deltaTime );


                                Vector3 halfWay = edges[ ^1 ].startingPoint + ( ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).normalized * ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).magnitude / 2 );
                                Vector3 arrow0 = halfWay + Quaternion.AngleAxis( 90.0f, Quaternion.AngleAxis( -90.0f, right ) * up ) * ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).normalized * 0.1f;
                                Vector3 arrow1 = halfWay + Quaternion.AngleAxis( 90.0f, Quaternion.AngleAxis( 90.0f, right ) * up ) * ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).normalized * 0.1f;
                                Vector3 arrow2 = halfWay + ( edges[ ^1 ].endingPoint - edges[ ^1 ].startingPoint ).normalized * 0.2f;

                                Debug.DrawLine( arrow0, arrow1, Color.cyan, Time.deltaTime );
                                Debug.DrawLine( arrow1, arrow2, Color.cyan, Time.deltaTime );
                                Debug.DrawLine( arrow2, arrow0, Color.cyan, Time.deltaTime );
                                Debug.DrawLine( edges[ ^1 ].startingPoint, edges[ ^1 ].endingPoint, Color.cyan, Time.deltaTime );
                                

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
            uv.Add( new Vector3( dotU / textureHorLenght, dotV / textureVertLenght ) );
        }

        Mesh poligon = new Mesh { vertices = points.ToArray(),
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

    public static Mesh GenerateSwitchNewLineGround( Side side, List<Vector3> points0, List<Vector3> points1, int start0, int end0, List<Vector3> points2, int minIndex1, int maxIndex1, Vector3 switchDir, bool clockwiseRotation, Vector2 textureTilting, float tunnelWidth, float centerWidth ) {
        
        Mesh groundMesh = new();

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uv = new();
        List<Vector3> normals = new();

        int verticesCounter = 0;
        float u, v;

        List<Vector3> groundLimitPoints0 = new() { points0[ 0 ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f * ( side == Side.Left ? -1 : 1 ) ) * switchDir.normalized * tunnelWidth ) };

        for( int i = 0; i <= start0; i++ ) {
            groundLimitPoints0.Add( points0[ i ] );
        }

        for( int i = maxIndex1; i >= minIndex1; i-- ) {
            groundLimitPoints0.Add( points2[ i ] );
        }

        groundLimitPoints0.Add( points2[ minIndex1 ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f * ( side == Side.Left ? -1 : 1 ) ) * switchDir.normalized * tunnelWidth ) );
        groundLimitPoints0.Add( groundLimitPoints0[ 0 ] + ( switchDir.normalized * ( groundLimitPoints0[ groundLimitPoints0.Count - 1 ] - groundLimitPoints0[ 0 ] ).magnitude / 2 ) );

        vertices.AddRange( groundLimitPoints0 );

        for( int i = 0; i < groundLimitPoints0.Count - 1; i++ ){
            
            if( i < groundLimitPoints0.Count - 2 ) {

                triangles.Add( verticesCounter + i );
                if( clockwiseRotation ) {
                    triangles.Add( verticesCounter + i + 1 );
                    triangles.Add( verticesCounter + groundLimitPoints0.Count - 1 );
                }
                else {
                    triangles.Add( verticesCounter + groundLimitPoints0.Count - 1 );
                    triangles.Add( verticesCounter + i + 1 );
                }


            }

            normals.Add( -Vector3.forward );
            u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints0[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints0[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );
        }
        
        normals.Add( -Vector3.forward );
        u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints0[ groundLimitPoints0.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints0[ groundLimitPoints0.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> up1 = new();
        List<Vector3> down1 = new();

        for( int i = start0; i < points0.Count - end0; i++ ) {
            up1.Add( points0[ i ] );
        }
        
        for( int i = minIndex1 + maxIndex1 - 1; i < points0.Count - 1 - start0 - end0 + minIndex1 + maxIndex1; i++ ) {
            down1.Add( points2[ i ] );
        }

        for( int i = 0; i < down1.Count - 1; i++ ){
            
            vertices.Add( down1[ i ] );
            vertices.Add( up1[ i ] );

            triangles.Add( verticesCounter + ( i * 2 ) );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            normals.Add( -Vector3.forward );
            normals.Add( -Vector3.forward );

            u = MeshGenerator.CalculateDistanceFromPointToLine( down1[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( down1[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            u = MeshGenerator.CalculateDistanceFromPointToLine( up1[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( up1[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );
        }

        vertices.Add( down1[ down1.Count - 1 ] );
        vertices.Add( up1[ up1.Count - 1 ] );

        normals.Add( -Vector3.forward );
        normals.Add( -Vector3.forward );

        u = MeshGenerator.CalculateDistanceFromPointToLine( down1[ down1.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( down1[ down1.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        u = MeshGenerator.CalculateDistanceFromPointToLine( up1[ up1.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( up1[ up1.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> groundLimitPoints2 = new();

        for( int i = 0; i <= end0; i++ ) {
            groundLimitPoints2.Add( points1[ i ] );
        }
        
        for( int i = ( points0.Count - 1 ) - start0 - end0 + ( 2 * maxIndex1 ) - 1; i > ( points0.Count - 1 ) - start0 - end0 + maxIndex1 - 1; i-- ) {
            groundLimitPoints2.Add( points2[ i ] );
        }

        for( int i = ( points0.Count - 1 ) - end0; i < points0.Count; i++ ) {
            groundLimitPoints2.Add( points0[ i ] );
        }

        groundLimitPoints2.Add( groundLimitPoints2[ 0 ] - ( switchDir.normalized * ( tunnelWidth + ( centerWidth / 2 ) ) ) );

        vertices.AddRange( groundLimitPoints2 );

        for( int i = 0; i < groundLimitPoints2.Count - 1; i++ ){

            triangles.Add( verticesCounter + i );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + i + 1 );
                triangles.Add( verticesCounter + groundLimitPoints2.Count - 1 );
            }
            else {
                triangles.Add( verticesCounter + groundLimitPoints2.Count - 1 );
                triangles.Add( verticesCounter + i + 1 );
            }

            normals.Add( -Vector3.forward );

            u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints2[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints2[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );
        }

        normals.Add( -Vector3.forward );

        u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints2[ groundLimitPoints2.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints2[ groundLimitPoints2.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> up3 = new();
        List<Vector3> down3 = new();

        for( int i = end0; i < points1.Count - start0; i++ ) {
            up3.Add( points1[ i ] );
        }
        
        for( int i = minIndex1 + ( 2 * ( maxIndex1 - 1 ) ) + points0.Count - start0 - end0 - 1; i < points2.Count - maxIndex1 + 2; i++ ) {
            down3.Add( points2[ i ] );
        }

        for( int i = 0; i < up3.Count - 1; i++ ){

            vertices.Add( down3[ i ] );
            vertices.Add( up3[ i ] );
            
            triangles.Add( verticesCounter + ( i * 2 ) );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            normals.Add( -Vector3.forward );
            normals.Add( -Vector3.forward );

            u = MeshGenerator.CalculateDistanceFromPointToLine( down3[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( down3[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            u = MeshGenerator.CalculateDistanceFromPointToLine( up3[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( up3[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );
        }

        vertices.Add( down3[ down3.Count - 1 ] );
        vertices.Add( up3[ up3.Count - 1 ] );

        normals.Add( -Vector3.forward );
        normals.Add( -Vector3.forward );

        u = MeshGenerator.CalculateDistanceFromPointToLine( down3[ down3.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( down3[ down3.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        u = MeshGenerator.CalculateDistanceFromPointToLine( up3[ up3.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( up3[ up3.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> groundLimitPoints4 = new();

        groundLimitPoints4.Add( points2[ 0 ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f * ( side == Side.Left ? -1 : 1 )  ) * switchDir.normalized * tunnelWidth ) );
        groundLimitPoints4.Add( points2[ 0 ] );

        for( int i = points2.Count - 1; i > ( points2.Count - 1 ) - maxIndex1 + minIndex1; i-- ) {
            groundLimitPoints4.Add( points2[ i ] );
        }

        for( int i = ( points1.Count - 1 ) - start0; i <= points1.Count - 1; i++ ) {
            groundLimitPoints4.Add( points1[ i ] );
        }

        groundLimitPoints4.Add( groundLimitPoints4[ groundLimitPoints4.Count - 1 ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f * ( side == Side.Left ? -1 : 1 ) ) * switchDir.normalized * tunnelWidth ) );
        groundLimitPoints4.Add( groundLimitPoints4[ groundLimitPoints4.Count - 1 ] - ( switchDir.normalized * ( groundLimitPoints4[ groundLimitPoints4.Count - 1 ] - groundLimitPoints4[ 0 ] ).magnitude / 2 ) );

        vertices.AddRange( groundLimitPoints4 );

        for( int i = 0; i < groundLimitPoints4.Count - 1; i++ ){

            triangles.Add( verticesCounter + i );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + i + 1 );
                triangles.Add( verticesCounter + groundLimitPoints4.Count - 1 );
            }
            else {
                triangles.Add( verticesCounter + groundLimitPoints4.Count - 1 );
                triangles.Add( verticesCounter + i + 1 );
            }

            normals.Add( -Vector3.forward );

            u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints4[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints4[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );
        }

        normals.Add( -Vector3.forward );

        u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints4[ groundLimitPoints4.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints4[ groundLimitPoints4.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> up5 = new();
        List<Vector3> down5 = new();

        up5.Add( groundLimitPoints0[ groundLimitPoints0.Count - 3 ] );
        up5.Add( groundLimitPoints4[ 2 ] );
        down5.Add( groundLimitPoints0[ groundLimitPoints0.Count - 2 ] );
        down5.Add( groundLimitPoints4[ 0 ] );

        for( int i = 0; i < up5.Count - 1; i++ ) {

            vertices.Add( down5[ i ] );
            vertices.Add( up5[ i ] );

            triangles.Add( verticesCounter + ( i * 2 ) );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            normals.Add( -Vector3.forward );
            normals.Add( -Vector3.forward );

            u = CalculateDistanceFromPointToLine( down5[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = CalculateDistanceFromPointToLine( down5[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            u = CalculateDistanceFromPointToLine( up5[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = CalculateDistanceFromPointToLine( up5[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );
        }

        vertices.Add( down5[ down5.Count - 1 ] );
        vertices.Add( up5[ up5.Count - 1 ] );

        normals.Add( -Vector3.forward );
        normals.Add( -Vector3.forward );

        u = CalculateDistanceFromPointToLine( down5[ down5.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = CalculateDistanceFromPointToLine( down5[ down5.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        u = CalculateDistanceFromPointToLine( up5[ up5.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = CalculateDistanceFromPointToLine( up5[ up5.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        /////////////////////

        groundMesh.vertices = vertices.ToArray();
        groundMesh.triangles = triangles.ToArray();
        groundMesh.normals = normals.ToArray();
        groundMesh.uv = uv.ToArray();

        return groundMesh;
    }

    public static float CalculateDistanceFromPointToLine(Vector3 point, Vector3 linePoint, Vector3 lineDirection)
    {
        // Calcola il vettore dalla retta al punto
        Vector3 pointToLine = point - linePoint;

        // Calcola la distanza utilizzando la formula
        float distance = Vector3.Cross( pointToLine, lineDirection ).magnitude;

        return distance;
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

    public static Wall CalculateWallsBaseLines( List<Vector3> curve, List<Vector3> controlPoints, float baseWidth, float sideHeight )
    {
        Wall walls = new() { leftDown = new List<Vector3>(),
                             rightDown = new List<Vector3>() };

        float alpha = Mathf.Atan( sideHeight / baseWidth ) * Mathf.Rad2Deg;
        float dirLength = Mathf.Sqrt( Mathf.Pow( sideHeight, 2 ) + Mathf.Pow( baseWidth, 2 ) );

        Vector3 curveDir = ( curve[ 1 ] - curve[ 0 ] ).normalized;
        Vector3 leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, 0.0f ) ).normalized * dirLength;
        leftDir = Quaternion.AngleAxis( alpha, curveDir ) * leftDir;
        Vector3 rightDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, 0.0f ) ).normalized * dirLength;
        rightDir = Quaternion.AngleAxis( -alpha, curveDir ) * rightDir;

        walls.leftDown.Add( curve[ 0 ] + leftDir );
        walls.rightDown.Add( curve[ 0 ] - rightDir );

        for( int i = 1; i < curve.Count - 1; i++ )
        {

            curveDir = ( curve[ i ] - curve[ i - 1 ] ).normalized;

            //Debug.DrawRay( curve[ 0 ], curveDir * 10, Color.cyan, 999 );

            leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, 0.0f ) ).normalized * dirLength;
            leftDir = Quaternion.AngleAxis( alpha, curveDir ) * leftDir;
            rightDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, 0.0f ) ).normalized * dirLength;
            rightDir = Quaternion.AngleAxis( -alpha, curveDir ) * rightDir;

            //Debug.DrawRay( curve[ i ], leftDir, Color.green, 999 );
            //Debug.DrawRay( curve[ i ], -rightDir, Color.red, 999 );

            walls.leftDown.Add( curve[ i ] + leftDir );
            walls.rightDown.Add( curve[ i ] - rightDir );
        }

        curveDir = ( curve[ curve.Count - 1 ] - curve[ curve.Count - 2 ] ).normalized;

        leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, 0.0f ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, 0.0f ) ).normalized * dirLength;
        leftDir = Quaternion.AngleAxis( alpha, curveDir ) * leftDir;
        rightDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, 0.0f ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, 0.0f ) ).normalized * dirLength;
        rightDir = Quaternion.AngleAxis( -alpha, curveDir ) * rightDir;

        walls.leftDown.Add( curve[ curve.Count - 1 ] + leftDir );
        walls.rightDown.Add( curve[ curve.Count - 1 ] - rightDir );

        return walls;
    }

    public static Wall CalculateWallsMeshesVertex( List<Vector3> curve, List<Vector3> controlPoints, float baseWidth, float sideHeight, float wallHeight )
    {
        Wall walls = new() { leftDown = new List<Vector3>(),
                             rightDown = new List<Vector3>(),
                             leftUp = new List<Vector3>(),
                             rightUp = new List<Vector3>() };

        float alpha = Mathf.Atan( sideHeight / baseWidth ) * Mathf.Rad2Deg;
        float dirLength = Mathf.Sqrt( Mathf.Pow( sideHeight, 2 ) + Mathf.Pow( baseWidth, 2 ) );

        Vector3 leftDir = Quaternion.Euler( alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, 0.0f ) ).normalized * dirLength;
        Vector3 rightDir = Quaternion.Euler( -alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, 0.0f ) ).normalized * dirLength;

        walls.leftDown.Add( curve[ 0 ] - leftDir );
        walls.leftUp.Add( new Vector3( walls.leftDown[ 0 ].x, walls.leftDown[ 0 ].y, walls.leftDown[ 0 ].z - wallHeight ) );
        walls.rightDown.Add( curve[ 0 ] + rightDir );
        walls.rightUp.Add( new Vector3( walls.rightDown[ 0 ].x, walls.rightDown[ 0 ].y, walls.rightDown[ 0 ].z - wallHeight ) );

        for( int i = 1; i < curve.Count - 1; i++ )
        {

            leftDir = Quaternion.Euler( alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, 0.0f ) ).normalized * dirLength;
            rightDir = Quaternion.Euler( -alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, 0.0f ) ).normalized * dirLength;

            walls.leftDown.Add( curve[ i ] - leftDir );
            walls.leftUp.Add( new Vector3( walls.leftDown[ i ].x, walls.leftDown[ i ].y, walls.leftDown[ i ].z - wallHeight ) );
            walls.rightDown.Add( curve[ i ] + rightDir );
            walls.rightUp.Add( new Vector3( walls.rightDown[ i ].x, walls.rightDown[ i ].y, walls.rightDown[ i ].z - wallHeight ) );
        }

        leftDir = Quaternion.Euler( alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, 0.0f ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, 0.0f ) ).normalized * dirLength;
        rightDir = Quaternion.Euler( -alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, 0.0f ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, 0.0f ) ).normalized * dirLength;

        walls.leftDown.Add( curve[ curve.Count - 1 ] - leftDir );
        walls.leftUp.Add( new Vector3( walls.leftDown[ curve.Count - 1 ].x, walls.leftDown[ curve.Count - 1 ].y, walls.leftDown[ curve.Count - 1 ].z - wallHeight ) );
        walls.rightDown.Add( curve[ curve.Count - 1 ] + rightDir );
        walls.rightUp.Add( new Vector3( walls.rightDown[ curve.Count - 1 ].x, walls.rightDown[ curve.Count - 1 ].y, walls.rightDown[ curve.Count - 1 ].z - wallHeight ) );

        return walls;
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

    public static ProceduralMesh GenerateExtrudedMesh( /*Orientation extrusionOrientation,*/ List<Vector3> profileVertices, float profileScale, List<Vector3> previousProfileVertices, List<Vector3> baseVertices, float horPosCorrection, float vertPosCorrection, bool clockwiseRotation, bool closeMesh, float textureHorLenght, float textureVertLenght, float verticalRotationCorrection, float smoothFactor ) {
        
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

                if( i > 0 ) {
                    Debug.DrawLine( vertices[ i * h ], vertices[ ( i - 1 ) * h ], Color.yellow, 9999 );
                }

                if( i == baseVertices.Count - 1 ) {
                    lastProfileVertices.Add( vertices[ i * h ] );
                }
            }

            float u = distancesHor[ i ] / textureHorLenght;
            uv[ i * h ] = new Vector2( u, 0 );

            // Il numero di dir orizontali e verticali è minore di 1 rispetto al nomero di vertici che stiamo ciclando, quindi per l'ultimo punto calcolo la normale usando la direzione precedente
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
                    vertices[ ( i * h ) + j + 1 ] = ( vertices[ ( i * h ) + j ] + Quaternion.Euler( 0.0f, 0.0f, alpha + verticalRotationCorrection ) * profileDirs[ j ] );
                    verticesStructure[ Orientation.Horizontal ][ j + 1 ].Add( vertices[ ( i * h ) + j + 1 ] );
                    verticesStructure[ Orientation.Vertical ][ i ].Add( vertices[ ( i * h ) + j + 1 ] );

                    if( i == baseVertices.Count - 1 ) {
                        lastProfileVertices.Add( vertices[ ( i * h ) + j + 1 ] );
                    }
                }

                float v = distancesVert[ j + 1 ] / textureVertLenght;
                uv[ ( i * h ) + j + 1 ] = new Vector2( u, v );

                int normalVertIndex = ( j + 1 ) < profileDirs.Count ? ( j + 1 ) : ( profileDirs.Count - 1 );
                normal = Vector3.Cross( Quaternion.Euler( 0.0f, 0.0f, alpha + verticalRotationCorrection ) * profileDirs[ normalVertIndex ], baseDirs[ normalHorIndex ] ).normalized;
                if( !clockwiseRotation ) {
                    normal = -normal;
                }
                normals[ ( i * h ) + j + 1 ] = normal;

                if( i > 0 ) {
                    int arrayIndex = ( 6 * ( ( ( i - 1 ) * ( h - 1 ) ) + j ) );
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

        return extrudedMesh;
    }

    public static Mesh GeneratePlanarMesh( List<Vector3> closedLine, Vector3 center, bool clockwiseRotation, Vector3 planeNormal, Vector3 textureDir, float textureHorLenght, float textureVertLenght ) {
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

            Vector3 r = ( perimeter[ i ] - center );
            float dotU = Vector3.Dot( r, dirU );
            float dotV = Vector3.Dot( r, dirV );
            uv[ i + 1 ] = new Vector3( dotU / textureHorLenght, dotV / textureVertLenght );
        }

        planarMesh.vertices = vertices.ToArray();
        planarMesh.triangles = triangles;
        planarMesh.uv = uv;
        planarMesh.RecalculateNormals();

        return planarMesh;
    }

    public static ProceduralMesh GeneratePlanarMesh( List<Vector3> line, Vector3[ , ] vertMatrix, bool clockwiseRotation,  bool closeMesh, bool centerTexture, float textureHorLenght, float textureVertLenght )
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
        Vector2[] uvs = new Vector2[ vertices.Length ];
        float u = 0.0f, vMin = 0.0f, vMax = 0.0f;
        for( int i = 0; i < curve.Count; i++ )
        {
            vertices[ i * 2 ] = vertMatrix[ 0, i ];
            vertices[ ( i * 2 ) + 1 ] = vertMatrix[ 1, i ];

            floorStructure[ Orientation.Vertical ].Add( i, new List<Vector3>{ vertMatrix[ 1, i ], vertMatrix[ 0, i ] } );

            // deltaLText rappresenta la distanza della curva che una singola ripetizione di texture deve coprire,
            // considerando la lunghezza dell'intera curva e dividendola per il numero di punti ho la 
            // distanza (costante) fra ogni punto deltaLCurve. In questo modo, dividendo deltaLCurve per deltaLText
            // ottengo quante volte la texture deve ripetersi per ogni segmento e moltiplico per i in modo da mappare la texture
            // sulla curva indipendentemente dalla lunghezza della stessa.
            
            if( i > 0 ) {
                u += ( float )( ( curve[ i ] - curve[ i - 1 ] ).magnitude / textureHorLenght );
            }
            vMax = ( float )( ( vertMatrix[ 1, i ] - vertMatrix[ 0, i ] ).magnitude / textureVertLenght );

            if( centerTexture ){
                vMin = -( vMax / 2 );
                vMax = -vMin;
            }

            uvs[ ( i * 2 ) ] = new Vector2( u, vMin );
            uvs[ ( i * 2 ) + 1 ] = new Vector2( u, vMax );

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
        }

        floorMesh.Clear();
        floorMesh.vertices = vertices;
        floorMesh.triangles = edges;
        floorMesh.uv = uvs;

        floorMesh.RecalculateNormals();

        ProceduralMesh toReturn = new() { mesh = floorMesh,
                                          lastProfileVertex = floorStructure[ Orientation.Vertical ][ floorStructure[ Orientation.Vertical ].Count - 1 ],
                                          verticesStructure = floorStructure };

        return toReturn;
    }
}
