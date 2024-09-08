using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class MaintenanceGenerator : MonoBehaviour
{
    public class TunnelNode {

        public int id;
        public Vector3 pos;
        public List<TunnelNode> connectedNodes = new();
        public bool isStarting = false;
        public Dictionary<int, Dictionary<string, Vector3>> intersectionsPoints = new();
    }

    public List<Utils.Shape> tunnelWallShape;

    public Vector2 tunnelLenghtRange = new() { x = 10000.0f, y = 50000.0f };

    public float tunnelWidth = 20.0f;
    public float tunnelIntersectionExtension = 5.0f;

    public float tunnelPointsDistance = 500.0f;
    public float tunnelPointsStartingDistance = 150.0f;
    public float tunnelPointsDistanceMultiplier = 1.5f;
    public float turnMaxDistance = 100.0f;
    public Utils.Texturing tunnelGroundTexturing;
    private int seed = 0;
    public bool ready = false;

    //private Dictionary<string, List<TunnelNode>> maintenanceTunnels = new();
    List<TunnelNode> tunnelNodes = new();

    public bool CheckTunnelIntersections( Vector3 a, Vector3 b ) {
        for( int i = 0; i < tunnelNodes.Count; i++ ) {
            foreach( TunnelNode connectedNode in tunnelNodes[ i ].connectedNodes ) {
                
                Vector3 intersection = new();
                if( connectedNode.id > i ) {
                    bool isIntersecting = MeshGenerator.LineLineIntersect( out intersection, -Vector3.forward, a, b - a, tunnelNodes[ i ].pos, connectedNode.pos - tunnelNodes[ i ].pos, ArrayType.Segment, ArrayType.Segment );

                    if( isIntersecting ) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void CalculateTunnelIntersection( TunnelNode node ) {
        Dictionary<int, Dictionary<string, Vector3>> points = new();
        Dictionary<int, Vector3> dirs = new();
        Dictionary<float, TunnelNode> connectedNodes = new();
        Dictionary<int, TunnelNode> orderedConnectedNodes = new();
        if( node.connectedNodes.Count > 2 ) {
            foreach( TunnelNode connectedNode in node.connectedNodes ) {
                Vector3 dir = connectedNode.pos - node.pos;
                connectedNodes.Add( Vector3.SignedAngle( Vector3.right, dir, -Vector3.forward ), connectedNode );
            }

            List<float> alphas = connectedNodes.Keys.ToList();
            alphas.Sort();

            foreach( float alpha in alphas ) {
                
                TunnelNode connectedNode = connectedNodes[ alpha ];

                Dictionary<string, Vector3> nodePoints = new();
                Vector3 dir = ( connectedNode.pos - node.pos ).normalized;
                dirs.Add( connectedNode.id, dir );
                nodePoints.Add( "R", node.pos + dir * ( tunnelWidth / 2 ) + Quaternion.AngleAxis( -90.0f, -Vector3.forward ) * dir * ( tunnelWidth / 2 ) );
                nodePoints.Add( "L", node.pos + dir * ( tunnelWidth / 2 ) + Quaternion.AngleAxis( 90.0f, -Vector3.forward ) * dir * ( tunnelWidth / 2 ) );

                Debug.DrawRay( nodePoints[ "R" ], Vector3.right, Color.red, 999 );
                Debug.DrawRay( nodePoints[ "R" ], Vector3.up, Color.red, 999 );

                Debug.DrawRay( nodePoints[ "L" ], Vector3.right, Color.green, 999 );
                Debug.DrawRay( nodePoints[ "L" ], Vector3.up, Color.green, 999 );

                Debug.DrawLine( nodePoints[ "R" ], nodePoints[ "L" ], Color.white, 999 );

                points.Add( connectedNode.id, nodePoints );
                orderedConnectedNodes.Add( connectedNode.id, connectedNode );
            }

            List<int> keys = points.Keys.ToList();
            Dictionary<int, Dictionary<string, Vector3>> intersections = new();
            for( int i = 0; i < points.Keys.Count; i++ ) {

                int key = keys[ i ];
                int previousKey = keys[ ^1 ];
                if( i > 0 ) {
                    previousKey = keys[ i - 1 ];
                }

                Vector3 d1 = orderedConnectedNodes[ key ].pos - node.pos;
                Debug.DrawRay( points[ key ][ "R" ], d1.normalized * 50, Color.red, 999 );
                Debug.DrawRay( points[ key ][ "L" ], d1.normalized * 50, Color.green, 999 );
                Vector3 d2 = orderedConnectedNodes[ previousKey ].pos - node.pos;
                Debug.DrawRay( points[ previousKey ][ "L" ], d2.normalized * 50, Color.green, 999 );
                Vector3 intersection = Vector3.zero;
                bool intersect = MeshGenerator.LineLineIntersect( out intersection, -Vector3.forward, points[ key ][ "R" ], d1.normalized, points[ previousKey ][ "L" ], d2.normalized, ArrayType.Ray, ArrayType.Ray );

                if( intersect ) {
                    // Debug.DrawRay( intersection, Vector3.right, Color.yellow, 999 );
                    // Debug.DrawRay( intersection, Vector3.up, Color.yellow, 999 );

                    if( intersections.ContainsKey( key ) ) {
                        Dictionary<string, Vector3> temp = intersections[ key ];
                        temp.Add( "R", intersection );
                        intersections[ key ] = temp;
                    }
                    else {
                        Dictionary<string, Vector3> intersectionKey = new() { { "R", intersection }, };
                        intersections.Add( key, intersectionKey );
                    }

                    if( intersections.ContainsKey( previousKey ) ) {
                        Dictionary<string, Vector3> temp = intersections[ previousKey ];
                        temp.Add( "L", intersection );
                        intersections[ previousKey ] = temp;
                    }
                    else {
                        Dictionary<string, Vector3> intersectionPreviousKey = new() { { "L", intersection }, };
                        intersections.Add( previousKey, intersectionPreviousKey );
                    }
                }
            }

            foreach( int key in intersections.Keys ) {
                if( points.ContainsKey( key ) ) {

                    if( intersections[ key ].ContainsKey( "R" ) ) {
                        points[ key ][ "R" ] = intersections[ key ][ "R" ];
                    }
                    if( intersections[ key ].ContainsKey( "L" ) ) {
                        points[ key ][ "L" ] = intersections[ key ][ "L" ];
                    }
                }
            }

            List<Vector3> groundPoints = new();
            for( int i = 0; i < points.Keys.Count; i++ ) {

                int key = keys[ i ];
                int previousKey = keys[ ^1 ];
                if( i > 0 ) {
                    previousKey = keys[ i - 1 ];
                }
                
                if( !groundPoints.Contains( points[ key ][ "R" ] ) ) {
                    groundPoints.Add( points[ key ][ "R" ] );
                }

                if( !groundPoints.Contains( points[ key ][ "L" ] ) ) {
                    groundPoints.Add( points[ key ][ "L" ] );
                }

                List<Vector3> wallPoints = new() { points[ key ][ "R" ] + dirs[ key ].normalized * this.tunnelIntersectionExtension,
                                                   points[ key ][ "R" ],
                                                   points[ previousKey ][ "L" ],
                                                   points[ previousKey ][ "L" ] + dirs[ previousKey ].normalized * this.tunnelIntersectionExtension };

                if( wallPoints[ 1 ] == wallPoints[ 2 ] ) {
                    wallPoints.RemoveAt( 1 ); 
                }

                InstantiateComposedExtrudedMesh( this.transform, "muro", wallPoints, this.tunnelWallShape, null, 180.0f, false, false, Vector2.zero );

                // for( int k = 1; k < wallPoints.Count; k++ ) {
                //     Debug.DrawLine( wallPoints[ k ], wallPoints[ k - 1 ], Color.cyan, 999 );
                // }
                
            }
            MetroGenerator.InstantiatePoligon( this.transform, "Pavimento nodo " + node.id, groundPoints, true, -Vector3.forward, Vector3.right, Vector2.zero, this.tunnelGroundTexturing );

            node.intersectionsPoints = intersections;
        }
    }

    public void GenerateMaintenance( MetroGenerator metroGenerator ) {

        this.ready = false;

        this.seed = metroGenerator.seed;
        Random.InitState( this.seed ); 

        int previousBetaCoeff = 0, betaCoeff;

        for( int i = 0; i < metroGenerator.maintenanceTunnelsStart.Count; i++ ) {
            TunnelNode firstNode = new() { id = tunnelNodes.Count,
                                           pos = metroGenerator.maintenanceTunnelsStart[ i ].Item1, 
                                           isStarting = true };
            Vector3 startDir = metroGenerator.maintenanceTunnelsStart[ i ].Item2;
            Vector3 startPos = metroGenerator.maintenanceTunnelsStart[ i ].Item1 + startDir.normalized * this.tunnelPointsStartingDistance;

            List<TunnelNode> currentTunnel = new();
            tunnelNodes.Add( firstNode );
            currentTunnel.Add( firstNode );

            if( metroGenerator.IsPointInsideProibitedArea( startPos, "" ) ) {
                continue;
            }

            TunnelNode startNode = new() { id = tunnelNodes.Count,
                                           pos = startPos,
                                           isStarting = true };
            startNode.connectedNodes.Add( firstNode );
            tunnelNodes[ ^1 ].connectedNodes.Add( startNode );
            tunnelNodes.Add( startNode );

            float alpha = Vector3.SignedAngle( Vector3.right, startDir, -Vector3.forward );

            Vector3 mainDir = Vector3.right;
            if( alpha >= 45.0f && alpha < 135.0f ) {
                mainDir = -Vector3.up;
            }
            else if( alpha >= 135.0f || alpha < -135.0f ) {
                mainDir = -Vector3.right;
            }
            else if( alpha < -45.0f && alpha >= -135.0f ) {
                mainDir = Vector3.up;
            }

            float lenghtElapsed = 0.0f;
            Vector3 dir = new( mainDir.x, mainDir.y, mainDir.z );

            float lenght = Random.Range( tunnelLenghtRange.x, tunnelLenghtRange.y );
            for( int k = 0; k < ( int )( lenght / tunnelPointsDistance ); k++ ) {

                if( lenghtElapsed > turnMaxDistance ) {
                    betaCoeff = Random.Range( -1, 2 );
                    while( betaCoeff != 0 && betaCoeff == -previousBetaCoeff ) {
                        betaCoeff = Random.Range( -1, 2 );
                    }
                    previousBetaCoeff = betaCoeff;
                    
                    dir = Quaternion.AngleAxis( 90.0f * betaCoeff, -Vector3.forward ) * mainDir;
                    lenghtElapsed = 0.0f;
                }


                Vector3 nextPoint = tunnelNodes[ ^1 ].pos + tunnelPointsDistance * dir;
                TunnelNode newNode = new(), forcedNode = null;
                foreach( TunnelNode tunnelNode in this.tunnelNodes ) {

                    Vector3 nextDir = tunnelNode.pos - nextPoint;
                    if( !tunnelNode.isStarting && !currentTunnel.Contains( tunnelNode ) && nextDir.magnitude < tunnelPointsDistance * this.tunnelPointsDistanceMultiplier && !metroGenerator.IsSegmentIntersectingProibitedArea( tunnelNodes[ ^1 ].pos, tunnelNode.pos, "" ) && !CheckTunnelIntersections( tunnelNodes[ ^1 ].pos, tunnelNode.pos ) ) {
                        forcedNode = tunnelNode;
                        break;
                    }
                }

                if( forcedNode == null ) {
                    if( metroGenerator.IsSegmentIntersectingProibitedArea( tunnelNodes[ ^1 ].pos, nextPoint, "" ) ) {
                        break;
                    }
                    else if( CheckTunnelIntersections( tunnelNodes[ ^1 ].pos, nextPoint ) ) {
                        break;
                    }
                }

                lenghtElapsed += tunnelPointsDistance;

                if( forcedNode == null ) {
                    newNode.id = tunnelNodes.Count;
                    newNode.pos = nextPoint;
                    newNode.connectedNodes.Add( tunnelNodes[ ^1 ] );

                    tunnelNodes[ ^1 ].connectedNodes.Add( newNode );
                    tunnelNodes.Add( newNode );
                    currentTunnel.Add( newNode );
                }
                else{
                    forcedNode.connectedNodes.Add( tunnelNodes[ ^1 ] );
                    tunnelNodes[ ^1 ].connectedNodes.Add( forcedNode );

                    break;
                }
            }
        }

        foreach( TunnelNode node in tunnelNodes ) {
            CalculateTunnelIntersection( node );
        }

        this.ready = true;
    }

    private List<MeshGenerator.ProceduralMesh> InstantiateComposedExtrudedMesh( Transform parent, string gameObjName, List<Vector3> baseLine, List<Utils.Shape> partialShapes, List<int> partialShapesIndexes, float profileRotationCorrection, bool clockwiseRotation, bool closeMesh, Vector3 uvOffset ) {

        List<MeshGenerator.ProceduralMesh> newMeshes = new();
        MeshGenerator.ProceduralMesh extrudedMesh = new();

        for( int i = 0; i < partialShapes.Count; i++ ) {

            if( i > 0 ) {
                int verticesStructuresLastIndex = newMeshes[ i - 1 ].verticesStructure[ Orientation.Horizontal ].Count - 1;
                baseLine = newMeshes[ i - 1 ].verticesStructure[ Orientation.Horizontal ][ verticesStructuresLastIndex ];

                if( closeMesh ) {
                    baseLine.RemoveAt( baseLine.Count - 1 );
                }

                if( partialShapes[ i ].texturing.materials == partialShapes[ i - 1 ].texturing.materials ) {
                    uvOffset = extrudedMesh.uvMax;
                }
            }

            extrudedMesh = MeshGenerator.GenerateExtrudedMesh( partialShapes[ i ].partialProfile, partialShapes[ i ].scale, null, baseLine, partialShapes[ i ].positionCorrection.x, partialShapes[ i ].positionCorrection.y, clockwiseRotation, closeMesh, partialShapes[ i ].texturing.tiling, uvOffset, profileRotationCorrection, partialShapes[ i ].smoothFactor );
            extrudedMesh.mesh.name = "Procedural Extruded Mesh";

            if( ( partialShapesIndexes != null && partialShapesIndexes.Contains( i ) ) || partialShapesIndexes == null ) {

                GameObject extrudedGameObj = new( gameObjName + " - " + partialShapes[ i ].name );
                extrudedGameObj.transform.parent = parent;
                extrudedGameObj.transform.position = Vector3.zero;
                extrudedGameObj.AddComponent<MeshFilter>();
                extrudedGameObj.AddComponent<MeshRenderer>();
                extrudedGameObj.GetComponent<MeshFilter>().sharedMesh = extrudedMesh.mesh;
                extrudedGameObj.GetComponent<MeshRenderer>().SetMaterials( partialShapes[ i ].texturing.materials );

                extrudedMesh.gameObj = extrudedGameObj;
            }

            newMeshes.Add( extrudedMesh );
        }

        return newMeshes;
    }

    private void OnDrawGizmos() {
        if( tunnelNodes.Count > 0 ) {
            for( int i = 0; i < tunnelNodes.Count; i++ ) {
                foreach( TunnelNode connectedNode in tunnelNodes[ i ].connectedNodes ) {
                    Debug.DrawLine( tunnelNodes[ i ].pos, connectedNode.pos, Color.blue, Time.deltaTime );
                }
            }
        }
    }
}
